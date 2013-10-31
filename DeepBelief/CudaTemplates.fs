namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Kernels
    open Utils

    let max i (j : int) = Math.Max(i, j)

    let multiplyMatrices (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Matrix) (B:Matrix) ->

            let wA = width A
            let wB = width B
            let wC = wB
            let hC = height A

            let A = flatten A
            let B = flatten B

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let threads = dim3(blockSize, blockSize)
            let grid = dim3(wB / threads.x |> max 1, hC / threads.y |> max 1)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr wA wB
            let result = C.Gather()
            result

    let matrixMulTemplate (blockSize:int) = cuda {
        let! kernel = blockSize |> matrixMulKernel |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (B : Matrix) ->
                multiplyMatrices blockSize worker kernel A B
            ) }
