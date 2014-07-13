namespace DeepBelief

module CudaNeuralNet =

    open DeepBeliefNet
    open CudaTemplates
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Utils

    let gpuComputeNnetResults netProps trainingSet testSet rand parameters = 
        use runTrainNeuralNetEpochProgram = 32 |> runTrainNeuralNetEpochTemplate parameters |> Compiler.load Worker.Default
        let gpuOutput = runTrainNeuralNetEpochProgram.Run netProps rand trainingSet testSet
        gpuOutput
