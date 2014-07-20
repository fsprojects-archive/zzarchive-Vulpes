namespace Backpropagation

module CudaSupervisedLearning =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Common.NeuralNet
    open CudaTemplates
    open Parameters

    type BackPropagationNetwork with
        member network.Train (trainingSet : TrainingSet) (testSet : TestSet) (rnd : Random) =
            use runTrainNeuralNetEpochProgram = 32 |> runTrainNeuralNetEpochTemplate network.Parameters |> Compiler.load Worker.Default
            let gpuOutput = runTrainNeuralNetEpochProgram.Run network trainingSet testSet rnd
            gpuOutput
