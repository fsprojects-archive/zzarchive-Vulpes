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
        member rbm.TrainLayerGpu rnd (layerInputs : LayerInputs) (sampleFrequency : SampleFrequency) layerIndex =
            use cudaRbmEpochProgram = 32 |> trainRbmEpochTemplate |> Compiler.load Worker.Default
            let epochs = match rbm.Parameters.Epochs with Epochs e -> e
            [1..epochs] |> List.fold(fun acc i -> cudaRbmEpochProgram.Run rnd acc layerInputs sampleFrequency layerIndex i) rbm// callback) rbm
        member rbm.NextLayerUpGpu rnd (LayerInputs layerInputs) =
            let toLayerInput (BatchOutput output) =
                let width = output.Width
                let output = output.Submatrix 0 1 output.Height (output.Width - 1)
                [0..output.Height - 1] |> List.map (fun i -> output.Row i |> Input.FromVector) |> LayerInputs
            use multiplyProgram = 32 |> multiplyTemplate |> Compiler.load Worker.Default
            let batch = (InputBatch.FromTrainingExamples layerInputs).ActivateFirstColumn
            let xInputs = match batch with InputBatch inputBatch -> inputBatch
            let weightsAndBiases = match rbm.ToWeightsAndBiases with WeightsAndBiases wb -> wb
            let output = multiplyProgram.Run xInputs weightsAndBiases.Transpose |> BatchOutput
            output.Activate rnd sigmoidFunction |> toLayerInput

    type DeepBeliefNetwork with
        member dbn.TrainGpu rnd (trainingSet : TrainingSet) sampleFrequency =
            let firstLayerInput = trainingSet.ToFirstLayerInput
            let layerIndex = ref 1
            let start = dbn.Machines.Head.TrainLayerGpu rnd firstLayerInput sampleFrequency layerIndex.Value
            {
                Parameters = dbn.Parameters;
                Machines = dbn.Machines.Tail |> List.fold (fun acc element -> 
                    incr layerIndex
                    let currentTuple = List.head acc
                    let rbm : RestrictedBoltzmannMachine = fst currentTuple
                    let layerInputs = snd currentTuple
                    let nextLayerUp = rbm.NextLayerUpGpu rnd layerInputs
                    let nextRbm = element.TrainLayerGpu rnd nextLayerUp sampleFrequency layerIndex.Value
                    (nextRbm, nextLayerUp) :: acc) [(start, firstLayerInput)]
                    |> List.map fst |> List.rev 
            }
