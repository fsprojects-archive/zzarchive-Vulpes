namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief.DeepBeliefNet
open DeepBelief.CudaTemplates
open DeepBelief.Kernels
open DeepBelief.Utils
open System

module Common =
    type SimpleMatrixOperationKernelSignature = deviceptr<float32> -> deviceptr<float32> -> unit
    let simpleMatrixOperation blockSize A B (kernel : Kernel<SimpleMatrixOperationKernelSignature>) (worker : Worker) =
        let hA = height A
        let wA = width A
        let paddedA = padToMultiplesOf blockSize A
        let paddedB = padToMultiplesOf blockSize B
        let hPaddedA = height paddedA
        let wPaddedA = width paddedA
        let flattenedA = flattenMatrix paddedA
        let flattenedB = flattenMatrix paddedB

        use flattenedA = worker.Malloc flattenedA
        use flattenedB = worker.Malloc flattenedB

        let lp = createSimpleMatrixOperationLp blockSize hPaddedA wPaddedA
        kernel.Launch lp flattenedA.Ptr flattenedB.Ptr

        flattenedA.Gather() |> rebuildMatrix wPaddedA hA wA

    type SimpleVectorOperationKernelSignature = deviceptr<float32> -> deviceptr<float32> -> deviceptr<float32> -> int -> unit
    let simpleVectorOperation blockSize x y (kernel : Kernel<SimpleVectorOperationKernelSignature>) (worker : Worker) =
        let size = Array.length x
        let paddedX = padToMultipleOf blockSize x
        let paddedY = padToMultipleOf blockSize y

        use paddedX = worker.Malloc paddedX
        use paddedY = worker.Malloc paddedY
        use result = worker.Malloc<float32> paddedX.Length

        let lp = createSimpleVectorOperationLp blockSize paddedX.Length
        kernel.Launch lp result.Ptr paddedX.Ptr paddedY.Ptr paddedX.Length

        let result = result.Gather() 
        Array.sub result 0 size

