namespace Backpropagation

module CudaTemplates =

    open System
    open Parameters
    open Alea.CUDA
    open Common
    open Common.Analytics
    open Common.CudaTemplates
    open Common.Kernels
    open Common.Utils
    open NeuralNet

    let padToMultipleOf n (signals : Signal[]) =
        let value (Signal signal) = signal
        let size = Array.length signals
        let paddedSize = nextMultipleOf n size
        Array.init paddedSize 
            (fun i -> if i < size then value signals.[i] else 0.0f)

    type Input with
        member this.PrependForBias =
            match this with Input input -> Signal 1.0f :: List.ofArray input |> Array.ofList |> Input
        member this.PadToMultipleOf n =
            match this with Input input -> padToMultipleOf n input
        member this.Dimension =
            match this with Input input -> input.Length

    type Target with
        member this.PrependForBias =
            match this with Target target -> Signal 1.0f :: List.ofArray target |> Array.ofList |> Target
        member this.PadToMultipleOf n =
            match this with Target target -> padToMultipleOf n target
        member this.Dimension =
            match this with Target target -> target.Length

    type WeightsAndBiases with
        member this.Height = match this with WeightsAndBiases weightsAndBiases -> weightsAndBiases.Height
        member this.Width = match this with WeightsAndBiases weightsAndBiases -> weightsAndBiases.Width

    let writeErrorReport i j (errorSignalsSample : Vector) =
        Console.WriteLine("Iteration {0}, Layer {1}, Batch {2}, Error {3}", i, j + 1, errorSignalsSample.SumOfSquares)

    let runTrainNeuralNetEpochTemplate (blockSize : int) = cuda {
        let! multiplyVectorByMatrixAndTransformKernel = multiplyVectorByMatrixAndTransformKernel blockSize <@ sigmoid @> |> Compiler.DefineKernel
        let! multiplyVectorByMatrixAndTransformTwiceKernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize <@ sigmoid @> <@ dSigmoid @> |> Compiler.DefineKernel
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel blockSize |> Compiler.DefineKernel
        let! copyKernel = copyKernel blockSize |> Compiler.DefineKernel
        let! addVectorKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! outerProductKernel = outerProductKernel blockSize |> Compiler.DefineKernel
        let! scalarMultiplyMatrixKernel = scalarMultiplyMatrixKernel blockSize |> Compiler.DefineKernel
        let! addMatrixKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByMatrixAndTransformKernel = program.Apply multiplyVectorByMatrixAndTransformKernel
            let multiplyVectorByMatrixAndTransformTwiceKernel = program.Apply multiplyVectorByMatrixAndTransformTwiceKernel
            let multiplyVectorByTransposeOfMatrixKernel = program.Apply multiplyVectorByTransposeOfMatrixKernel
            let coerceKernel = program.Apply coerceKernel
            let copyKernel = program.Apply copyKernel
            let addVectorKernel = program.Apply addVectorKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel
            let outerProductKernel = program.Apply outerProductKernel
            let scalarMultiplyMatrixKernel = program.Apply scalarMultiplyMatrixKernel
            let addMatrixKernel = program.Apply addMatrixKernel

            fun (network : BackPropagationNetwork) (TrainingSet trainingSet) (rnd : RandomSingle) (SampleFrequency sampleFrequency) -> 
                let Ws = network.Layers |> List.map (fun layer -> layer.Weights)

                let paddedWeights = Ws |> List.map (fun (weightsAndBiases : WeightsAndBiases) -> weightsAndBiases.PrependRowOfZeroes.PadToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize w.Height w.Width)
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize w.Height w.Width)
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize w.Height)
                let simpleMatrixLp = paddedWeights |> List.map (fun w -> createSimpleMatrixOperationLp blockSize w.Height w.Width)
                let offsetMatrixLp = paddedWeights |> List.map (fun w -> createOffsetMatrixOperationLp blockSize w.Height w.Width)
                let outerProductLp = paddedWeights |> List.map (fun w -> createOuterProductLp blockSize w.Height w.Width)

                use inputs0 = worker.Malloc<float32>(paddedWeights.[0].Width)
                use targetSignals = [|0..trainingSet.Length - 1|] |> Array.map (fun index -> trainingSet.[index].TrainingTarget.PrependForBias.PadToMultipleOf blockSize) |> Array.concat |> worker.Malloc
                use inputSignals = [|0..trainingSet.Length - 1|] |> Array.map (fun index -> trainingSet.[index].TrainingInput.PrependForBias.PadToMultipleOf blockSize) |> Array.concat |> worker.Malloc

                // The contents of these lists will need to be disposed at the end of the run.
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                let weights = paddedWeights |> List.map ((fun w -> w.ToRowMajorFormat) >> worker.Malloc)
                let prevDWeights = paddedWeights |> List.map (fun w -> Array2D.zeroCreate w.Height w.Width |> Matrix |> fun w -> w.ToRowMajorFormat |> worker.Malloc)
                let grads = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(w.Height * w.Width))
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                let errorSignals = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                
                let paddedTargetDimension = trainingSet.[0].TrainingTarget.Dimension + 1 |> nextMultipleOf blockSize
                let paddedInputDimension = trainingSet.[0].TrainingInput.Dimension + 1 |> nextMultipleOf blockSize
                let copyTargetLp = createSimpleVectorOperationLp blockSize paddedTargetDimension
                let copyInputLp = createSimpleVectorOperationLp blockSize paddedInputDimension
                let inputs = inputs0 :: outputs
                let N = weights.Length - 1
                let epochs = match network.Parameters.Epochs with Epochs n -> n
                let learningRate = match network.Parameters.LearningRate with ScaledLearningRate lr -> lr
                let momentum = match network.Parameters.Momentum with Momentum m -> m
                for i in 0..(trainingSet.Length * epochs) - 1 do
                    let index = rnd.Next trainingSet.Length
                    copyKernel.Launch copyInputLp inputs0.Ptr inputSignals.Ptr (index * paddedInputDimension)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch (coerceLp 1) lastOutput.Ptr 0 0 1.0f
                        multiplyVectorByMatrixAndTransformTwiceKernel.Launch forwardLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr paddedWeights.[j].Height paddedWeights.[j].Width
                        coerceKernel.Launch (coerceLp 1) dOutputs.[j].Ptr 0 0 0.0f

                        let minIndex = 1 + Ws.[j].Height
                        let maxIndex = paddedWeights.[j].Height

                        coerceKernel.Launch (coerceLp maxIndex) outputs.[j].Ptr minIndex maxIndex 0.0f
                        coerceKernel.Launch (coerceLp maxIndex) dOutputs.[j].Ptr minIndex maxIndex 0.0f

                    coerceKernel.Launch (coerceLp 1) outputs.[N].Ptr 0 0 1.0f

                    copyKernel.Launch copyTargetLp errorSignals.[N].Ptr targetSignals.Ptr (index * paddedTargetDimension)
                    subtractVectorKernel.Launch outputLp.[N] errorSignals.[N].Ptr errorSignals.[N].Ptr outputs.[N].Ptr

                    for j in N..(-1)..0 do
                        if j < N then 
                            multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j + 1] errorSignals.[j].Ptr weights.[j + 1].Ptr errorSignals.[j + 1].Ptr paddedWeights.[j + 1].Height paddedWeights.[j + 1].Width

                        pointwiseMultiplyVectorKernel.Launch outputLp.[j] errorSignals.[j].Ptr dOutputs.[j].Ptr errorSignals.[j].Ptr
                        outerProductKernel.Launch outerProductLp.[j] grads.[j].Ptr errorSignals.[j].Ptr inputs.[j].Ptr paddedWeights.[j].Width
                        if i % sampleFrequency = 0 then
                            let errorSignalsSample = errorSignals.[j].Gather() |> Vector
                            writeErrorReport i j errorSignalsSample

                    for j in N..(-1)..0 do
                        let wW = paddedWeights.[j].Width
                        scalarMultiplyMatrixKernel.Launch simpleMatrixLp.[j] grads.[j].Ptr learningRate
                        scalarMultiplyMatrixKernel.Launch simpleMatrixLp.[j] prevDWeights.[j].Ptr momentum
                        addMatrixKernel.Launch offsetMatrixLp.[j] (prevDWeights.[j].Ptr + wW) (prevDWeights.[j].Ptr + wW) (grads.[j].Ptr + wW)
                        addMatrixKernel.Launch offsetMatrixLp.[j] (weights.[j].Ptr + wW) (weights.[j].Ptr + wW) (prevDWeights.[j].Ptr + wW)

                let trainedLayers = weights |> List.mapi (fun i weightsAndBiases -> 
                    {
                        Weights = weightsAndBiases.Gather() |> Matrix.FromRowMajorFormat network.Layers.[i].Weights.Width |> WeightsAndBiases;
                        Activation = network.Layers.[i].Activation
                    })
                disposeAll [|outputs; weights; prevDWeights; grads; dOutputs; errorSignals|]
                {
                    Parameters = network.Parameters;
                    Layers = trainedLayers
                }
        ) }

    let runReadNeuralNetTemplate (blockSize : int) = cuda {
        let! multiplyVectorByMatrixAndTransformKernel = multiplyVectorByMatrixAndTransformKernel blockSize <@ sigmoid @> |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel blockSize |> Compiler.DefineKernel
        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByMatrixAndTransformKernel = program.Apply multiplyVectorByMatrixAndTransformKernel
            let coerceKernel = program.Apply coerceKernel

            fun (network : BackPropagationNetwork) (TestSet testSet) ->
                let Ws = network.Layers |> List.map (fun layer -> layer.Weights)
                let paddedWeights = Ws |> List.map (fun (weightsAndBiases : WeightsAndBiases) -> weightsAndBiases.PrependRowOfZeroes.PadToMultiplesOf blockSize)
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize w.Height w.Width)
                use inputs0 = worker.Malloc<float32>(paddedWeights.[0].Width)

                // The contents of these lists will need to be disposed at the end of the run.
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                let weights = paddedWeights |> List.map ((fun w -> w.ToRowMajorFormat) >> worker.Malloc)

                let N = weights.Length - 1

                let mutable testOutputs = [||]
                for i in 0..testSet.Length - 1 do
                    inputs0.Scatter(testSet.[i].TestInput.PrependForBias.PadToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch (coerceLp 1) lastOutput.Ptr 0 0 1.0f
                        multiplyVectorByMatrixAndTransformKernel.Launch forwardLp.[j] outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr paddedWeights.[j].Height paddedWeights.[j].Width

                        let minIndex = 1 + Ws.[j].Height
                        let maxIndex = paddedWeights.[j].Height
                        coerceKernel.Launch (coerceLp maxIndex) outputs.[j].Ptr minIndex maxIndex 0.0f

                    let finalOutput = outputs.[N].Gather()
                    testOutputs <- Array.append testOutputs [|(Array.sub finalOutput 1 testSet.[i].TestTarget.Dimension)|]

                testOutputs 
                |> Array.map (fun output -> output |> Array.map (fun value -> Signal value) |> Output)
                |> List.ofArray
                |> TestOutput
        ) }


