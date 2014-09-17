namespace Common

module Kernels = 

    open Microsoft.FSharp.Quotations
    open Alea.CUDA
    open Alea.CUDA.Utilities

    type MatrixMulKernelSignature = deviceptr<float32> -> deviceptr<float32> -> deviceptr<float32> -> int -> int -> int -> int -> unit

    let [<ReflectedDefinition>] sigmoid x = 1.0f / (1.0f + exp(-x))
    let [<ReflectedDefinition>] dSigmoid s x = s * (1.0f - s)

    let [<ReflectedDefinition>] pointwiseAdd (a : float32) (b : float32) = a + b
    let [<ReflectedDefinition>] pointwiseSubtract (a : float32) (b : float32) = a - b
    let [<ReflectedDefinition>] pointwiseMultiply (a : float32) (b : float32) = a * b

    type PointwiseOperation = Expr<float32 -> float32 -> float32>
    type TransformationFunction = Expr<float32 -> float32>
    type SecondOrderTransformationFunction = Expr<float32 -> float32 -> float32>

    // a quotation that: fun height width blockIndex -> begin, end, step
    type IterationStrategy = Expr<int -> int -> int -> int * int * int>

    // a quotation that: fun A B ty k tx -> result
    type MultiplyElement = Expr<float32[,] -> float32[,] -> int -> int -> int -> float32>

    type CUpdate = Expr<int -> int -> int -> int -> int -> int -> int -> int>

    type Strategy =
        {
            BlockSize : int
            AIteration : IterationStrategy
            BIteration : IterationStrategy
            MultiplyElement : MultiplyElement
            CUpdate : CUpdate
        }

    let multiplyStrategy blockSize =
        let aIteration = 
            <@ fun height width blockIndex ->
                let b = width * blockSize * blockIndex
                let e = width * blockSize * blockIndex + width - 1
                let s = blockSize
                b, e, s @>

        let bIteration =
            <@ fun height width blockIndex ->
                let b = blockSize * blockIndex
                let e = blockSize * blockIndex + width * (height - blockSize)
                let s = blockSize * width
                b, e, s @>

        let multiplyElement = <@ fun (A:float32[,]) (B:float32[,]) ty k tx -> A.[ty, k] * B.[k, tx] @>
        let cUpdate = <@ fun hB wB blockSize yBlockIndex yThreadIndex xBlockIndex xThreadIndex -> wB * (blockSize * yBlockIndex + yThreadIndex) + (blockSize * xBlockIndex + xThreadIndex) @>
        {
            BlockSize = blockSize
            AIteration = aIteration
            BIteration = bIteration
            MultiplyElement = multiplyElement
            CUpdate = cUpdate
        }

    let coerceKernel (blockSize : int) =
        <@ fun (X : deviceptr<float32>) (minIndex : int) (maxIndex : int) (value : float32) ->

            let index = (blockIdx.x * blockSize + threadIdx.x)
            if index >= minIndex && index <= maxIndex then
                X.[index] <- value @>

    let transformKernel (blockSize : int) (transformationFunction : TransformationFunction) =
        <@ fun (tX : deviceptr<float32>) (X : deviceptr<float32>) (startIndex : int) (size : int) ->

            // Block index
            let bx = blockIdx.x

            // Thread index
            let tx = threadIdx.x

            let i = bx * blockSize + tx;

            // Write the block sub-matrix to device memory;
            // each thread writes one element
            if i >= startIndex && i < startIndex + size then
                tX.[i] <- (%transformationFunction) X.[i]
            else
                tX.[i] <- 0.0f
            __syncthreads() @>

    let pointwiseBinaryOperationKernel (blockSize : int) (operation : PointwiseOperation) =
        <@ fun (result : deviceptr<float32>) (lhs : deviceptr<float32>) (rhs : deviceptr<float32>) ->
            let i = blockIdx.x * blockSize + threadIdx.x;
            result.[i] <- (%operation) lhs.[i] rhs.[i]
        @>

    let copyKernel (blockSize : int) =
        <@ fun (dest : deviceptr<float32>) (src : deviceptr<float32>) (baseIndex : int) ->
            let i = blockIdx.x * blockSize + threadIdx.x;
            dest.[i] <- src.[baseIndex + i]
        @>

    let multiplyByTransposeStrategy blockSize =
        let aIteration = 
            <@ fun height width blockIndex ->
                let b = width * blockSize * blockIndex
                let e = width * blockSize * blockIndex + width - 1
                let s = blockSize
                b, e, s @>

        let bIteration =
            <@ fun height width blockIndex ->
                let b = width * blockSize * blockIndex
                let e = width * blockSize * blockIndex + width - 1
                let s = blockSize
                b, e, s @>

        let multiplyElement = <@ fun (A:float32[,]) (B:float32[,]) ty k tx -> A.[ty, k] * B.[tx, k] @>
        let cUpdate = <@ fun hB wB blockSize yBlockIndex yThreadIndex xBlockIndex xThreadIndex -> hB * (blockSize * yBlockIndex + yThreadIndex) + (blockSize * xBlockIndex + xThreadIndex) @>
        {
            BlockSize = blockSize
            AIteration = aIteration
            BIteration = bIteration
            MultiplyElement = multiplyElement
            CUpdate = cUpdate
        }

    let transposeAndMultiplyStrategy blockSize =
        let aIteration = 
            <@ fun height width blockIndex ->
                let b = blockSize * blockIndex
                let e = blockSize * blockIndex + width * (height - blockSize)
                let s = blockSize * width
                b, e, s @>

        let bIteration =
            <@ fun height width blockIndex ->
                let b = blockSize * blockIndex
                let e = blockSize * blockIndex + width * (height - blockSize)
                let s = blockSize * width
                b, e, s @>

        let multiplyElement = <@ fun (A:float32[,]) (B:float32[,]) ty k tx -> A.[k, ty] * B.[k, tx] @>
        let cUpdate = <@ fun hB wB blockSize yBlockIndex yThreadIndex xBlockIndex xThreadIndex -> wB * (blockSize * yBlockIndex + yThreadIndex) + (blockSize * xBlockIndex + xThreadIndex) @>
        {
            BlockSize = blockSize
            AIteration = aIteration
            BIteration = bIteration
            MultiplyElement = multiplyElement
            CUpdate = cUpdate
        }

    let matrixMulKernel (blockSize:int) (strategy:Strategy) =
        <@ fun (C:deviceptr<float32>) (A:deviceptr<float32>) (B:deviceptr<float32>) (hA:int) (wA:int) (hB:int) (wB:int) ->

            // Block index
            let bx = blockIdx.x
            let by = blockIdx.y

            // Thread index
            let tx = threadIdx.x
            let ty = threadIdx.y

            let aBegin, aEnd, aStep = (%strategy.AIteration) hA wA by
            let bBegin, bEnd, bStep = (%strategy.BIteration) hB wB bx

            // Csub is used to store the element of the block sub-matrix
            // that is computed by the thread
            let mutable Csub = 0.0f

            // Loop over all the sub-matrices of A and B
            // required to compute the block sub-matrix
            let mutable a = aBegin
            let mutable b = bBegin
            while a <= aEnd do
            
                // Declaration of the shared memory array As used to
                // store the sub-matrix of A
                let As = __shared__.Array2D(blockSize, blockSize)

                // Declaration of the shared memory array Bs used to
                // store the sub-matrix of B
                let Bs = __shared__.Array2D(blockSize, blockSize)

                // Load the matrices from device memory
                // to shared memory; each thread loads
                // one element of each matrix
                As.[ty, tx] <- A.[a + wA * ty + tx]
                Bs.[ty, tx] <- B.[b + wB * ty + tx]

                // Synchronize to make sure the matrices are loaded
                __syncthreads()

                // Multiply the two matrices together;
                // each thread computes one element
                // of the block sub-matrix
                for k = 0 to blockSize - 1 do
                    Csub <- Csub + ((%strategy.MultiplyElement) As Bs ty k tx)

                // Synchronize to make sure that the preceding
                // computation is done before loading two new
                // sub-matrices of A and B in the next iteration
                __syncthreads()

                a <- a + aStep
                b <- b + bStep

            // Write the block sub-matrix to device memory;
            // each thread writes one element
            C.[(%strategy.CUpdate) hB wB blockSize by ty bx tx] <- Csub @>

    let multiplyVectorByMatrixKernel (blockSize:int) =
        <@ fun (y:deviceptr<float32>) (A:deviceptr<float32>) (x:deviceptr<float32>) (hA:int) (wA:int) ->

            let Xds = __shared__.Array(blockSize);

            let bx = blockIdx.x
            let tx = threadIdx.x

            let row = bx * blockSize + tx;

            let mutable value = 0.0f

            let mutable m = 0
            let upperBound = (wA - 1)/blockSize
            for m in 0..upperBound do
                
                Xds.[tx] <- if (m * blockSize + tx < wA) then x.[m * blockSize + tx] else 0.0f
                __syncthreads()
                
                for k in 0..(blockSize - 1) do
                    value <- value + (if row < hA && m * blockSize + k < wA then A.[row * wA + m * blockSize + k] * Xds.[k] else 0.0f)
                __syncthreads()

            y.[row] <- value
            __syncthreads() @>

    let multiplyVectorByTransposeOfMatrixKernel (blockSize:int) =
        <@ fun (y:deviceptr<float32>) (A:deviceptr<float32>) (x:deviceptr<float32>) (hA:int) (wA:int) ->
            let Xds = __shared__.Array(blockSize);

            // Block index
            let bx = blockIdx.x

            // Thread index
            let tx = threadIdx.x

            let column = bx * blockSize + tx;

            let mutable value = 0.0f

            let mutable m = 0
            let upperBound = (hA - 1)/blockSize
            for m in 0..upperBound do

                if (m * blockSize + tx < hA) then
                    Xds.[tx] <-  x.[m * blockSize + tx]
                    __syncthreads()
                
                    for k in 0..(blockSize - 1) do
                        value <- value + (if column < wA && m * blockSize + k < hA then A.[column + wA * (m * blockSize + k)] * Xds.[k] else 0.0f)
                    __syncthreads()

            if column < wA then y.[column] <- value
            __syncthreads() 
        @>
    let outerProductKernel (blockSize : int) =
        let strategy = multiplyStrategy blockSize
        <@ fun (A : deviceptr<float32>) (v : deviceptr<float32>) (w : deviceptr<float32>) (wA : int) ->
            
            let bx = blockIdx.x
            let by = blockIdx.y

            let tx = threadIdx.x
            let ty = threadIdx.y

            let i = bx * blockSize + tx;
            let j = by * blockSize + ty;

            A.[i * wA + j] <- v.[i] * w.[j]
            @>

    let scalarMultiplyMatrixKernel (blockSize : int) =
        let strategy = multiplyStrategy blockSize
        <@ fun (A : deviceptr<float32>) (lambda : float32) ->
            
            // Block index
            let bx = blockIdx.x

            // Thread index
            let tx = threadIdx.x

            let i = bx * blockSize + tx;

            // Write the block sub-matrix to device memory;
            // each thread writes one element
            A.[i] <- A.[i] * lambda @>

    let multiplyVectorByMatrixAndTransformKernel (blockSize:int) (transformationFunction : TransformationFunction) =
        <@ fun (y:deviceptr<float32>) (A:deviceptr<float32>) (x:deviceptr<float32>) (hA:int) (wA:int) ->

            let Xds = __shared__.Array(blockSize);

            let bx = blockIdx.x
            let tx = threadIdx.x

            let row = bx * blockSize + tx;

            let mutable value = 0.0f

            let mutable m = 0
            let upperBound = (wA - 1)/blockSize
            for m in 0..upperBound do
                
                Xds.[tx] <- if (m * blockSize + tx < wA) then x.[m * blockSize + tx] else 0.0f
                __syncthreads()
                
                for k in 0..(blockSize - 1) do
                    value <- value + (if row < hA && m * blockSize + k < wA then A.[row * wA + m * blockSize + k] * Xds.[k] else 0.0f)
                __syncthreads()

            y.[row] <- (%transformationFunction) value
            __syncthreads() @>

    let multiplyVectorByMatrixAndTransformTwiceKernel (blockSize:int) (transformationFunction1 : TransformationFunction) (transformationFunction2 : SecondOrderTransformationFunction) =
        <@ fun (y2:deviceptr<float32>) (y1:deviceptr<float32>) (A:deviceptr<float32>) (x:deviceptr<float32>) (hA:int) (wA:int) ->

            let Xds = __shared__.Array(blockSize);

            let bx = blockIdx.x
            let tx = threadIdx.x

            let row = bx * blockSize + tx;

            let mutable value = 0.0f

            let mutable m = 0
            let upperBound = (wA - 1)/blockSize
            for m in 0..upperBound do
                
                Xds.[tx] <- if (m * blockSize + tx < wA) then x.[m * blockSize + tx] else 0.0f
                __syncthreads()
                
                for k in 0..(blockSize - 1) do
                    value <- value + (if row < hA && m * blockSize + k < wA then A.[row * wA + m * blockSize + k] * Xds.[k] else 0.0f)
                __syncthreads()

            y1.[row] <- (%transformationFunction1) value
            y2.[row] <- (%transformationFunction2) y1.[row] value
            __syncthreads() @>
