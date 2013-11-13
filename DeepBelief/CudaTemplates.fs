namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Kernels

    let max i (j : int) = Math.Max(i, j)

    let loadAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Utils.Matrix) (B:Utils.Matrix) ->

            let finalHeight = Utils.height A
            let finalWidth = Utils.width B

            let A = Utils.padToMultiplesOf blockSize A
            let B = Utils.padToMultiplesOf blockSize B

            let wA = Utils.width A
            let wB = Utils.width B
            let wC = wB
            let hC = Utils.height A

            let A = Utils.flattenMatrix A
            let B = Utils.flattenMatrix B

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let threads = dim3(blockSize, blockSize)
            let grid = dim3(wB / threads.x |> max 1, hC / threads.y |> max 1)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr wA wB
            let result = C.Gather()
            Utils.rebuildMatrix wC result |> Utils.topLeftSubmatrix finalHeight finalWidth

    let loadAndMultiplyByTranspose (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Utils.Matrix) (B:Utils.Matrix) ->

            let finalHeight = Utils.height A
            let finalWidth = Utils.height B

            let A = Utils.padToMultiplesOf blockSize A
            let B = Utils.padToMultiplesOf blockSize B

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
            let grid = dim3(hB / threads.x |> max 1, hC / threads.y |> max 1)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr wA wB
            let result = C.Gather()
            Utils.rebuildMatrix wC result |> Utils.topLeftSubmatrix finalHeight finalWidth

    let loadTransposeAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Utils.Matrix) (B:Utils.Matrix) ->

            let finalHeight = Utils.width A
            let finalWidth = Utils.width B

            let A = Utils.padToMultiplesOf blockSize A
            let B = Utils.padToMultiplesOf blockSize B

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
            let grid = dim3(hB / threads.x |> max 1, hC / threads.y |> max 1)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr wA wB
            let result = C.Gather()
            Utils.rebuildMatrix wC result |> Utils.topLeftSubmatrix finalHeight finalWidth

    let loadAndMultiplyTemplate (blockSize:int) = cuda {
        let! kernel = multiplyElement |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                loadAndMultiply blockSize worker kernel A B
            ) }

    let loadAndMultiplyByTransposeTemplate (blockSize:int) = cuda {
        let! kernel = multiplyByTransposeElement |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                loadAndMultiplyByTranspose blockSize worker kernel A B
            ) }

    let loadTransposeAndMultiplyTemplate (blockSize:int) = cuda {
        let! kernel = transposeAndMultiplyElement |> matrixMulKernel blockSize |> Compiler.DefineKernel

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
        let! kernel = multiplyElement |> matrixMulKernel blockSize |> Compiler.DefineKernel

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
        let! multplyKernel = multiplyElement |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeElement |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyElement |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply(rngKernel)

            // Copy pre-calculated bit-matrices, needed for jump-ahead
            // calculations, to the device memory.
            let jumpAheadMatrices = worker.Malloc(Data.jumpAheadMatrices)

            fun alpha momentum batchSize (rbm : DeepBeliefNet.RestrictedBoltzmannMachine) xInputs -> 
                let nRows = Utils.height xInputs
                let nCols = Utils.width xInputs
                let xRand = Utils.permuteRows Utils.rand xInputs
                let samples = xRand |> Utils.batchesOf batchSize |> Array.map array2D
                let nHidden = Array.length rbm.HiddenBiases
                let nVisible = Array.length rbm.VisibleBiases
                let sizeOfHiddenUnitVectors = 1 + nHidden
                let sizeOfVisibleUnitVectors = 1 + nVisible
                let sizeOfHiddenBatch = sizeOfHiddenUnitVectors * batchSize
                let sizeOfVisibleBatch = sizeOfVisibleUnitVectors * batchSize
                let sizeOfWeightsAndBiases = sizeOfHiddenUnitVectors * sizeOfVisibleUnitVectors

                let weightsAndBiases = DeepBeliefNet.toWeightsAndBiases rbm |> Utils.padToMultiplesOf blockSize |> Utils.flattenMatrix
                let dWeightsAndBiases = DeepBeliefNet.toDWeightsAndBiases rbm |> Utils.padToMultiplesOf blockSize |> Utils.flattenMatrix

                use weightsAndBiases = worker.Malloc weightsAndBiases
                use dWeightsAndBiases = worker.Malloc dWeightsAndBiases
                use v1 = worker.Malloc<float32>(sizeOfVisibleBatch)
                use h1 = worker.Malloc<float32>(sizeOfHiddenBatch)
                use v2 = worker.Malloc<float32>(sizeOfVisibleBatch)
                use h2 = worker.Malloc<float32>(sizeOfHiddenBatch)
                use c1 = worker.Malloc<float32>(sizeOfWeightsAndBiases)
                use c2 = worker.Malloc<float32>(sizeOfWeightsAndBiases)

                for sample in samples do
                    sample.[0, 0] <- 1.0f
                alpha
        ) }
