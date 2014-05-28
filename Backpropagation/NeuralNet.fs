namespace Backproagation

module NeuralNet =

    open System
    open Common.Analytics
    open Backpropagation.Parameters

    type NnetInput = NnetInput of Matrix

    type NnetOutput = NnetOutput of Matrix

    type SupervisedLearning = SupervisedLearning of (NnetInput -> NnetOutput)

    /// returns list of (out, out') vectors per layer
    // Taken from Reto Matter's blog, http://retomatter.blogspot.ch/2013/01/functional-feed-forward-neural-networks.html
    let feedForward (network : BackPropagationNetwork) input = 
        List.fold 
            (fun (os : Signals list) layer -> 
                let prevLayerOutput = 
                    match os.IsEmpty with
                    | true -> input
                    | _    -> os.Head
                let layerInput = layer.Weight * prevLayerOutput.ValuesPrependedForBias
                (layerInput |> Array.map (fst f), 
                 layerInput |> Array.map (fun x -> (snd f) (x |> fst f) x)) :: os) 
          [] network.Layers

    /// computes the error signals per layer
    /// starting at output layer towards first hidden layer
    let cpuErrorSignals (network : BackPropagationNetwork) layeroutputs (target : Vector) = 
        let Ws = network.Layers |> List.map (fun layer -> layer.Weight)
        let trp = fun W -> Some(transpose W)

        // need weights and layer outputs in reverse order, 
        // e.g starting from output layer
        let weightsAndOutputs = 
            let transposed = Ws |> List.tail |> List.map trp |> List.rev
            List.zip (None :: transposed) layeroutputs

        List.fold (fun prevDs ((W : Matrix option), (o, o')) -> 
            match W with
            | None    -> (o' .* (subtractVectors target o)) :: prevDs 
            | Some(W) -> (o' .* ((multiplyVectorByMatrix W prevDs.Head)).[1..]) :: prevDs) 
          [] weightsAndOutputs

    /// computes a list of gradients matrices
    let cpuGradients (network : BackPropagationNetwork) layeroutputs input target = 
        let actualOuts = layeroutputs |> List.unzip |> fst |> List.tail |> List.rev
        let signals = cpuErrorSignals network layeroutputs target
        (input :: actualOuts, signals) 
            ||> List.zip 
            |> List.map (fun (zs, ds) -> outerProduct ds (prependForBias zs))

    /// updates the weights matrices with the given deltas 
    /// of timesteps (t) and (t-1)
    /// returns the new weights matrices
    let updateWeights (network : BackPropagationNetwork) (Gs : Matrix list) (prevDs : Matrix list) (parameters : BackPropagationParameters) = 
        let Ws = network.Layers |> List.map (fun layer -> layer.Weight)
        (List.zip3 Ws Gs prevDs) 
            |> List.map (fun (W, G, prevD) ->
                let dW = addMatrices (multiplyMatrixByScalar (value parameters.LearningRate) G) (multiplyMatrixByScalar (value parameters.Momentum) prevD)
                addMatrices W dW, dW)

    /// for each weight matrix builds another matrix with same dimension
    /// initialized with 0.0
    let initDeltaWeights (Ws : Matrix list) = 
        Ws |> List.map (fun W -> initGaussianWeights (height W) (width W))

    let initZeroWeights (Ws : Matrix list) = 
        Ws |> List.map (fun W -> Array2D.zeroCreate (height W) (width W))

    let step (network : BackPropagationNetwork) prevDs input target parameters = 
        let layeroutputs = feedForward network input
        let Gs = cpuGradients network layeroutputs input target
        (updateWeights network Gs prevDs parameters)

    let nnetTrain (rnd : Random) (network : BackPropagationNetwork) samples (parameters : BackPropagationParameters) = 
        let count = samples |> Array.length
        let Ws = network.Layers |> List.map (fun layer -> layer.Weight)
        let fs = network.Layers |> List.map (fun layer -> layer.Activation)
        let epochs = value parameters.Epochs
        let rec loop Ws Ds i =
            match i < (epochs * count) with
            | true -> 
                let index = rnd.Next(count)
                let input, target = samples.[index]
                let netProps = { Parameters = parameters; Layers = List.zip Ws fs |> List.map(fun (w, f) -> { Weight = w; Activation = f }) }
                let ws, ds = List.unzip (step netProps Ds input target parameters)
                loop ws ds (i + 1)
            | _    -> (Ws, Ds)
        let Wsf = loop Ws (initZeroWeights Ws) 0
        { Parameters = parameters; Layers = List.zip (fst Wsf) fs |> List.map(fun (w, f) -> { Weight = w; Activation = f }) }

    let netoutput (layeroutputs : ('a * 'a) list) = fst (layeroutputs.Head)

    let cpuComputeNnetResults netProps trainingSet testSet rnd parameters = 
        let netProps' = nnetTrain rnd netProps trainingSet parameters
        let cpuOutput = testSet |> Array.map (fun (x, t) -> netoutput (feedForward netProps' x))
        cpuOutput
