namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Kernels
    open NeuralNet

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

    let runRbmEpochTemplate (blockSize:int) = cuda {
        let! multiplyKernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel
        let! activateKernel = <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel
        let! addKernel = addKernel blockSize |> Compiler.DefineKernel
        let! subtractKernel = subtractKernel blockSize |> Compiler.DefineKernel
        let! scalarMultiplyKernel = scalarMultiplyKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply rngKernel
            let multiplyKernel = program.Apply multiplyKernel
            let multiplyByTransposeKernel = program.Apply multiplyByTransposeKernel
            let transposeAndMultiplyKernel = program.Apply transposeAndMultiplyKernel
            let activateFirstRowKernel = program.Apply activateFirstRowKernel
            let activateFirstColumnKernel = program.Apply activateFirstColumnKernel
            let activateKernel = program.Apply activateKernel
            let addKernel = program.Apply addKernel
            let subtractKernel = program.Apply subtractKernel
            let scalarMultiplyKernel = program.Apply scalarMultiplyKernel

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
                    subtractKernel.Launch simpleWeightsLp c1.Ptr c2.Ptr hHiddenUnitMatrix wVisibleUnitMatrix
                    scalarMultiplyKernel.Launch simpleWeightsLp c1.Ptr weightedAlpha hHiddenUnitMatrix wVisibleUnitMatrix
                    scalarMultiplyKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr momentum hHiddenUnitMatrix wVisibleUnitMatrix
                    addKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr c1.Ptr hHiddenUnitMatrix wVisibleUnitMatrix

                    // weightsAndBiases -> weightsAndBiases + dWeightsAndBiases
                    addKernel.Launch simpleWeightsLp weightsAndBiases.Ptr dWeightsAndBiases.Ptr hHiddenUnitMatrix wVisibleUnitMatrix

                let weightsAndBiases = weightsAndBiases.Gather() |> Utils.rebuildMatrix wVisibleUnitMatrix |> Utils.topLeftSubmatrix (nHidden + 1) (nVisible + 1)
                let dWeightsAndBiases = dWeightsAndBiases.Gather() |> Utils.rebuildMatrix wVisibleUnitMatrix |> Utils.topLeftSubmatrix (nHidden + 1) (nVisible + 1)
                DeepBeliefNet.toRbm weightsAndBiases dWeightsAndBiases
        ) }

    let runTrainNeuralNetEpoch (blockSize:int) = cuda {
        let! multiplyKernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply rngKernel
            let multiplyKernel = program.Apply multiplyKernel

            fun (alpha:float32) dbn (netProps : NnetProperties) trainingSet -> 
                let weights = netProps.Weights |> List.map (Utils.flattenMatrix >> worker.Malloc)
                let inputs = netProps.Weights |> List.map (fun w -> worker.Malloc<float32>(1 + Utils.height w)) 
                let dInputs = netProps.Weights |> List.map (fun w -> worker.Malloc<float32>(1 + Utils.height w)) 

                for i in 0..Array.length trainingSet do
                    use inputs0 = fst trainingSet.[i] |> (fun (x : float32[]) -> worker.Malloc x)
                    (fst trainingSet.[0]).[0] = 0.0f |> ignore
                netProps
        ) }
