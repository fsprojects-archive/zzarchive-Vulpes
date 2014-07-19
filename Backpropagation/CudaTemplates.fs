namespace Backpropagation

module CudaTemplates =

    open System
    open Parameters
    open Alea.CUDA
    open Common
    open Common.Analytics
    open Common.Kernels
    open Common.CudaTemplates
    open NeuralNet

    let runTrainNeuralNetEpochTemplate (parameters : BackPropagationParameters) (blockSize : int) = cuda {
        let! multiplyVectorByMatrixAndTransformKernel = multiplyVectorByMatrixAndTransformKernel blockSize <@ sigmoid @> |> Compiler.DefineKernel
        let! multiplyVectorByMatrixAndTransformTwiceKernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize <@ sigmoid @> <@ dSigmoid @> |> Compiler.DefineKernel
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel blockSize |> Compiler.DefineKernel
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
            let addVectorKernel = program.Apply addVectorKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel
            let outerProductKernel = program.Apply outerProductKernel
            let scalarMultiplyMatrixKernel = program.Apply scalarMultiplyMatrixKernel
            let addMatrixKernel = program.Apply addMatrixKernel

            fun (network : BackPropagationNetwork) (NeuralNet.TrainingSet trainingSet) (rnd : Random) -> 
                let Ws = network.Layers |> List.map (fun layer -> layer.Weights)

                let paddedWeights = Ws |> List.map (fun (weightsAndBiases : WeightsAndBiases) -> weightsAndBiases.PrependRowOfZeroes.PadToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize w.Height w.Width)
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize w.Height w.Width)
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize w.Height)
                let simpleMatrixLp = paddedWeights |> List.map (fun w -> createSimpleMatrixOperationLp blockSize w.Height w.Width)
                let offsetMatrixLp = paddedWeights |> List.map (fun w -> createOffsetMatrixOperationLp blockSize w.Height w.Width)
                let outerProductLp = paddedWeights |> List.map (fun w -> createOuterProductLp blockSize w.Height w.Width)

                use inputs0 = worker.Malloc<float32>(paddedWeights.[0].Width)

                // The contents of these lists will need to be disposed at the end of the run.
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                let weights = paddedWeights |> List.map ((fun w -> w.ToRowMajorFormat) >> worker.Malloc)
                let prevDWeights = paddedWeights |> List.map (fun w -> Array2D.zeroCreate w.Height w.Width |> Matrix |> fun w -> w.ToRowMajorFormat |> worker.Malloc)
                let grads = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(w.Height * w.Width))
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                let errorSignals = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                
                let inputs = inputs0 :: outputs
                let N = weights.Length - 1
                let epochs = match parameters.Epochs with Epochs n -> n
                let learningRate = match parameters.LearningRate with ScaledLearningRate lr -> lr
                let momentum = match parameters.Momentum with Momentum m -> m
                for i in 0..(trainingSet.Length * epochs) - 1 do
                    let index = rnd.Next trainingSet.Length
                    inputs0.Scatter(trainingSet.[index].Input |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch (coerceLp 1) lastOutput.Ptr 0 0 1.0f
                        multiplyVectorByMatrixAndTransformTwiceKernel.Launch forwardLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (Utils.height paddedWeights.[j]) (Utils.width paddedWeights.[j])
                        coerceKernel.Launch (coerceLp 1) dOutputs.[j].Ptr 0 0 0.0f

                        let minIndex = 1 + Utils.height Ws.[j]
                        let maxIndex = Utils.height paddedWeights.[j]

                        coerceKernel.Launch (coerceLp maxIndex) outputs.[j].Ptr minIndex maxIndex 0.0f
                        coerceKernel.Launch (coerceLp maxIndex) dOutputs.[j].Ptr minIndex maxIndex 0.0f

                    coerceKernel.Launch (coerceLp 1) outputs.[N].Ptr 0 0 1.0f

                    errorSignals.[N].Scatter (snd trainingSet.[index] |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)
                    subtractVectorKernel.Launch outputLp.[N] errorSignals.[N].Ptr errorSignals.[N].Ptr outputs.[N].Ptr

                    for j in N..(-1)..0 do
                        if j < N then 
                            multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j + 1] errorSignals.[j].Ptr weights.[j + 1].Ptr errorSignals.[j + 1].Ptr (Utils.height paddedWeights.[j + 1]) (Utils.width paddedWeights.[j + 1])

                        pointwiseMultiplyVectorKernel.Launch outputLp.[j] errorSignals.[j].Ptr dOutputs.[j].Ptr errorSignals.[j].Ptr
                        outerProductKernel.Launch outerProductLp.[j] grads.[j].Ptr errorSignals.[j].Ptr inputs.[j].Ptr (Utils.width paddedWeights.[j])

                    for j in N..(-1)..0 do
                        let wW = Utils.width paddedWeights.[j]
                        scalarMultiplyMatrixKernel.Launch simpleMatrixLp.[j] grads.[j].Ptr learningRate
                        scalarMultiplyMatrixKernel.Launch simpleMatrixLp.[j] prevDWeights.[j].Ptr momentum
                        addMatrixKernel.Launch offsetMatrixLp.[j] (prevDWeights.[j].Ptr + wW) (prevDWeights.[j].Ptr + wW) (grads.[j].Ptr + wW)
                        addMatrixKernel.Launch offsetMatrixLp.[j] (weights.[j].Ptr + wW) (weights.[j].Ptr + wW) (prevDWeights.[j].Ptr + wW)

                let mutable testOutputs = [||]
                for i in 0..Array.length testSet - 1 do
                    inputs0.Scatter(fst testSet.[i] |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch (coerceLp 1) lastOutput.Ptr 0 0 1.0f
                        multiplyVectorByMatrixAndTransformKernel.Launch forwardLp.[j] outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (Utils.height paddedWeights.[j]) (Utils.width paddedWeights.[j])

                        let minIndex = 1 + Utils.height Ws.[j]
                        let maxIndex = Utils.height paddedWeights.[j]
                        coerceKernel.Launch (coerceLp maxIndex) outputs.[j].Ptr minIndex maxIndex 0.0f

                    let finalOutput = outputs.[N].Gather()
                    testOutputs <- Array.append testOutputs [|(Array.sub finalOutput 1 (Array.length (snd testSet.[i])))|]

                Utils.disposeAll [|outputs; weights; prevDWeights; grads; dOutputs; errorSignals|]
                testOutputs
        ) }


