namespace Backpropagation

module CudaSupervisedLearning =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Common.Utils
    open Common.NeuralNet
    open CudaTemplates
    open Parameters

    type BackPropagationNetwork with
        member network.TrainGpu (rnd : RandomSingle) (trainingSet : TrainingSet) (sampleFrequency : SampleFrequency) =
            use runTrainNeuralNetEpochProgram = 32 |> runTrainNeuralNetEpochTemplate |> Compiler.load Worker.Default
            runTrainNeuralNetEpochProgram.Run network trainingSet rnd sampleFrequency
        member network.ReadTestSetGpu (testSet : TestSet) =
            use runReadNeuralNetTemplate = 32 |> runReadNeuralNetTemplate |> Compiler.load Worker.Default
            runReadNeuralNetTemplate.Run network testSet
