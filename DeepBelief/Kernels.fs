namespace DeepBelief

module Kernels = 

    open Microsoft.FSharp.Quotations
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Common.Kernels

    type ActivationFunction = Expr<float32 -> float32>

    let activateKernel (blockSize : int) (activationFunction : ActivationFunction) =
        let strategy = multiplyStrategy blockSize
        <@ fun (result : deviceptr<float32>) (A : deviceptr<float32>) (rnd : deviceptr<float32>) ->

            // Block index
            let bx = blockIdx.x

            // Thread index
            let tx = threadIdx.x

            let i = bx * blockSize + tx;
            result.[i] <- if (%activationFunction) A.[i] < rnd.[i] then 0.0f else 1.0f
            __syncthreads() @>

    let activateFirstRowKernel (blockSize:int) =
        <@ fun (M:deviceptr<float32>) (wM:int) (nActivations:int) -> 
            // Block index
            let bx = blockIdx.x
            // Thread index
            let tx = threadIdx.x

            let start = blockSize * bx
            let i = start + tx
            M.[i] <- if i < nActivations then 1.0f else 0.0f
            @>

    let activateFirstColumnKernel (blockSize:int) =
        <@ fun (M:deviceptr<float32>) (hM:int) (wM:int) (nActivations:int) -> 
            // Block index
            let bx = blockIdx.x
            // Thread index
            let tx = threadIdx.x

            let start = wM * blockSize * bx
            let i = start + wM * tx
            let max = nActivations * wM
            M.[i] <- if i < max then 1.0f else 0.0f
            @>

