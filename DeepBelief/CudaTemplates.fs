namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Kernels
    open NeuralNet

    let coerceLp =
        let threads = dim3(1)
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

    let createSimpleMatrixOperationLp blockSize hA wA =
        createMultiplyLp blockSize hA wA hA wA

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

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                let finalHeight = Utils.height A
                let finalWidth = Utils.width B

                let A = Utils.padToMultiplesOf blockSize A
                let B = Utils.padToMultiplesOf blockSize B

                let hA = Utils.height A
                let wA = Utils.width A
                let hB = Utils.height B
                let wB = Utils.width B
                let wC = wB
                let hC = Utils.height A

                let A = Utils.flattenMatrix A
                let B = Utils.flattenMatrix B

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyLp blockSize hA wA hB wB
                kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                worker.Synchronize()
                let result = C.Gather()
                result |> Utils.rebuildMatrix wC finalHeight finalWidth
            ) }

    let multiplyByTransposeTemplate (blockSize:int) = cuda {
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let multiplyByTransposeKernel = program.Apply(multiplyByTransposeKernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                let finalHeight = Utils.height A
                let finalWidth = Utils.height B

                let A = Utils.padToMultiplesOf blockSize A
                let B = Utils.padToMultiplesOf blockSize B

                let hA = Utils.height A
                let wA = Utils.width A
                let hB = Utils.height B
                let wB = Utils.width B
                let wC = hB
                let hC = Utils.height A

                let A = Utils.flattenMatrix A
                let B = Utils.flattenMatrix B

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyByTransposeLp blockSize hA wA hB wB
                multiplyByTransposeKernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                let result = C.Gather()
                result |> Utils.rebuildMatrix wC finalHeight finalWidth
            ) }

    let runRbmEpochTemplate (blockSize:int) = cuda {
        let! multiplyKernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel
        let! activateKernel = <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel
        let! addMatrixKernel = <@ pointwiseAdd @> |> pointwiseMatrixOperationKernel blockSize |> Compiler.DefineKernel
        let! subtractMatrixKernel = <@ pointwiseSubtract @> |> pointwiseMatrixOperationKernel blockSize |> Compiler.DefineKernel
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

            fun (alpha:float32) momentum batchSize rbm xInputs -> 
                let nRows = Utils.height xInputs
                let nCols = Utils.width xInputs
                let xRand = Utils.permuteRows Utils.rand xInputs
                let samples = 
                    xRand |> Utils.batchesOf batchSize 
                    |> Array.map (array2D >> Utils.prependColumnOfOnes >> Utils.padToMultiplesOf blockSize)
                
                let paddedSampleHeight = Utils.height samples.[0]
                let paddedSampleWidth = Utils.width samples.[0]

                let samples = samples |> Array.map (Utils.flattenMatrix >> worker.Malloc)

                let nHidden = DeepBeliefNet.numberOfHiddenUnits rbm
                let nVisible = DeepBeliefNet.numberOfVisibleUnits rbm
                
                let hVisibleUnitMatrix = paddedSampleHeight
                let wVisibleUnitMatrix = paddedSampleWidth

                let wHiddenUnitMatrix = hVisibleUnitMatrix
                let hHiddenUnitMatrix = 1 + nHidden |> Utils.nextMultipleOf blockSize

                let dimVisibleUnits = hVisibleUnitMatrix * wVisibleUnitMatrix
                let dimHiddenUnits = hHiddenUnitMatrix * wHiddenUnitMatrix

                let weightsAndBiases = DeepBeliefNet.toWeightsAndBiases rbm |> Utils.padToMultiplesOf blockSize 
                let dWeightsAndBiases = DeepBeliefNet.toDWeightsAndBiases rbm |> Utils.padToMultiplesOf blockSize
                let weightsAndBiasesWidth = Utils.width weightsAndBiases
                let weightsAndBiasesHeight = Utils.height weightsAndBiases
                let weightsAndBiases = weightsAndBiases|> Utils.flattenMatrix
                let dWeightsAndBiases = dWeightsAndBiases |> Utils.flattenMatrix
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

                let weightedAlpha = alpha / (float32 samples.Length)
                use state0 = Utils.generateStartState 42u |> worker.Malloc

                let numRuns = 3 * samples.Length
                for i in 0..samples.Length - 1 do
                    
                    use v1 = samples.[i]

                    // Perform the forward iteration to populate h1
                    multiplyByTransposeKernel.Launch forwardMatrixLp h1.Ptr weightsAndBiases.Ptr v1.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns i state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h1.Ptr hiddenRandoms.Ptr hHiddenUnitMatrix wHiddenUnitMatrix
                    activateFirstRowKernel.Launch activateFirstRowLp h1.Ptr wHiddenUnitMatrix nRows

                    // Perform the backward iteration to populate v2
                    transposeAndMultiplyKernel.Launch backwardMatrixLp v2.Ptr h1.Ptr weightsAndBiases.Ptr hHiddenUnitMatrix wHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                    rngKernel.Launch rngLp numRuns (i + samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimVisibleUnits / rngNumStreams) visibleRandoms.Ptr
                    activateKernel.Launch activateVisibleLp v2.Ptr visibleRandoms.Ptr hVisibleUnitMatrix wVisibleUnitMatrix
                    activateFirstColumnKernel.Launch activateFirstColumnLp v2.Ptr hVisibleUnitMatrix wVisibleUnitMatrix nCols

                    // Perform the forward iteration to populate h2
                    multiplyByTransposeKernel.Launch forwardMatrixLp h2.Ptr weightsAndBiases.Ptr v2.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns (i + 2 * samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h2.Ptr hiddenRandoms.Ptr hHiddenUnitMatrix wHiddenUnitMatrix
                    activateFirstRowKernel.Launch activateFirstRowLp h2.Ptr wHiddenUnitMatrix nRows

                    // Compute c1 = h1 * v1 and c2 = h2 * v2
                    multiplyKernel.Launch computeCValueLp c1.Ptr h1.Ptr v1.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix
                    multiplyKernel.Launch computeCValueLp c2.Ptr h2.Ptr v2.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix

                    // dWeightsAndBiases -> momentum * dWeightsAndBiases + weightedAlpha * (c1 - c2)
                    subtractMatrixKernel.Launch simpleWeightsLp c1.Ptr c2.Ptr hHiddenUnitMatrix wVisibleUnitMatrix
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp c1.Ptr weightedAlpha hHiddenUnitMatrix wVisibleUnitMatrix
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr momentum hHiddenUnitMatrix wVisibleUnitMatrix
                    addMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr c1.Ptr hHiddenUnitMatrix wVisibleUnitMatrix

                    // weightsAndBiases -> weightsAndBiases + dWeightsAndBiases
                    addMatrixKernel.Launch simpleWeightsLp weightsAndBiases.Ptr dWeightsAndBiases.Ptr hHiddenUnitMatrix wVisibleUnitMatrix

                let weightsAndBiases = weightsAndBiases.Gather() |> Utils.rebuildMatrix wVisibleUnitMatrix (nHidden + 1) (nVisible + 1)
                let dWeightsAndBiases = dWeightsAndBiases.Gather() |> Utils.rebuildMatrix wVisibleUnitMatrix (nHidden + 1) (nVisible + 1)
                DeepBeliefNet.toRbm weightsAndBiases dWeightsAndBiases
        ) }

    let runTrainNeuralNetEpochTemplate (blockSize:int) = cuda {
        let! multiplyVectorByMatrixKernel = multiplyVectorByMatrixKernel blockSize |> Compiler.DefineKernel
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel
        let! sigmoidKernel = <@ sigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel
        let! dSigmoidKernel = <@ dSigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel |> Compiler.DefineKernel
        let! addVectorKernel = <@ pointwiseAdd @> |> pointwiseVectorOperationKernel blockSize |> Compiler.DefineKernel
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseVectorOperationKernel blockSize |> Compiler.DefineKernel
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseVectorOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply rngKernel
            let multiplyVectorByMatrixKernel = program.Apply multiplyVectorByMatrixKernel
            let multiplyVectorByTransposeOfMatrixKernel = program.Apply multiplyVectorByTransposeOfMatrixKernel
            let sigmoidKernel = program.Apply sigmoidKernel
            let dSigmoidKernel = program.Apply dSigmoidKernel
            let coerceKernel = program.Apply coerceKernel
            let addVectorKernel = program.Apply addVectorKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel

            fun (netProps : NnetProperties) trainingSet -> 
                let paddedWeights = netProps.Weights |> List.map (Utils.prependRowOfZeroes >> Utils.padToMultiplesOf blockSize)
                let weights = paddedWeights |> List.map (Utils.flattenMatrix >> worker.Malloc)
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize (Utils.height w) (Utils.width w))
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize (Utils.height w) (Utils.width w))
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (Utils.height w))
                let inputs0 = worker.Malloc<float32>(Utils.width paddedWeights.[0])
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let errorSignals = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let diffs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let N = weights.Length - 1
                let diffN = worker.Malloc<float32>(Utils.height paddedWeights.[N])

                for i in 0..Array.length trainingSet - 1 do
                    inputs0.Scatter(fst trainingSet.[i] |> Utils.padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        multiplyVectorByMatrixKernel.Launch forwardLp.[j] outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (Utils.height paddedWeights.[j]) (Utils.width paddedWeights.[j])
                        sigmoidKernel.Launch outputLp.[j] outputs.[j].Ptr outputs.[j].Ptr 1 (Utils.height netProps.Weights.[j])
                        dSigmoidKernel.Launch outputLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr 1 (Utils.height netProps.Weights.[j])
                        coerceKernel.Launch coerceLp outputs.[j].Ptr 0 1.0f
                        coerceKernel.Launch coerceLp dOutputs.[j].Ptr 0 0.0f

                    diffs.[N].Scatter (snd trainingSet.[i] |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)
                    subtractVectorKernel.Launch outputLp.[N] diffs.[N].Ptr diffs.[N].Ptr outputs.[N].Ptr (Utils.height paddedWeights.[N])
                    for j in N..(-1)..0 do
                        let weight = weights.[j].Gather() |> Utils.rebuildMatrix (Utils.width paddedWeights.[j]) (Utils.height paddedWeights.[j]) (Utils.width paddedWeights.[j])
                        if j < N then 
                            multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j] diffs.[j].Ptr weights.[j + 1].Ptr errorSignals.[j + 1].Ptr (Utils.height paddedWeights.[j + 1]) (Utils.width paddedWeights.[j + 1])
                        pointwiseMultiplyVectorKernel.Launch outputLp.[j] errorSignals.[j].Ptr dOutputs.[j].Ptr diffs.[j].Ptr (Utils.height paddedWeights.[N])
                        
                netProps
        ) }
