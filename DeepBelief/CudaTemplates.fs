namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Kernels
    open DeepBeliefNet
    open Common.Analytics
    open Common.NeuralNet
    open Utils

    type Matrix with
        member this.PadToMultiplesOf n =
            match this with
                Matrix matrix ->
                    let h = Array2D.length1 matrix
                    let w = Array2D.length2 matrix
                    let paddedHeight = nextMultipleOf n h
                    let paddedWidth = nextMultipleOf n w
                    Array2D.init paddedHeight paddedWidth 
                        (fun i j -> if i < h && j < w then matrix.[i, j] else 0.0f) |> Matrix
        member this.ToRowMajorFormat =
            match this with
                Matrix matrix ->
                    let h = this.Height
                    let w = this.Width
                    Array.init (h*w) (fun i -> matrix.[i / w, i % w])
        static member FromRowMajorFormat width (array : float32[]) = 
            Array2D.init (array.Length / width) width (fun i j -> array.[i * width + j]) |> Matrix

    type WeightsAndBiases with
        member this.PadToMultiplesOf blockSize =
            match this with WeightsAndBiases weightsAndBiases -> weightsAndBiases.PadToMultiplesOf blockSize

    type WeightChanges with
        member this.PadToMultiplesOf blockSize =
            match this with WeightChanges weightChanges -> weightChanges.PadToMultiplesOf blockSize

    type InputBatch with
        member this.PadToMultiplesOf blockSize =
            match this with InputBatch inputBatch -> inputBatch.PadToMultiplesOf blockSize |> InputBatch

    let coerceLp blockSize =
        let threads = dim3(blockSize)
        let grid = dim3(1)
        LaunchParam(grid, threads)

    let createMultiplyVectorByMatrixLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(hA / threads.x)
        LaunchParam(grid, threads)

    let createMultiplyVectorByTransposeOfMatrixLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(wA / threads.x)
        LaunchParam(grid, threads)

    let createMultiplyLp blockSize hA wA hB wB =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(wB / threads.x, hA / threads.y)
        LaunchParam(grid, threads)

    let createMultiplyByTransposeLp blockSize hA wA hB wB =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(hB / threads.x, hA / threads.y)
        LaunchParam(grid, threads)

    let createTransposeAndMultiplyLp blockSize hA wA hB wB =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(wB / threads.x, wA / threads.y)
        LaunchParam(grid, threads)

    let createSimpleVectorOperationLp blockSize size =
        let threads = dim3(blockSize)
        let grid = dim3(size / threads.x)
        LaunchParam(grid, threads)

    let createOuterProductLp blockSize hA wA =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(hA / threads.x, wA / threads.y)
        LaunchParam(grid, threads)

    let createSimpleMatrixOperationLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3((hA * wA) / threads.x)
        LaunchParam(grid, threads)

    let createOffsetMatrixOperationLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(((hA - 1) * wA) / threads.x)
        LaunchParam(grid, threads)

    let createActivateFirstRowLp blockSize hM wM =
        let threads = dim3(blockSize)
        let grid = dim3(wM / threads.x)
        LaunchParam(grid, threads)

    let createActivateFirstColumnLp blockSize hM wM =
        let threads = dim3(blockSize)
        let grid = dim3(hM / threads.x)
        LaunchParam(grid, threads)
        
    let multiplyTemplate (blockSize:int) = cuda {
        let! kernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (B : Matrix) ->
                let finalHeight = A.Height
                let finalWidth = B.Width

                let A = A.PadToMultiplesOf blockSize
                let B = B.PadToMultiplesOf blockSize

                let hA = A.Height
                let wA = A.Width
                let hB = B.Height
                let wB = B.Width
                let wC = wB
                let hC = A.Height

                let A = A.ToRowMajorFormat
                let B = B.ToRowMajorFormat

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyLp blockSize hA wA hB wB
                kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                let result = C.Gather() |> Matrix.FromRowMajorFormat wC 
                result.Submatrix 0 0 finalHeight finalWidth
            ) }

    let multiplyByTransposeTemplate (blockSize:int) = cuda {
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let multiplyByTransposeKernel = program.Apply(multiplyByTransposeKernel)

            fun (A : Matrix) (B : Matrix) ->
                let finalHeight = A.Height
                let finalWidth = B.Height

                let A = A.PadToMultiplesOf blockSize
                let B = B.PadToMultiplesOf blockSize

                let hA = A.Height
                let wA = A.Width
                let hB = B.Height
                let wB = B.Width
                let wC = hB
                let hC = A.Height

                let A = A.ToRowMajorFormat
                let B = B.ToRowMajorFormat

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyByTransposeLp blockSize hA wA hB wB
                multiplyByTransposeKernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                let result = C.Gather() |> Matrix.FromRowMajorFormat wC
                result.Submatrix 0 0 finalHeight finalWidth
            ) }

    let trainRbmEpochTemplate (blockSize:int) = cuda {
        let! multiplyKernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel
        let! activateKernel = <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel
        let! addMatrixKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! subtractMatrixKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! scalarMultiplyMatrixKernel = scalarMultiplyMatrixKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply rngKernel
            let multiplyKernel = program.Apply multiplyKernel
            let multiplyByTransposeKernel = program.Apply multiplyByTransposeKernel
            let transposeAndMultiplyKernel = program.Apply transposeAndMultiplyKernel
            let activateFirstRowKernel = program.Apply activateFirstRowKernel
            let activateFirstColumnKernel = program.Apply activateFirstColumnKernel
            let activateKernel = program.Apply activateKernel
            let addMatrixKernel = program.Apply addMatrixKernel
            let subtractMatrixKernel = program.Apply subtractMatrixKernel
            let scalarMultiplyMatrixKernel = program.Apply scalarMultiplyMatrixKernel

            // Copy pre-calculated bit-matrices, needed for jump-ahead
            // calculations, to the device memory.
            let jumpAheadMatrices = worker.Malloc(Data.jumpAheadMatrices)

            fun rnd (rbm : RestrictedBoltzmannMachine) (inputs : LayerInputs) -> 
                let batches = inputs.GetRandomisedInputBatches rnd rbm.Parameters.BatchSize
                let nRows = batches.Head.Size
                let nCols = batches.Head.Dimension
                let batches = batches |> List.map (fun inputBatch -> inputBatch.PadToMultiplesOf blockSize)
                let paddedBatchHeight = batches.Head.Size
                let paddedBatchWidth = batches.Head.Dimension
                let batches = batches |> List.map (fun (InputBatch inputBatch) -> inputBatch.ToRowMajorFormat)
                let nHidden = rbm.NumberOfHiddenUnits
                let nVisible = rbm.NumberOfVisibleUnits
                
                let hVisibleUnitMatrix = paddedBatchHeight
                let wVisibleUnitMatrix = paddedBatchWidth

                let wHiddenUnitMatrix = hVisibleUnitMatrix
                let hHiddenUnitMatrix = 1 + nHidden |> Utils.nextMultipleOf blockSize

                let dimVisibleUnits = hVisibleUnitMatrix * wVisibleUnitMatrix
                let dimHiddenUnits = hHiddenUnitMatrix * wHiddenUnitMatrix

                let weightsAndBiases = rbm.ToWeightsAndBiases.PadToMultiplesOf blockSize 
                let dWeightsAndBiases = rbm.ToWeightsAndBiasesChanges.PadToMultiplesOf blockSize
                let weightsAndBiasesWidth = weightsAndBiases.Width
                let weightsAndBiasesHeight = weightsAndBiases.Height
                let weightsAndBiases = weightsAndBiases.ToRowMajorFormat
                let dWeightsAndBiases = dWeightsAndBiases.ToRowMajorFormat
                let dimWeightsAndBiases = Array.length weightsAndBiases

                use weightsAndBiases = worker.Malloc weightsAndBiases
                use dWeightsAndBiases = worker.Malloc dWeightsAndBiases
                use h1 = worker.Malloc<float32>(dimHiddenUnits)
                use v2 = worker.Malloc<float32>(dimVisibleUnits)
                use h2 = worker.Malloc<float32>(dimHiddenUnits)
                use c1 = worker.Malloc<float32>(dimWeightsAndBiases)
                use c2 = worker.Malloc<float32>(dimWeightsAndBiases)

                use hiddenRandoms = worker.Malloc<float32>(dimHiddenUnits)
                use visibleRandoms = worker.Malloc<float32>(dimVisibleUnits)

                let threads = dim3(blockSize, blockSize)

                let forwardMatrixLp = createMultiplyByTransposeLp blockSize weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                let backwardMatrixLp = createTransposeAndMultiplyLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                let activateHiddenLp = createSimpleMatrixOperationLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix
                let activateVisibleLp = createSimpleMatrixOperationLp blockSize hVisibleUnitMatrix wVisibleUnitMatrix
                let activateFirstRowLp = createActivateFirstRowLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix
                let activateFirstColumnLp = createActivateFirstColumnLp blockSize hVisibleUnitMatrix wVisibleUnitMatrix
                let computeCValueLp = createMultiplyLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix
                let simpleWeightsLp = createSimpleMatrixOperationLp blockSize hHiddenUnitMatrix wVisibleUnitMatrix

                let rngNumStreams = 1024
                let rngBlockSize = dim3(32, 8)
                let rngNumThreadsPerBlock = rngBlockSize.Size
                let rngGridSize = dim3(rngNumStreams / rngNumThreadsPerBlock)
                let rngSharedMemorySize = XorShift7.Size * rngNumThreadsPerBlock
                let rngLp = LaunchParam(rngGridSize, rngBlockSize, rngSharedMemorySize)

                let learningRate = rbm.Parameters.LearningRate
                let momentum = value rbm.Parameters.Momentum
                let weightedLearningRate = value (learningRate / samples.Length)
                use state0 = Utils.generateStartState 42u |> worker.Malloc

                let numRuns = 3 * samples.Length
                for i in 0..samples.Length - 1 do
                    
                    use v1 = samples.[i]

                    // Perform the forward iteration to populate h1
                    multiplyByTransposeKernel.Launch forwardMatrixLp h1.Ptr weightsAndBiases.Ptr v1.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns i state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h1.Ptr h1.Ptr hiddenRandoms.Ptr
                    activateFirstRowKernel.Launch activateFirstRowLp h1.Ptr wHiddenUnitMatrix nRows

                    // Perform the backward iteration to populate v2
                    transposeAndMultiplyKernel.Launch backwardMatrixLp v2.Ptr h1.Ptr weightsAndBiases.Ptr hHiddenUnitMatrix wHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                    rngKernel.Launch rngLp numRuns (i + samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimVisibleUnits / rngNumStreams) visibleRandoms.Ptr
                    activateKernel.Launch activateVisibleLp v2.Ptr v2.Ptr visibleRandoms.Ptr
                    activateFirstColumnKernel.Launch activateFirstColumnLp v2.Ptr hVisibleUnitMatrix wVisibleUnitMatrix nCols

                    // Perform the forward iteration to populate h2
                    multiplyByTransposeKernel.Launch forwardMatrixLp h2.Ptr weightsAndBiases.Ptr v2.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns (i + 2 * samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h2.Ptr h2.Ptr hiddenRandoms.Ptr
                    activateFirstRowKernel.Launch activateFirstRowLp h2.Ptr wHiddenUnitMatrix nRows

                    // Compute c1 = h1 * v1 and c2 = h2 * v2
                    multiplyKernel.Launch computeCValueLp c1.Ptr h1.Ptr v1.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix
                    multiplyKernel.Launch computeCValueLp c2.Ptr h2.Ptr v2.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix

                    // dWeightsAndBiases -> momentum * dWeightsAndBiases + weightedLearningRate * (c1 - c2)
                    subtractMatrixKernel.Launch simpleWeightsLp c1.Ptr c1.Ptr c2.Ptr
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp c1.Ptr weightedLearningRate
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr momentum
                    addMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr dWeightsAndBiases.Ptr c1.Ptr

                    // weightsAndBiases -> weightsAndBiases + dWeightsAndBiases
                    addMatrixKernel.Launch simpleWeightsLp weightsAndBiases.Ptr weightsAndBiases.Ptr dWeightsAndBiases.Ptr

                let weightsAndBiases = weightsAndBiases.Gather() |> Utils.rebuildMatrix wVisibleUnitMatrix (nHidden + 1) (nVisible + 1)
                let wbg = dWeightsAndBiases.Gather()
                let max = Array.maxBy (fun el -> Math.Abs(el |> float)) (Array.sub wbg 1 (wbg.Length - 1))
                let dWeightsAndBiases = wbg |> Utils.rebuildMatrix wVisibleUnitMatrix (nHidden + 1) (nVisible + 1)
                let result = DeepBeliefNet.toRbm rbm.Parameters weightsAndBiases dWeightsAndBiases
                result
        ) }

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

            fun (network : BackPropagationNetwork) (rand : Random) trainingSet testSet -> 
                let Ws = network.Layers |> List.map (fun layer -> layer.Weight)

                let paddedWeights = Ws |> List.map (Utils.prependRowOfZeroes >> Utils.padToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize (Utils.height w) (Utils.width w))
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize (Utils.height w) (Utils.width w))
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (Utils.height w))
                let simpleMatrixLp = paddedWeights |> List.map (fun w -> createSimpleMatrixOperationLp blockSize (Utils.height w) (Utils.width w))
                let offsetMatrixLp = paddedWeights |> List.map (fun w -> createOffsetMatrixOperationLp blockSize (Utils.height w) (Utils.width w))
                let outerProductLp = paddedWeights |> List.map (fun w -> createOuterProductLp blockSize (Utils.height w) (Utils.width w))

                use inputs0 = worker.Malloc<float32>(Utils.width paddedWeights.[0])

                // The contents of these lists will need to be disposed at the end of the run.
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let weights = paddedWeights |> List.map (Utils.flattenMatrix >> worker.Malloc)
                let prevDWeights = paddedWeights |> List.map (fun w -> Array2D.zeroCreate (Utils.height w) (Utils.width w) |> Utils.flattenMatrix |> worker.Malloc)
                let grads = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w * Utils.width w))
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let errorSignals = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                
                let inputs = inputs0 :: outputs
                let N = weights.Length - 1
                let epochs = value parameters.Epochs
                let learningRate = value parameters.LearningRate
                let momentum = value parameters.Momentum
                for i in 0..(Array.length trainingSet * epochs) - 1 do
                    let index = rand.Next (Array.length trainingSet)
                    inputs0.Scatter(fst trainingSet.[index] |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)

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
