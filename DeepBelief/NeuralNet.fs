namespace DeepBelief

module NeuralNet =

    open System
    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions
    open Utils

    /// precision for calculating the derivatives
    let prc = 1e-6f

    type NnetProperties = {
        Weights : Matrix list
        Activations : (float32 -> float32) list
    }

    /// returns list of (out, out') vectors per layer
    let feedForward (netProps : NnetProperties) input = 
        List.fold 
            (fun (os : (Vector * Vector) list) (W, f) -> 
                let prevLayerOutput = 
                    match os.IsEmpty with
                    | true -> input
                    | _    -> fst (os.Head)
                let prevOut = prependForBias prevLayerOutput
                let layerInput = prevOut |> multiplyVectorByMatrix W
                (layerInput |> Array.map f, 
                 layerInput |> Array.map (derivative prc f)) :: os) 
          [] (List.zip netProps.Weights netProps.Activations)

    /// matlab like pointwise multiply
    let (.*) (v1 : Vector) (v2 : Vector) = 
        let n = Array.length v1
        Array.init n (fun i -> v1.[i] * v2.[i])

    /// computes the error signals per layer
    /// starting at output layer towards first hidden layer
    let errorSignals (Ws : Matrix list) layeroutputs (target : Vector) = 
        let trp = fun W -> Some(transpose W)

        // need weights and layer outputs in reverse order, 
        // e.g starting from output layer
        let weightsAndOutputs = 
            let transposed = Ws |> List.tail |> List.map trp |> List.rev
            List.zip (None :: transposed) layeroutputs

        List.fold (fun prevDs ((W : Matrix option), (o, o')) -> 
            match W with
            | None    -> (o' .* (subtractVectors target o)) :: prevDs 
            | Some(W) -> let ds = prevDs.Head
                         (o' .* ((multiplyVectorByMatrix W ds)).[1..]) :: prevDs) 
          [] weightsAndOutputs

    /// computes a list of gradients matrices
    let gradients (Ws : Matrix list) layeroutputs input target = 
        let actualOuts = 
            layeroutputs |> List.unzip |> fst |> List.tail |> List.rev
        let signals = errorSignals Ws layeroutputs target
        (input :: actualOuts, signals) 
            ||> List.zip 
            |> List.map (fun (zs, ds) -> outerProduct ds (prependForBias zs))

    let eta = 0.8f
    let alpha = 0.25f

    /// updates the weights matrices with the given deltas 
    /// of timesteps (t) and (t-1)
    /// returns the new weights matrices
    let updateWeights Ws (Gs : Matrix list) (prevDs : Matrix list) = 
        (List.zip3 Ws Gs prevDs) 
            |> List.map (fun (W, G, prevD) ->
                let dW = addMatrices (multiplyMatrixByScalar eta G) (multiplyMatrixByScalar alpha prevD)
                addMatrices W dW, dW)

    /// for each weight matrix builds another matrix with same dimension
    /// initialized with 0.0
    let initDeltaWeights (Ws : Matrix list) = 
        Ws |> List.map (fun W -> initGaussianWeights (height W) (width W))

    let step netProps prevDs input target = 
        let layeroutputs = feedForward netProps input
        let Gs = gradients netProps.Weights layeroutputs input target
        (updateWeights netProps.Weights Gs prevDs)

    let nnetTrain (rnd : AbstractRandomNumberGenerator) props samples epochs = 
        let count = samples |> Array.length
        let Ws, fs = props.Weights, props.Activations
        let rec loop Ws Ds i =
            match i < (epochs * count) with
            | true -> 
                let input, target = samples.[rnd.Next(count)]
                let netProps = { Weights = Ws; Activations = fs }
                let ws, ds = List.unzip (step netProps Ds input target)
                loop ws ds (i + 1)
            | _    -> Ws
        let Ws' = loop Ws (initDeltaWeights Ws) 0
        { props with Weights = Ws' }

    let netoutput (layeroutputs : ('a * 'a) list) = fst (layeroutputs.Head)

    let computeResults rnd netProps trainingSet testSet epochs = 
        let netProps' = nnetTrain rnd netProps trainingSet epochs
        let setSize = trainingSet.Length

        let testError = 
            testSet 
            |> Array.fold (fun E (x, t) -> 
                let outs = feedForward netProps' x
                let En = error t (netoutput outs)
                E + En) 0.0f

        testError / (float32 setSize)