namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Kernels

    let max i (j : int) = Math.Max(i, j)

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

    let createActivateLp blockSize hA wA =
        createMultiplyLp blockSize hA wA hA wA

    let createActivateFirstRowLp blockSize hM wM =
        let threads = dim3(blockSize)
        let grid = dim3(wM / threads.x)
        LaunchParam(grid, threads)

    let createActivateFirstColumnLp blockSize hM wM =
        let threads = dim3(blockSize)
        let grid = dim3(hM / threads.x)
        LaunchParam(grid, threads)

    let runDbnEpochTemplate (blockSize:int) = cuda {
        let! multiplyKernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel
        let! activateKernel = <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply rngKernel
            let multiplyKernel = program.Apply multiplyKernel
            let multiplyByTransposeKernel = program.Apply multiplyByTransposeKernel
            let transposeAndMultiplyKernel = program.Apply transposeAndMultiplyKernel
            let activateFirstRowKernel = program.Apply activateFirstRowKernel
            let activateFirstColumnKernel = program.Apply activateFirstColumnKernel
            let activateKernel = program.Apply activateKernel

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
                
                let heightOfVisibleUnitMatrix = paddedSampleHeight
                let widthOfVisibleUnitMatrix = paddedSampleWidth

                let widthOfHiddenUnitMatrix = heightOfVisibleUnitMatrix
                let heightOfHiddenUnitMatrix = 1 + nHidden |> Utils.nextMultipleOf blockSize

                let dimVisibleUnits = heightOfVisibleUnitMatrix * widthOfVisibleUnitMatrix
                let dimHiddenUnits = heightOfHiddenUnitMatrix * widthOfHiddenUnitMatrix

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

                let forwardMatrixLp = createMultiplyByTransposeLp blockSize weightsAndBiasesHeight weightsAndBiasesWidth heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix
                let backwardMatrixLp = createTransposeAndMultiplyLp blockSize heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                let activateHiddenLp = createActivateLp blockSize heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix
                let activateVisibleLp = createActivateLp blockSize heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix
                let activateFirstRowLp = createActivateFirstRowLp blockSize heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix
                let activateFirstColumnLp = createActivateFirstColumnLp blockSize heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix
                let computeCValueLp = createMultiplyLp blockSize heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix

                let rngNumStreams = 1024
                let rngBlockSize = dim3(32, 8)
                let rngNumThreadsPerBlock = rngBlockSize.Size
                let rngGridSize = dim3(rngNumStreams / rngNumThreadsPerBlock)
                let rngSharedMemorySize = XorShift7.Size * rngNumThreadsPerBlock
                let rngLp = LaunchParam(rngGridSize, rngBlockSize, rngSharedMemorySize)

                use state0 = Utils.generateStartState 42u |> worker.Malloc

                let numRuns = 3 * samples.Length
                for i in 0..samples.Length - 1 do
                    
                    use v1 = samples.[0]
                    // Perform the forward iteration to populate h1
                    multiplyByTransposeKernel.Launch forwardMatrixLp h1.Ptr weightsAndBiases.Ptr v1.Ptr weightsAndBiasesHeight weightsAndBiasesWidth heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns i state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h1.Ptr hiddenRandoms.Ptr heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix
                    activateFirstRowKernel.Launch activateFirstRowLp h1.Ptr widthOfHiddenUnitMatrix nRows

                    // Perform the backward iteration to populate v2
                    transposeAndMultiplyKernel.Launch backwardMatrixLp v2.Ptr h1.Ptr weightsAndBiases.Ptr heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                    rngKernel.Launch rngLp numRuns (i + samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimVisibleUnits / rngNumStreams) visibleRandoms.Ptr
                    activateKernel.Launch activateVisibleLp v2.Ptr visibleRandoms.Ptr heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix
                    activateFirstColumnKernel.Launch activateFirstColumnLp v2.Ptr heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix nCols

                    // Perform the forward iteration to populate h2
                    multiplyByTransposeKernel.Launch forwardMatrixLp h2.Ptr weightsAndBiases.Ptr v2.Ptr weightsAndBiasesHeight weightsAndBiasesWidth heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns (i + 2 * samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h2.Ptr hiddenRandoms.Ptr heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix
                    activateFirstRowKernel.Launch activateFirstRowLp h2.Ptr widthOfHiddenUnitMatrix nRows

                    // Compute c1 and c2
                    multiplyKernel.Launch computeCValueLp c1.Ptr h1.Ptr v1.Ptr heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix
                    multiplyKernel.Launch computeCValueLp c2.Ptr h2.Ptr v2.Ptr heightOfHiddenUnitMatrix widthOfHiddenUnitMatrix heightOfVisibleUnitMatrix widthOfVisibleUnitMatrix

                    let x = c1.Gather()
                    let y = c2.Gather()

                    let xNan = x |> Array.filter Single.IsNaN
                    let yNan = y |> Array.filter Single.IsNaN
                    x.[0] <- x.[0]
                alpha
        ) }
