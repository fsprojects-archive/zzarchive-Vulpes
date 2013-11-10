namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Kernels

    let max i (j : int) = Math.Max(i, j)

    let multiplyMatrices (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
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

    let runEpoch (blockSize : int) (worker : Worker) (kernel:Kernel<RunEpochKernelSignature>) =
        fun (samples : Utils.Matrix[]) rbm ->
            let nSamples = samples |> Array.map (fun sample -> Utils.height sample) |> Array.sum
            let sampleSize = Utils.width samples.[0]

            let flattenedSamples = Utils.flattenSamples samples
            let flattenedRbm = DeepBeliefNet.flattenRbm rbm
            let sizeOfRbm = Array.length flattenedRbm
            
            use flattenedRbm = worker.Malloc(flattenedRbm)
            use flattenedSamples = worker.Malloc(flattenedSamples)
            use output = worker.Malloc<float32>(sizeOfRbm)

            let nVisible = Array.length rbm.VisibleBiases
            let nHidden = Array.length rbm.HiddenBiases

            let threads = dim3(blockSize, blockSize)
            let grid = dim3(sizeOfRbm / threads.x |> max 1, sizeOfRbm / threads.y |> max 1)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp nVisible nHidden flattenedRbm.Ptr flattenedSamples.Ptr output.Ptr
            output.Gather() |> DeepBeliefNet.rebuildRbm nVisible nHidden

    let matrixMulTemplate (blockSize:int) = cuda {
        let! kernel = blockSize |> matrixMulKernel |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                multiplyMatrices blockSize worker kernel A B
            ) }

    let runEpochTemplate (blockSize:int) = cuda {
        let! mulKernel = blockSize |> matrixMulKernel |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply(rngKernel)

            // Copy pre-calculated bit-matrices, needed for jump-ahead
            // calculations, to the device memory.
            let jumpAheadMatrices = worker.Malloc(Data.jumpAheadMatrices)

            fun (streams : int) steps seed runs rank -> 
                let state0 = Utils.generateStartState seed
                use state0 = worker.Malloc(state0)

                use numbers = worker.Malloc<float32>(streams * steps)

                let threads = dim3(blockSize, blockSize)
                let grid = dim3(1, 1)
                let lp = LaunchParam(grid, threads)
                rngKernel.Launch lp runs rank state0.Ptr jumpAheadMatrices.Ptr steps numbers.Ptr
                numbers.Gather()
        ) }
