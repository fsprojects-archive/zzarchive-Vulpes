namespace DeepBelief

module NeuralNet =

    open System
    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions
    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic

    /// precision for calculating the derivatives
    let prc = 1e-6

    let sigmoid x = 1.0 / (1.0 + exp(-x))

    let prepend value (vec : Vector<float>) = 
        vector [ yield! value :: (vec |> Vector.toList) ]

    let prependForBias : Vector<float> -> Vector<float> = prepend 1.0

    let layer f (weights : Matrix<float>) (inputs : Vector<float>) =
        (weights * inputs) |> Vector.map f |> prependForBias

    type NnetProperties = {
        Weights : Matrix<float> list
        Activations : (float -> float) list
    }

    /// build the network by means of function composition
    let twoLayerNetwork (wmd : Matrix<float>) (wkm : Matrix<float>) : (Vector<float> -> Vector<float>) = 
        layer sigmoid wmd >> layer sigmoid wkm

    /// shield the user from handling the bias
    let compute (wmd : Matrix<float>) (wkm : Matrix<float>) (input : Vector<float>) =
        (twoLayerNetwork wmd wkm <| prependForBias input).[ 1 .. ]

    /// compute the derivative of a function, midpoint rule
    let derivative eps f = 
        fun x -> ((f (x + eps/2.0) - f (x - eps/2.0)) / eps)

    /// returns list of (out, out') vectors per layer
    let feedforward (netProps : NnetProperties) input = 
        List.fold 
            (fun (os : (Vector<_> * Vector<_>) list) (W, f) -> 
                let prevLayerOutput = 
                    match os.IsEmpty with
                    | true -> input
                    | _    -> fst (os.Head)
                let prevOut = prependForBias prevLayerOutput
                let layerInput = W * prevOut
                (layerInput |> Vector.map f, 
                 layerInput |> Vector.map (derivative prc f)) :: os) 
          [] (List.zip netProps.Weights netProps.Activations)

    let network (args : (Matrix<float> * (float -> float)) list) = 
        let rec loop ls accfun =
            match ls with
            | [] -> accfun
            | (weights,func) :: rest 
                 -> loop rest (accfun >> layer func weights)
        fun xs -> (loop args id) xs

    /// matlab like pointwise multiply
    let (.*) (a : Vector<float>) (b : Vector<float>) = 
        a.PointwiseMultiply(b)

    /// computes the error signals per layer
    /// starting at output layer towards first hidden layer
    let errorSignals (Ws : Matrix<_> list) layeroutputs (target : Vector<float>) = 
        let trp = fun (W : Matrix<_>) -> Some(W.Transpose())

        // need weights and layer outputs in reverse order, 
        // e.g starting from output layer
        let weightsAndOutputs = 
            let transposed = Ws |> List.tail |> List.map trp |> List.rev
            List.zip (None :: transposed) layeroutputs

        List.fold (fun prevDs ((W : Matrix<_> option), (o, o')) -> 
            match W with
            | None    -> (o' .* (target - o)) :: prevDs 
            | Some(W) -> let ds = prevDs.Head
                         (o' .* ((W * ds)).[1..]) :: prevDs) 
          [] weightsAndOutputs

    /// computes a list of gradients matrices
    let gradients (Ws : Matrix<_> list) layeroutputs input target = 
        let actualOuts = 
            layeroutputs |> List.unzip |> fst |> List.tail |> List.rev
        let signals = errorSignals Ws layeroutputs target
        (input :: actualOuts, signals) 
            ||> List.zip 
            |> List.map (fun (zs, ds) -> 
                ds.OuterProduct(prependForBias zs))

    let eta = 0.8
    let alpha = 0.25

    /// updates the weights matrices with the given deltas 
    /// of timesteps (t) and (t-1)
    /// returns the new weights matrices
    let updateWeights Ws (Gs : Matrix<_> list) (prevDs : Matrix<_> list) = 
        (List.zip3 Ws Gs prevDs) 
            |> List.map (fun (W, G, prevD) ->
                let dW = eta * G + (alpha * prevD)
                W + dW, dW)

    /// for each weight matrix builds another matrix with same dimension
    /// initialized with 0.0
    let initDeltaWeights (Ws : Matrix<_> list) = 
        Ws |> List.map (fun W -> 
            DenseMatrix.Create(W.RowCount, W.ColumnCount, fun _ _ -> 0.0) :> Matrix<float>)

    let step netProps prevDs input target = 
        let layeroutputs = feedforward netProps input
        let Gs = gradients netProps.Weights layeroutputs input target
        (updateWeights netProps.Weights Gs prevDs)

    let mersenne = new MersenneTwister()

    let nnetTrain props samples epochs = 
        let count = samples |> Array.length
        let rnd = new MersenneTwister()
        let Ws, fs = props.Weights, props.Activations
        let rec loop Ws Ds i =
            match i < (epochs * count) with
            | true -> 
                let inp, trg = samples.[rnd.Next(count)]
                let netProps = { Weights = Ws; Activations = fs }
                let ws, ds = List.unzip (step netProps Ds inp trg)
                loop ws ds (i + 1)
            | _    -> Ws
        let Ws' = loop Ws (initDeltaWeights Ws) 0
        { props with Weights = Ws' }

    type Rbm(nh, nv) =
        let nHidden = nh
        let nVisible = nv
        let mersenne = new MersenneTwister()
        let hBias = Array.zeroCreate nHidden |> DenseVector.OfEnumerable
        let vBias = Array.zeroCreate nVisible |> DenseVector.OfEnumerable
        let bound = 4.0 * Math.Sqrt (6.0 / ((float)nHidden + (float)nVisible))
        let uniform = new ContinuousUniform(-bound, bound, RandomSource = mersenne)
        let initialW = Array2D.init nVisible nHidden (fun _ _ -> uniform.Sample()) |> DenseMatrix.OfArray
        member x.W = initialW

        member x.PropUp (vis:Vector) =
            let product = x.W.Multiply vis + hBias
            product |> Vector.map sigmoid
            
        member x.PropDown (hid:Vector) =
            let product = x.W.LeftMultiply hid + vBias
            product |> Vector.map sigmoid
