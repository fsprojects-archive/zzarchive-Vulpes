namespace Backproagation

module SupervisedLearning =

    open System
    open Common.Analytics
    open Backpropagation.Parameters

    type NnetInput = NnetInput of Matrix

    type NnetOutput = NnetOutput of Matrix

    type LayerOutputs = LayerOutputs of HiddenUnits list

    type ErrorSignalsAndHiddenUnits = ErrorSignalsAndHiddenUnits of ErrorSignals * HiddenUnits with
        member this.ErrorSignals = match this with ErrorSignalsAndHiddenUnits (errorSignals, hiddenUnits) -> errorSignals
        member this.HiddenUnits = match this with ErrorSignalsAndHiddenUnits (errorSignals, hiddenUnits) -> hiddenUnits

    type BackPropagationNetwork with
        member network.FeedForward (input : VisibleUnits) = 
            let generateNextLayerOfSignals (outputs : HiddenUnits list) layer =
                (layer.Weights * outputs.Head |> layer.Activation.GenerateHiddenUnits) :: outputs
            let firstLayer = network.Layers.Head
            let firstSetOfHiddenUnits = firstLayer.Weights * input |> firstLayer.Activation.GenerateHiddenUnits
            List.fold generateNextLayerOfSignals [firstSetOfHiddenUnits] network.Layers.Tail |> LayerOutputs
        member network.ComputeErrorSignals (LayerOutputs layerOutputs) target =
            let topLevel = ErrorSignalsAndHiddenUnits (layerOutputs.Head .* (target - layerOutputs.Head), layerOutputs.Head)
            let generatePreviousErrorSignals (errorSignalAndHiddenUnitLayers : ErrorSignalsAndHiddenUnits list) layer =
                let errorSignalsAndHiddenUnits = errorSignalAndHiddenUnitLayers.Head
                let error = layer.Weights * errorSignalsAndHiddenUnits.ErrorSignals
                ErrorSignalsAndHiddenUnits (errorSignalsAndHiddenUnits.HiddenUnits .* error, errorSignalsAndHiddenUnits.HiddenUnits) :: errorSignalAndHiddenUnitLayers
            List.fold generatePreviousErrorSignals [topLevel] (network.Layers |> List.rev)
        member network.Gradients (LayerOutputs layerOutputs) (input : VisibleUnits) target =
            let layerValues (VisibleUnits visibleUnits) (outputs : HiddenUnits list) =
                let visibleUnitValues = visibleUnits |> Array.map (fun (VisibleUnit value) -> value) |> Vector
                let hiddenUnitValues = outputs |> List.rev |> List.map (Array.map (fun (HiddenUnit (value, derivative)) -> value) |> Vector)
                visibleUnitValues :: hiddenUnitValues
            let signals = layerValues input layerOutputs
            0

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
