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

            let i = blockIdx.x * blockSize + threadIdx.x
            result.[i] <- if (%activationFunction) A.[i] < rnd.[i] then 0.0f else 1.0f
            @>

    let activateFirstRowKernel (blockSize:int) =
        <@ fun (result:deviceptr<float32>) (M:deviceptr<float32>) (wM:int) (nActivations:int) -> 
            let i = blockIdx.x * blockSize + threadIdx.x
            let rowIndex = i / wM
            let columnIndex = i % wM
            result.[i] <- if rowIndex = 0 then (if columnIndex < nActivations then 1.0f else 0.0f) else M.[i]
            @>

    let activateFirstColumnKernel (blockSize:int) =
        <@ fun (result:deviceptr<float32>) (M:deviceptr<float32>) (hM:int) (wM:int) (nActivations:int) -> 
            let i = blockIdx.x * blockSize + threadIdx.x
            let rowIndex = i / wM
            let columnIndex = i % wM
            result.[i] <- if columnIndex = 0 then (if rowIndex < nActivations then 1.0f else 0.0f) else M.[i]
            @>
