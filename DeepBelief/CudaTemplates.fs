namespace DeepBelief

module CudaTemplates =

    open Alea.CUDA
    open Kernels
    open Utils

    let multiplyMatrices (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:float32[]) (B:float32[]) (wA:int) (wB:int) ->
            let wC = wB
            let hC = A.Length / wA

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let threads = dim3(blockSize, blockSize)
            let grid = dim3(wB / threads.x, hC / threads.y)
            let lp = LaunchParam(grid, threads)
            kernel.Launch lp C.Ptr A.Ptr B.Ptr wA wB
            C.Gather()

    let matrixMulTemplate (blockSize:int) = cuda {
        let! kernel = blockSize |> matrixMulKernel |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)
            let gpuCalc = multiplyMatrices blockSize worker kernel

            let run (dimA:int*int) (dimB:int*int) =
                let wA, hA = dimA
                let wB, hB = dimB

                let sizeA = wA * hA
                let sizeB = wB * hB

                let A = Array.init sizeA (fun _ -> 1.0f)
                let B = Array.init sizeB (fun _ -> 0.01f)

                gpuCalc A B wA wB
            run ) }
