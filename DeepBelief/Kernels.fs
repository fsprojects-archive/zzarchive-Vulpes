namespace DeepBelief

module Kernels = 

    open Alea.CUDA

    type MatrixMulKernelSignature = deviceptr<float32> -> deviceptr<float32> -> deviceptr<float32> -> int -> int -> unit

    let matrixMulKernel (blockSize:int) =
        <@ fun (C:deviceptr<float32>) (A:deviceptr<float32>) (B:deviceptr<float32>) (wA:int) (wB:int) ->
            // Block index
            let bx = blockIdx.x
            let by = blockIdx.y

            // Thread index
            let tx = threadIdx.x
            let ty = threadIdx.y

            // Index of the first sub-matrix of A processed by the block
            let aBegin = wA * blockSize * by

            // Index of the last sub-matrix of A processed by the block
            let aEnd = aBegin + wA - 1

            // Step size used to iterate through the sub-matrices of A
            let aStep = blockSize

            // Index of the first sub-matrix of B processed by the block
            let bBegin = blockSize * bx

            // Step size used to iterate through the sub-matrices of B
            let bStep = blockSize * wB

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
                    Csub <- Csub + As.[ty, k] * Bs.[k, tx]

                // Synchronize to make sure that the preceding
                // computation is done before loading two new
                // sub-matrices of A and B in the next iteration
                __syncthreads()

                a <- a + aStep
                b <- b + bStep

            // Write the block sub-matrix to device memory;
            // each thread writes one element
            let c = wB * blockSize * by + blockSize * bx
            C.[c + wB * ty + tx] <- Csub @>
