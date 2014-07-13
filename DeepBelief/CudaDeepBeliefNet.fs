namespace DeepBelief

module CudaDeepBeliefNet =

    open DeepBeliefNet
    open CudaTemplates
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Utils

    let gpuRbmUp weightsAndBiases activation xInputs =
        use multiplyProgram = 32 |> multiplyTemplate |> Compiler.load Worker.Default
        multiplyProgram.Run xInputs (transpose weightsAndBiases) |> mapMatrix activation

    let gpuRbmTrain rand (rbm : RestrictedBoltzmannMachine) (xInputs : Matrix) =
        use cudaRbmEpochProgram = 32 |> trainRbmEpochTemplate |> Compiler.load Worker.Default
        let epochs = value rbm.Parameters.Epochs
        [1..epochs] |> List.fold(fun acc i -> cudaRbmEpochProgram.Run rand acc xInputs) rbm

    let gpuDbnTrain rand (dbn : DeepBeliefNetwork) xInputs =
        let prependedInputs = xInputs |> prependColumnOfOnes
        let start = gpuRbmTrain rand (List.head dbn.Machines) prependedInputs
        { 
            Parameters = dbn.Parameters
            Machines = dbn.Machines.Tail |> List.fold(fun acc element -> 
                let currentTuple = List.head acc
                let x = gpuRbmUp (fst currentTuple |> toWeightsAndBiases) sigmoidFunction (snd currentTuple)
                let nextRbm = gpuRbmTrain rand element x
                (nextRbm, x) :: acc) [(start, prependedInputs)]
                |> List.map fst |> List.rev 
        }
