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

    let gpuRbmTrain alpha momentum batchSize epochs rbm (xInputs : Matrix) =
        use cudaRbmEpochProgram = 32 |> runRbmEpochTemplate |> Compiler.load Worker.Default
        let initialisedRbm =
            {
                Weights = rbm.Weights;
                DWeights = rbm.DWeights;
                VisibleBiases = xInputs.[0..,1..] |> toColumns |> Array.map initVisibleBias;
                DVisibleBiases = rbm.DVisibleBiases
                HiddenBiases = rbm.HiddenBiases
                DHiddenBiases = rbm.DHiddenBiases
            }
        [1..epochs] |> List.fold(fun acc i ->
            cudaRbmEpochProgram.Run alpha momentum batchSize acc xInputs) initialisedRbm

    let gpuDbnTrain alpha momentum batchSize epochs rbms xInputs =
        let prependedInputs = xInputs |> prependColumnOfOnes
        let start = gpuRbmTrain alpha momentum batchSize epochs (List.head rbms) prependedInputs
        rbms.Tail |> List.fold(fun acc element -> 
            let currentTuple = List.head acc
            let x = gpuRbmUp (fst currentTuple |> toWeightsAndBiases) sigmoidFunction (snd currentTuple)
            let nextRbm = gpuRbmTrain alpha momentum batchSize epochs element x
            (nextRbm, x) :: acc) [(start, prependedInputs)]
            |> List.map fst |> List.rev

    let gpuComputeResults netProps trainingSet testSet nnEta nnAlpha epochs = 
        use runTrainNeuralNetEpochProgram = 32 |> runTrainNeuralNetEpochTemplate nnEta nnAlpha epochs |> Compiler.load Worker.Default
        let gpuOutput = runTrainNeuralNetEpochProgram.Run netProps trainingSet testSet
        let targets = testSet |> Array.map (fun x -> snd x)

        let testError = 
            Array.zip targets gpuOutput
            |> Array.fold (fun E (x, t) -> 
                let En = error t x
                E + En) 0.0f

        testError / (float32 testSet.Length)