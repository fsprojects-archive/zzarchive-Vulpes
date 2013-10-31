namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief.CudaTemplates
//open DeepBelief.Utils

type ``Matrix Multiplication`` () =

    let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ]
    let B = array2D [ [1.0f; 2.0f]; [3.0f; 4.0f]; [5.0f; 6.0f] ]
    let C = array2D [ [22.0f; 28.0f]; [49.0f; 64.0f] ]

    let blockSize = 32
    let multiplyMatricesProgram = blockSize |> matrixMulTemplate |> Compiler.load Worker.Default

//    let matrixMulTemplate() = cuda {
//        let! kernel = matrixMulKernel() |> defineKernelFunc
//
////        return PFunc(fun (m:Module) (A:float32[,]) (B:float32[,]) ->
//        return PFunc(fun (m:Module) (A:float32[]) (B:float32[]) (wA:int) (wB:int) ->
//            let kernel = kernel.Apply m
////            let wB = width B
////            let wA = width A
////            let flattenedA = flatten A
////            let flattenedB = flatten B
////            let wC = wB
////            let hC = height A
//            let wC = wB
//            let hC = A.Length / wA
//            use dA = m.Worker.Malloc(A)
//            use dB = m.Worker.Malloc(B)
//            use dC = m.Worker.Malloc<float32>(wC * hC)
//            let lp = LaunchParam(dim3(wC/blockSize, hC/blockSize), dim3(blockSize, blockSize))
//
//            kernel.Launch lp dC.Ptr dA.Ptr dB.Ptr wA wB
//            buildMatrix wC (dC.ToHost())) }
//
//    let worker = new DeviceWorker(Device(0))
    //let x = matrixMulTemplate
    //let matrixMulModule = worker.LoadPModule(matrixMulTemplate)


    [<Fact>] member test.
        ``The matrixMulModule multiplies A by B.``() =
            ((320, 320), (640, 320)) ||> multiplyMatricesProgram.Run
           // matrixMulModule.Invoke (flatten A) (flatten B) 3 2 |> should equal C

