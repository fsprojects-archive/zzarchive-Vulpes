namespace Backproagation

module SupervisedLearning =

    open System
    open Common.Analytics
    open Common
    open Backpropagation.Parameters

    type LayerOutputs = LayerOutputs of HiddenUnits list with
        member this.Result = 
            match this with LayerOutputs outputs -> outputs.Head 
            |> fun (HiddenUnits hiddenUnits) -> hiddenUnits 
            |> Array.map (fun (HiddenUnit hiddenUnit) -> fst hiddenUnit |> NeuralNet.Signal) 
            |> NeuralNet.Output

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
                |> List.map (fun (ErrorSignalsAndHiddenUnits (errorSignals, hiddenUnits)) -> errorSignals)
        member network.Gradients (LayerOutputs layerOutputs) (input : VisibleUnits) target =
            let layerValues (VisibleUnits visibleUnits) (outputs : HiddenUnits list) =
                let visibleUnitValues = visibleUnits |> Array.map (fun (VisibleUnit value) -> value) |> Vector
                let hiddenUnitValues = outputs |> List.rev |> List.map (fun (HiddenUnits hiddenUnits) -> hiddenUnits |> Array.map (fun (HiddenUnit (value, derivative)) -> value) |> Vector)
                visibleUnitValues :: hiddenUnitValues
            let signals = layerValues input layerOutputs
            let errorSignals = 
                (network.ComputeErrorSignals (LayerOutputs layerOutputs) target)
                |> List.map (fun (ErrorSignals e) -> Array.map(fun (ErrorSignal errorSignal) ->  errorSignal) e |> Vector)
            List.zip errorSignals signals |> List.map (fun (errorSignal, signal) -> errorSignal * signal |> WeightGradients)
        member network.InitialWeightChanges = network.Layers |> List.map (fun layer -> layer.Weights |> fun (WeightsAndBiases weightsAndBiases) -> Array2D.init weightsAndBiases.Height weightsAndBiases.Width (fun i j -> 0.0f) |> Matrix |> WeightChanges)
        member network.Update (input : VisibleUnits) target (previousWeightChanges : WeightChanges list) =
            let layerOutputs = network.FeedForward input
            let gradients = network.Gradients layerOutputs input target
            let currentWeightChanges = List.zip previousWeightChanges gradients |> List.map (fun (p, g) -> p.NextChanges network.Parameters.LearningRate network.Parameters.Momentum g p)
            ({
                Parameters = network.Parameters;
                Layers = List.zip network.Layers currentWeightChanges |> List.map (fun (layer, changes) -> { Weights = layer.Weights.Update changes; Activation = layer.Activation })
            }, currentWeightChanges)
        member network.Train (NeuralNet.TrainingSet trainingSet) (rnd : Random) =
            let previousWeightChanges = network.InitialWeightChanges
            trainingSet |> List.fold (fun (net, previousWeightChanges) trainingExample -> net.Update trainingExample.VisibleUnits trainingExample.Target previousWeightChanges) (network, network.InitialWeightChanges) |> fst

    let nnetTrain (rnd : Random) (network : BackPropagationNetwork) samples = 
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
