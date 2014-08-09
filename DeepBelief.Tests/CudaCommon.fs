namespace DeepBelief.Tests

module CudaCommon =

    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Xunit
    open FsUnit.Xunit
    open Backpropagation.CudaTemplates
    open Backpropagation.Parameters
    open Common.Analytics
    open Common.CudaTemplates
    open Common.Kernels
    open Common.NeuralNet
    open Common.Utils
    open DeepBelief.CudaTemplates
    open DeepBelief.DeepBeliefNet
    open DeepBelief.Utils
    open DeepBelief.Kernels
    open TestUtils
    open System

    type BinaryMatrixOperationKernelSignature = deviceptr<float32> -> deviceptr<float32> -> deviceptr<float32> -> unit
    let binaryMatrixOperation blockSize (A : Matrix) (B : Matrix) (kernel : Kernel<BinaryMatrixOperationKernelSignature>) (worker : Worker) =
        let hA = A.Height
        let wA = A.Width
        let paddedA = A.PadToMultiplesOf blockSize
        let paddedB = B.PadToMultiplesOf blockSize
        let hPaddedA = paddedA.Height
        let wPaddedA = paddedA.Width
        let flattenedA = paddedA.ToRowMajorFormat
        let flattenedB = paddedB.ToRowMajorFormat

        use flattenedA = worker.Malloc flattenedA
        use flattenedB = worker.Malloc flattenedB
        use result = worker.Malloc<float32> flattenedA.Length

        let lp = createSimpleMatrixOperationLp blockSize hPaddedA wPaddedA
        kernel.Launch lp result.Ptr flattenedA.Ptr flattenedB.Ptr

        let result = result.Gather() |> Matrix.FromRowMajorFormat wPaddedA 
        result.Submatrix 0 0 hA wA

    type BinaryVectorOperationKernelSignature = deviceptr<float32> -> deviceptr<float32> -> deviceptr<float32> -> unit
    let binaryVectorOperation blockSize (x : Vector) (y : Vector) (kernel : Kernel<BinaryVectorOperationKernelSignature>) (worker : Worker) =
        let size = x.Length
        let paddedX = x.PadToMultipleOf blockSize
        let paddedY = y.PadToMultipleOf blockSize

        use paddedX = worker.Malloc paddedX
        use paddedY = worker.Malloc paddedY
        use result = worker.Malloc<float32> paddedX.Length

        let lp = createSimpleVectorOperationLp blockSize paddedX.Length
        kernel.Launch lp result.Ptr paddedX.Ptr paddedY.Ptr

        let result = result.Gather() 
        Array.sub result 0 size |> Vector

    let sigmoidTemplate (blockSize:int) = cuda {
        let! sigmoidKernel = <@ sigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let sigmoidKernel = program.Apply sigmoidKernel

            fun (vector : Vector) start length -> 

                let size = vector.Length
                let vector = vector.PadToMultipleOf blockSize
                let simpleVectorLp = createSimpleVectorOperationLp blockSize vector.Length

                let vector = worker.Malloc vector

                sigmoidKernel.Launch simpleVectorLp vector.Ptr vector.Ptr start length

                Array.sub (vector.Gather()) 0 8
        ) }

    let feedForwardTemplate (blockSize:int) = cuda {
        let! multiplyVectorByMatrixAndTransformTwiceKernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize <@ sigmoid @> <@ dSigmoid @> |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByMatrixAndTransformTwiceKernel = program.Apply multiplyVectorByMatrixAndTransformTwiceKernel
            let coerceKernel = program.Apply coerceKernel

            fun (network : BackPropagationNetwork) (TrainingSet trainingSet) -> 
                let Ws = network.Layers |> List.map (fun layer -> layer.Weights)
                let paddedWeights = Ws |> List.map (fun weightsAndBiases -> weightsAndBiases.PrependRowOfZeroes.PadToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize w.Height w.Width)
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize w.Height)

                let inputs0 = worker.Malloc<float32>(paddedWeights.[0].Width)
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)

                // The contents of these lists will need to be disposed at the end of the run.
                let weights = paddedWeights |> List.map (fun matrix -> matrix.ToRowMajorFormat |> worker.Malloc)
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)

                let mutable result = []
                let N = weights.Length - 1
                for i in 0..List.length trainingSet - 1 do
                    blockSize |> trainingSet.[i].TrainingInput.PadToMultipleOf |> inputs0.Scatter

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch (coerceLp 1) lastOutput.Ptr 0 0 1.0f
                        multiplyVectorByMatrixAndTransformTwiceKernel.Launch forwardLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr paddedWeights.[j].Height paddedWeights.[j].Width

                    let zippedOutputs = List.zip outputs dOutputs
                    let gatheredOutputs = zippedOutputs |> List.mapi (fun iw (output, dOutput) -> (Array.sub (output.Gather()) 1 Ws.[iw].Height, Array.sub (dOutput.Gather()) 1 Ws.[iw].Height))
                    result <- gatheredOutputs :: result

                disposeAll [|weights; dOutputs|]
                result
        ) }

    let errorSignalsTemplate (blockSize:int) = cuda {
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByTransposeOfMatrixKernel = program.Apply multiplyVectorByTransposeOfMatrixKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel

            fun (network : BackPropagationNetwork) (layerOutputs : (Vector * Vector) list) (target : Vector) ->
                let Ws = network.Layers |> List.map (fun layer -> layer.Weights)
                let N = List.length Ws - 1
                let paddedWeights = Ws |> List.map (fun weightsAndBiases -> weightsAndBiases.PrependRowOfZeroes.PadToMultiplesOf blockSize)
                let paddedTarget = (target.Prepend 0.0f).PadToMultipleOf blockSize
                let paddedOutputValues = layerOutputs |> List.map (fst >> fun vector -> vector.PrependForBias.PadToMultipleOf blockSize)
                let paddedOutputDerivatives = layerOutputs |> List.map (snd >> fun vector -> (vector.Prepend 0.0f).PadToMultipleOf blockSize)

                let errorSignalsLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize w.Height)
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize w.Height w.Width)

                use paddedTargetDevice = worker.Malloc(paddedTarget)

                // The contents of these lists will need to be disposed at the end of the run.
                let errorSignalsDevice = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                let weightsDevice = paddedWeights |> List.map (fun w -> w.ToRowMajorFormat |> worker.Malloc)
                let paddedOutputValuesDevice = paddedOutputValues |> List.map (fun o -> worker.Malloc(o)) |> List.rev
                let paddedOutputDerivativesDevice = paddedOutputDerivatives |> List.map (fun o' -> worker.Malloc(o')) |> List.rev

                subtractVectorKernel.Launch errorSignalsLp.[N] errorSignalsDevice.[N].Ptr paddedTargetDevice.Ptr paddedOutputValuesDevice.[N].Ptr

                for j in N..(-1)..0 do
                    if j < N then
                        multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j + 1] errorSignalsDevice.[j].Ptr weightsDevice.[j + 1].Ptr errorSignalsDevice.[j + 1].Ptr paddedWeights.[j + 1].Height paddedWeights.[j + 1].Width
                    pointwiseMultiplyVectorKernel.Launch errorSignalsLp.[j] errorSignalsDevice.[j].Ptr paddedOutputDerivativesDevice.[j].Ptr errorSignalsDevice.[j].Ptr

                let output = errorSignalsDevice |> List.mapi (fun i e -> e.Gather().[1..(fst layerOutputs.[N - i] |> fun v -> v.Length)])
                disposeAll [|errorSignalsDevice; weightsDevice; paddedOutputValuesDevice; paddedOutputDerivativesDevice|]
                output                
        ) }

    let gradientsTemplate (blockSize:int) = cuda {
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! outerProductKernel = outerProductKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByTransposeOfMatrixKernel = program.Apply multiplyVectorByTransposeOfMatrixKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel
            let outerProductKernel = program.Apply outerProductKernel

            fun (network : BackPropagationNetwork) (layerOutputs : (Vector * Vector) list) (input : Vector) (target : Vector) ->
                let Ws = network.Layers |> List.map (fun layer -> layer.Weights)

                let N = List.length Ws - 1
                let paddedWeights = Ws |> List.map (fun w -> w.PrependRowOfZeroes.PadToMultiplesOf blockSize)
                let paddedTarget = (target.Prepend 0.0f).PadToMultipleOf blockSize
                let paddedOutputValues = layerOutputs |> List.map (fst >> fun v -> v.PrependForBias.PadToMultipleOf blockSize)
                let paddedOutputDerivatives = layerOutputs |> List.map (snd >> fun v -> (v.Prepend 0.0f).PadToMultipleOf blockSize)

                let errorSignalsLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize w.Height)
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize w.Height w.Width)
                let outerProductLp = paddedWeights |> List.map (fun w -> createOuterProductLp blockSize w.Height w.Width)

                use paddedTargetDevice = worker.Malloc(paddedTarget)

                use inputs0Device = worker.Malloc(input.PrependForBias.PadToMultipleOf blockSize)

                // The contents of these lists will need to be disposed at the end of the run.
                let errorSignalsDevice = paddedWeights |> List.map (fun w -> worker.Malloc<float32> w.Height)
                let weightsDevice = paddedWeights |> List.map (fun w -> w.ToRowMajorFormat |> worker.Malloc)
                let paddedOutputValuesDevice = paddedOutputValues |> List.map (fun o -> worker.Malloc(o)) |> List.rev
                let paddedOutputDerivativesDevice = paddedOutputDerivatives |> List.map (fun o' -> worker.Malloc(o')) |> List.rev
                let gradsDevice = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(w.Height * w.Width))

                let inputsDevice = inputs0Device :: paddedOutputValuesDevice

                subtractVectorKernel.Launch errorSignalsLp.[N] errorSignalsDevice.[N].Ptr paddedTargetDevice.Ptr paddedOutputValuesDevice.[N].Ptr

                for j in N..(-1)..0 do
                    if j < N then
                        multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j + 1] errorSignalsDevice.[j].Ptr weightsDevice.[j + 1].Ptr errorSignalsDevice.[j + 1].Ptr paddedWeights.[j + 1].Height paddedWeights.[j + 1].Width
                    pointwiseMultiplyVectorKernel.Launch errorSignalsLp.[j] errorSignalsDevice.[j].Ptr paddedOutputDerivativesDevice.[j].Ptr errorSignalsDevice.[j].Ptr
                    outerProductKernel.Launch outerProductLp.[j] gradsDevice.[j].Ptr errorSignalsDevice.[j].Ptr inputsDevice.[j].Ptr paddedWeights.[j].Width

                let output = gradsDevice |> List.mapi (fun i e -> 
                    let array = e.Gather() 
                    (array |> Matrix.FromRowMajorFormat paddedWeights.[i].Width).Submatrix 0 0 (1 + Ws.[i].Height) (Ws.[i].Width)) |> List.map (fun (Matrix m) -> m.[1..,0..] |> Matrix)
                disposeAll [|errorSignalsDevice; weightsDevice; paddedOutputValuesDevice; paddedOutputDerivativesDevice; gradsDevice|]
                output                
        ) }
