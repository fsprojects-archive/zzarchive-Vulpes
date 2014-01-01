namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief.CudaTemplates
open DeepBelief.DeepBeliefNet
open DeepBelief.Utils
open DeepBelief.Kernels
open DeepBelief.NeuralNet
open System

type ``CUDA Neural Net``()=
    
    let vector = [|0.1f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.8f; 0.9f|]
    let restrictedVector = [|0.0f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.0f; 0.0f|]
    let logitVector = vector |> Array.map logitFunction

    let sizes = [500; 300; 10]
    let alpha = 0.5f
    let momentum = 0.9f
    let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
    let gpuInputs = [| (xInput, None) |]
    let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
    let layeredDbn = dbn sizes xInputs
    let nnetProps = 
        {
            Weights = layeredDbn |> List.map (fun rbm -> prependColumn rbm.HiddenBiases rbm.Weights);
            Activations = layeredDbn |> List.map (fun _ -> (sigmoid, sigmoid >> dSigmoid))
        }

    let sigmoidTemplate (blockSize:int) = cuda {
        let! sigmoidKernel = <@ sigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let sigmoidKernel = program.Apply sigmoidKernel

            fun (vector : Vector) start length -> 

                let size = vector.Length
                let vector = vector |> padToMultipleOf blockSize
                let simpleVectorLp = createSimpleVectorOperationLp blockSize vector.Length

                let vector = worker.Malloc vector

                sigmoidKernel.Launch simpleVectorLp vector.Ptr vector.Ptr start length

                Array.sub (vector.Gather()) 0 8
        ) }

    let feedForwardTemplate (blockSize:int) = cuda {
        let! multiplyVectorByMatrixKernel = multiplyVectorByMatrixKernel blockSize |> Compiler.DefineKernel
        let! sigmoidKernel = <@ sigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel
        let! dSigmoidKernel = <@ dSigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByMatrixKernel = program.Apply multiplyVectorByMatrixKernel
            let sigmoidKernel = program.Apply sigmoidKernel
            let dSigmoidKernel = program.Apply dSigmoidKernel
            let coerceKernel = program.Apply coerceKernel

            fun (netProps : NnetProperties) trainingSet -> 
                let paddedWeights = netProps.Weights |> List.map (prependRowOfZeroes >> padToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize (height w) (width w))
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (height w))

                let inputs0 = worker.Malloc<float32>(width paddedWeights.[0])
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))

                // The contents of these lists will need to be disposed at the end of the run.
                let weights = paddedWeights |> List.map (flattenMatrix >> worker.Malloc)
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))

                let mutable result = []
                let N = weights.Length - 1
                for i in 0..Array.length trainingSet - 1 do
                    inputs0.Scatter(fst trainingSet.[i] |> prependForBias |> padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        multiplyVectorByMatrixKernel.Launch forwardLp.[j] outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (height paddedWeights.[j]) (width paddedWeights.[j])
                        sigmoidKernel.Launch outputLp.[j] outputs.[j].Ptr outputs.[j].Ptr 1 (height netProps.Weights.[j])
                        dSigmoidKernel.Launch outputLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr 1 (height netProps.Weights.[j])
                        coerceKernel.Launch coerceLp outputs.[j].Ptr 0 1.0f
                        coerceKernel.Launch coerceLp dOutputs.[j].Ptr 0 0.0f

                    let zippedOutputs = List.zip outputs dOutputs
                    let gatheredOutputs = zippedOutputs |> List.mapi (fun iw (output, dOutput) -> (Array.sub (output.Gather()) 0 (1 + height netProps.Weights.[iw]), Array.sub (dOutput.Gather()) 0 (1 + height netProps.Weights.[iw])))
                    result <- gatheredOutputs :: result
                result
       ) }

    let sigmoidProgramBlock1 = 1 |> sigmoidTemplate |> Compiler.load Worker.Default
    let sigmoidProgramBlock2 = 2 |> sigmoidTemplate |> Compiler.load Worker.Default
    let sigmoidProgramBlock32 = 32 |> sigmoidTemplate |> Compiler.load Worker.Default

    let feedForwardProgramBlock1 = 1 |> feedForwardTemplate |> Compiler.load Worker.Default
    let feedForwardProgramBlock2 = 1 |> feedForwardTemplate |> Compiler.load Worker.Default
    let feedForwardProgramBlock32 = 1 |> feedForwardTemplate |> Compiler.load Worker.Default

    let cpuFeedForwardOutputs = feedForward nnetProps xInput |> List.rev |> List.map (fun (output, dOutput) -> (prependForBias output, prepend 0.0f dOutput))
    let lastPrependedFeedForwardOutput = (prependForBias (fst cpuFeedForwardOutputs.[0]), prepend 0.0f (snd cpuFeedForwardOutputs.[0]))

    let liesWithinTolerance diffs =
        let maxDiff = Array.max diffs
        maxDiff < 1e-6

    let arraysMatch cpu gpu =
        Array.zip cpu gpu |> Array.map (fun el -> Math.Abs ((fst el |> float) - (snd el |> float))) |> liesWithinTolerance

    let outputsMatch result =
        arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

    let levelResultsMatch results =
        List.forall (fun result -> outputsMatch result) results

    let resultsMatch cpu gpu =
        List.zip cpu gpu |> levelResultsMatch

    [<Fact>] member test.
        ``The feedForward block 1 program matches the outputs of the feedForward function.``()=
            resultsMatch cpuFeedForwardOutputs ((feedForwardProgramBlock1.Run nnetProps gpuInputs).[0]) |> should equal true

    [<Fact>] member test.
        ``The feedForward block 2 program matches the outputs of the feedForward function.``()=
            resultsMatch cpuFeedForwardOutputs ((feedForwardProgramBlock2.Run nnetProps gpuInputs).[0]) |> should equal true

    [<Fact>] member test.
        ``The feedForward block 32 program matches the outputs of the feedForward function.``()=
            resultsMatch cpuFeedForwardOutputs ((feedForwardProgramBlock32.Run nnetProps gpuInputs).[0]) |> should equal true

    [<Fact>] member test.
        ``The sigmoid block 1 program maps the logit vector to the original vector.``()=
            sigmoidProgramBlock1.Run logitVector 0 8 |> should equal vector

    [<Fact>] member test.
        ``The sigmoid block 2 program maps the logit vector to the original vector.``()=
            sigmoidProgramBlock2.Run logitVector 0 8 |> should equal vector

    [<Fact>] member test.
        ``The sigmoid block 32 program maps the logit vector to the original vector.``()=
            sigmoidProgramBlock32.Run logitVector 0 8 |> should equal vector

    [<Fact>] member test.
        ``The sigmoid block 1 program reproduces the restricted vector.``()=
            sigmoidProgramBlock1.Run logitVector 1 5 |> should equal restrictedVector

    [<Fact>] member test.
        ``The sigmoid block 2 program reproduces the restricted vector.``()=
            sigmoidProgramBlock2.Run logitVector 1 5 |> should equal restrictedVector

    [<Fact>] member test.
        ``The sigmoid block 32 program reproduces the restricted vector.``()=
            sigmoidProgramBlock32.Run logitVector 1 5 |> should equal restrictedVector
