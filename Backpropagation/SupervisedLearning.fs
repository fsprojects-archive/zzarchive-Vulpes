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
        member network.Read (NeuralNet.Input input) =
            let readLayer (signals : NeuralNet.Signal[]) layer =
                (layer.Weights * signals |> layer.Activation.GenerateSignals)
            network.Layers 
            |> List.fold (fun signals layer -> readLayer signals layer) input
            |> NeuralNet.Output
        member network.Train (NeuralNet.TrainingSet trainingSet) (rnd : Random) =
            let gradients (LayerOutputs layerOutputs) (input : VisibleUnits) target =
                let computeErrorSignals =
                    let topLevel = ErrorSignalsAndHiddenUnits (layerOutputs.Head .* (target - layerOutputs.Head), layerOutputs.Head)
                    let generatePreviousErrorSignals (errorSignalAndHiddenUnitLayers : ErrorSignalsAndHiddenUnits list) layer =
                        let errorSignalsAndHiddenUnits = errorSignalAndHiddenUnitLayers.Head
                        let error = layer.Weights * errorSignalsAndHiddenUnits.ErrorSignals
                        ErrorSignalsAndHiddenUnits (errorSignalsAndHiddenUnits.HiddenUnits .* error, errorSignalsAndHiddenUnits.HiddenUnits) :: errorSignalAndHiddenUnitLayers
                    List.fold generatePreviousErrorSignals [topLevel] (network.Layers |> List.rev)
                        |> List.map (fun (ErrorSignalsAndHiddenUnits (errorSignals, hiddenUnits)) -> errorSignals)

                let layerValues (VisibleUnits visibleUnits) (outputs : HiddenUnits list) =
                    let visibleUnitValues = visibleUnits |> Array.map (fun (VisibleUnit value) -> value) |> Vector
                    let hiddenUnitValues = outputs |> List.rev |> List.map (fun (HiddenUnits hiddenUnits) -> hiddenUnits |> Array.map (fun (HiddenUnit (value, derivative)) -> value) |> Vector)
                    visibleUnitValues :: hiddenUnitValues
                let signals = layerValues input layerOutputs
                let errorSignals = 
                    computeErrorSignals
                    |> List.map (fun (ErrorSignals e) -> Array.map(fun (ErrorSignal errorSignal) ->  errorSignal) e |> Vector)
                List.zip errorSignals signals |> List.map (fun (errorSignal, signal) -> errorSignal * signal |> WeightGradients)

            let initialWeightChanges = network.Layers |> List.map (fun layer -> layer.Weights |> fun (WeightsAndBiases weightsAndBiases) -> Array2D.init weightsAndBiases.Height weightsAndBiases.Width (fun i j -> 0.0f) |> Matrix |> WeightChanges)
            let updateWeights (input : VisibleUnits) target (previousWeightChanges : WeightChanges list) =
                let feedForward (input : VisibleUnits) = 
                    let generateNextLayerOfSignals (outputs : HiddenUnits list) layer =
                        (layer.Weights * outputs.Head |> layer.Activation.GenerateHiddenUnits) :: outputs
                    let firstLayer = network.Layers.Head
                    let firstSetOfHiddenUnits = firstLayer.Weights * input |> firstLayer.Activation.GenerateHiddenUnits
                    List.fold generateNextLayerOfSignals [firstSetOfHiddenUnits] network.Layers.Tail |> LayerOutputs

                let layerOutputs = feedForward input
                let gradients = gradients layerOutputs input target
                let currentWeightChanges = List.zip previousWeightChanges gradients |> List.map (fun (p, g) -> p.NextChanges network.Parameters.LearningRate network.Parameters.Momentum g)
                {
                    Parameters = network.Parameters;
                    Layers = List.zip network.Layers currentWeightChanges |> List.map (fun (layer, changes) -> { Weights = layer.Weights.Update changes; Activation = layer.Activation })
                }

            let n = network.Parameters.Epochs.NumberOfSteps (NeuralNet.TrainingSet trainingSet)
            let rec loop previousWeightChanges (net : BackPropagationNetwork) i =
                match i < n with
                | true -> 
                    let trainingExample = trainingSet.[rnd.Next n]
                    loop previousWeightChanges (updateWeights trainingExample.VisibleUnits trainingExample.Target previousWeightChanges) (i + 1)
                | _ -> network

            loop initialWeightChanges network 0
