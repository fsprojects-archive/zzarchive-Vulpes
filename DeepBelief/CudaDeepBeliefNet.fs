namespace DeepBelief

module CudaDeepBeliefNet =

    open DeepBeliefNet
    open CudaTemplates
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Common.Analytics
    open Common.NeuralNet
    open Common.Utils
    open Utils

    type RestrictedBoltzmannMachine with
        member rbm.TrainLayerGpu rnd (layerInputs : LayerInputs) (sampleFrequency : SampleFrequency) callback =
            use cudaRbmEpochProgram = 32 |> trainRbmEpochTemplate |> Compiler.load Worker.Default
            let epochs = match rbm.Parameters.Epochs with Epochs e -> e
            [1..epochs] |> List.fold(fun acc i -> cudaRbmEpochProgram.Run rnd acc layerInputs sampleFrequency callback) rbm
        member rbm.NextLayerUpGpu rnd (layerInputs : LayerInputs) =
            let toLayerInput (BatchOutput output) =
                let width = output.Width
                let output = output.Submatrix 1 0 (output.Height - 1) output.Width
                [0..width - 1] |> List.map (fun j -> output.Column j |> Input.FromVector) |> LayerInputs
            use multiplyProgram = 32 |> multiplyTemplate |> Compiler.load Worker.Default
            let batch = (layerInputs.GetRandomisedInputBatches rnd (BatchSize 1)).Head.ActivateFirstColumn
            let xInputs = match batch with InputBatch inputBatch -> inputBatch
            let weightsAndBiases = match rbm.ToWeightsAndBiases with WeightsAndBiases wb -> wb
            let output = multiplyProgram.Run xInputs weightsAndBiases.Transpose |> BatchOutput
            output.Activate rnd sigmoidFunction |> toLayerInput

    type DeepBeliefNetwork with
        member dbn.TrainGpu rnd (trainingSet : TrainingSet) sampleFrequency callback =
            let firstLayerInput = trainingSet.ToFirstLayerInput
            let start = dbn.Machines.Head.TrainLayerGpu rnd firstLayerInput sampleFrequency callback
            {
                Parameters = dbn.Parameters;
                Machines = dbn.Machines.Tail |> List.fold(fun acc element -> 
                    let currentTuple = List.head acc
                    let rbm : RestrictedBoltzmannMachine = fst currentTuple
                    let layerInputs = snd currentTuple
                    let nextLayerUp = rbm.NextLayerUpGpu rnd layerInputs
                    let nextRbm = element.TrainLayerGpu rnd nextLayerUp sampleFrequency callback
                    (nextRbm, nextLayerUp) :: acc) [(start, firstLayerInput)]
                    |> List.map fst |> List.rev 
            }
