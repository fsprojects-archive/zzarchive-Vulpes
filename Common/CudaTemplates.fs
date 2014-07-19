namespace Common

module CudaTemplates =

    open Alea.CUDA
    open Analytics
    open Utils

    type Matrix with
        member this.PadToMultiplesOf n =
            match this with
                Matrix matrix ->
                    let h = Array2D.length1 matrix
                    let w = Array2D.length2 matrix
                    let paddedHeight = nextMultipleOf n h
                    let paddedWidth = nextMultipleOf n w
                    Array2D.init paddedHeight paddedWidth 
                        (fun i j -> if i < h && j < w then matrix.[i, j] else 0.0f) |> Matrix
        member this.ToRowMajorFormat =
            match this with
                Matrix matrix ->
                    let h = this.Height
                    let w = this.Width
                    Array.init (h*w) (fun i -> matrix.[i / w, i % w])
        static member FromRowMajorFormat width (array : float32[]) = 
            Array2D.init (array.Length / width) width (fun i j -> array.[i * width + j]) |> Matrix

    type WeightsAndBiases with
        member this.PadToMultiplesOf blockSize =
            match this with WeightsAndBiases weightsAndBiases -> weightsAndBiases.PadToMultiplesOf blockSize

    type WeightChanges with
        member this.PadToMultiplesOf blockSize =
            match this with WeightChanges weightChanges -> weightChanges.PadToMultiplesOf blockSize

    let createOffsetMatrixOperationLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(((hA - 1) * wA) / threads.x)
        LaunchParam(grid, threads)

    let coerceLp blockSize =
        let threads = dim3(blockSize)
        let grid = dim3(1)
        LaunchParam(grid, threads)

    let createMultiplyVectorByMatrixLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(hA / threads.x)
        LaunchParam(grid, threads)

    let createMultiplyVectorByTransposeOfMatrixLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(wA / threads.x)
        LaunchParam(grid, threads)

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

    let createSimpleVectorOperationLp blockSize size =
        let threads = dim3(blockSize)
        let grid = dim3(size / threads.x)
        LaunchParam(grid, threads)

    let createOuterProductLp blockSize hA wA =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(hA / threads.x, wA / threads.y)
        LaunchParam(grid, threads)

    let createSimpleMatrixOperationLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3((hA * wA) / threads.x)
        LaunchParam(grid, threads)
