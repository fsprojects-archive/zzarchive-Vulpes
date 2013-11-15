namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Kernels

    let max i (j : int) = Math.Max(i, j)

    let loadAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Utils.Matrix) (B:Utils.Matrix) ->

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

            let threads = dim3(blockSize, blockSize)
            let grid = dim3(wB / threads.x, hC / threads.y)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            Utils.rebuildMatrix wC result |> Utils.topLeftSubmatrix finalHeight finalWidth

    let loadAndMultiplyByTranspose (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Utils.Matrix) (B:Utils.Matrix) ->

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

            let threads = dim3(blockSize, blockSize)
            let grid = dim3(hB / threads.x, hC / threads.y)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            Utils.rebuildMatrix wC result |> Utils.topLeftSubmatrix finalHeight finalWidth

    let loadTransposeAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Utils.Matrix) (B:Utils.Matrix) ->

            let finalHeight = Utils.width A
            let finalWidth = Utils.width B

            let A = Utils.padToMultiplesOf blockSize A
            let B = Utils.padToMultiplesOf blockSize B

            let hA = Utils.height A
            let wA = Utils.width A
            let hB = Utils.height B
            let wB = Utils.width B
            let wC = wB
            let hC = Utils.width A

            let A = Utils.flattenMatrix A
            let B = Utils.flattenMatrix B

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let threads = dim3(blockSize, blockSize)
            let grid = dim3(hB / threads.x, hC / threads.y)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            Utils.rebuildMatrix wC result |> Utils.topLeftSubmatrix finalHeight finalWidth

    let loadAndMultiplyTemplate (blockSize:int) = cuda {
        let! kernel = multiplyStrategy |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                loadAndMultiply blockSize worker kernel A B
            ) }

    let loadAndMultiplyByTransposeTemplate (blockSize:int) = cuda {
        let! kernel = multiplyByTransposeStrategy |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                loadAndMultiplyByTranspose blockSize worker kernel A B
            ) }

    let loadTransposeAndMultiplyTemplate (blockSize:int) = cuda {
        let! kernel = transposeAndMultiplyStrategy |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                loadTransposeAndMultiply blockSize worker kernel A B
            ) }

    // This template, which finds the n-th power of a square matrix,
    // shows how launch logic can be reused within the CUDA monad.
    // The same launch parameters are used in each iteration, and the
    // inputs of the launcher are addresses in the GPU memory.  This
    // means that there is no copying of data from the CPU to the GPU
    // throughout the loop.
    let powerOfNTemplate (blockSize : int) = cuda {
        let! kernel = multiplyStrategy |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) n ->
                let originalSize = Utils.width A
                let A = Utils.padToMultiplesOf blockSize A
                let paddedSize = Utils.width A
                let A = Utils.flattenMatrix A
                let Ai = Utils.identityMatrix paddedSize |> Utils.flattenMatrix

                use A = worker.Malloc(A)
                use Ai = worker.Malloc(Ai)

                let threads = dim3(blockSize, blockSize)
                let grid = dim3(paddedSize / threads.x |> max 1, paddedSize / threads.y |> max 1)
                let lp = LaunchParam(grid, threads)

                for i = 1 to n do
                    kernel.Launch lp Ai.Ptr A.Ptr Ai.Ptr paddedSize paddedSize
                Ai.Gather() |> Utils.rebuildMatrix paddedSize |> Utils.topLeftSubmatrix originalSize originalSize
            ) }

    let runDbnEpochTemplate (blockSize:int) = cuda {
        let! multplyKernel = multiplyStrategy |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyStrategy |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply rngKernel
            let multiplyByTransposeKernel = program.Apply multiplyByTransposeKernel
            let activateFirstRowKernel = program.Apply activateFirstRowKernel

            // Copy pre-calculated bit-matrices, needed for jump-ahead
            // calculations, to the device memory.
            let jumpAheadMatrices = worker.Malloc(Data.jumpAheadMatrices)

            fun alpha momentum batchSize rbm xInputs -> 
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

                let fowardMatrixGrid = dim3(heightOfVisibleUnitMatrix / threads.x |> max 1, weightsAndBiasesWidth / threads.y |> max 1)
                let forwardMatrixLp = LaunchParam(fowardMatrixGrid, threads)

                let activateFirstRowGrid = dim3(heightOfHiddenUnitMatrix / threads.x |> max 1, widthOfHiddenUnitMatrix / threads.y |> max 1)
                let activateFirstRowLp = LaunchParam(activateFirstRowGrid, threads)

                let rngNumStreams = 1024
                let rngBlockSize = dim3(32, 8)
                let rngNumThreadsPerBlock = rngBlockSize.Size
                let rngGridSize = dim3(rngNumStreams / rngNumThreadsPerBlock)
                let rngSharedMemorySize = XorShift7.Size * rngNumThreadsPerBlock
                let rngLp = LaunchParam(rngGridSize, rngBlockSize, rngSharedMemorySize)

                use state0 = Utils.generateStartState 42u |> worker.Malloc

                for i in 0..samples.Length - 1 do
                    
                    use v1 = samples.[0]
                    // Perform the forward iteration to populate h1
                    multiplyByTransposeKernel.Launch forwardMatrixLp h1.Ptr weightsAndBiases.Ptr v1.Ptr weightsAndBiasesWidth widthOfVisibleUnitMatrix
                    // rngKernel.Launch rngLp samples.Length i state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    // activateFirstRowKernel.Launch activateFirstRowLp h1.Ptr widthOfHiddenUnitMatrix nRows
                    
                    let h = h1.Gather()
                    let v = v1.Gather()
                    let w = weightsAndBiases.Gather()

                    let hNans = h |> Array.filter Single.IsNaN
                    let vNans = h |> Array.filter Single.IsNaN
                    let wNans = h |> Array.filter Single.IsNaN

                    h.[0] <- h.[0] + 0.0f

                alpha
        ) }
