// The MIT License (MIT)
// 
// Copyright (c) 2014 SpiegelSoft Ltd
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open Common
open DeepBelief.CudaTemplates
open DeepBelief.Kernels
open DeepBelief.Utils
open TestUtils

type ``CUDA Matrix Multiplication``()=

    let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ]
    let B = array2D [ [1.0f; 2.0f]; [3.0f; 4.0f]; [5.0f; 6.0f] ]
    let C = array2D [ [22.0f; 28.0f]; [49.0f; 64.0f] ]
    let x = [|7.0f; 8.0f; 9.0f|]
    let y = [|50.0f; 122.0f|]
    
    let a = [|1.0f; 2.0f; 3.0f|]
    let b = [|4.0f; 5.0f; 6.0f|]
    let c = [|4.0f; 5.0f|]

    let aPlusb = [|5.0f; 7.0f; 9.0f|]
    let bMinusa = [|3.0f; 3.0f; 3.0f|]
    let aPointwiseTimesb = [|4.0f; 10.0f; 18.0f|]

    let aOuterProductc = array2D [ [4.0f; 5.0f]
                                   [8.0f; 10.0f]
                                   [12.0f; 15.0f] ]
    let cOuterProducta = array2D [ [4.0f; 8.0f; 12.0f]
                                   [5.0f; 10.0f; 15.0f] ]

    let D = array2D [ [1.0f; 2.0f;]
                      [3.0f; 4.0f;] 
                      [5.0f; 6.0f;] ];
    let E = array2D [ [1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f; 7.0f; 8.0f];
                      [2.0f; 4.0f; 6.0f; 8.0f; 1.0f; 3.0f; 5.0f; 7.0f] ]

    let ATimes2 = array2D [ [2.0f; 4.0f; 6.0f]; [8.0f; 10.0f; 12.0f] ]
    let ATimes3 = array2D [ [3.0f; 6.0f; 9.0f]; [12.0f; 15.0f; 18.0f] ]

    let At = array2D [ [1.0f; 4.0f]; [2.0f; 5.0f]; [3.0f; 6.0f] ]
    let Bt = array2D [ [1.0f; 3.0f; 5.0f]; [2.0f; 4.0f; 6.0f] ]

    let Dt = array2D [ [1.0f; 3.0f; 5.0f];
                       [2.0f; 4.0f; 6.0f] ];
    let Et = array2D [ [1.0f; 2.0f];
                       [2.0f; 4.0f];
                       [3.0f; 6.0f];
                       [4.0f; 8.0f];
                       [5.0f; 1.0f];
                       [6.0f; 3.0f];
                       [7.0f; 5.0f];
                       [8.0f; 7.0f] ] 
    let DE = array2D [ [5.0f;  10.0f; 15.0f; 20.0f; 7.0f;  12.0f; 17.0f; 22.0f ];
                       [11.0f; 22.0f; 33.0f; 44.0f; 19.0f; 30.0f; 41.0f; 52.0f ];
                       [17.0f; 34.0f; 51.0f; 68.0f; 31.0f; 48.0f; 65.0f; 82.0f ] ]
    
    let M = array2D [ [2.0f; 0.0f]; [0.0f; 2.0f] ]
    let MtoN n = array2D [ [pown 2.0f n; 0.0f]; [0.0f; pown 2.0f n] ]

    let largeRandomMatrix = Array2D.init 50 100 (fun _ _ -> rand.NextDouble() |> float32)
    let largeRandomVector = Array.init 100 (fun _ -> rand.NextDouble() |> float32)
    let transposeOfLargeRandomMatrix = largeRandomMatrix |> transpose

    let UpperTriangle a b =
        array2D [ [a; b]; [0.0f; a] ]

    let UpperTriangleToN n a b =
        let aToN = pown a n
        array2D [ [aToN; (float32 n) * pown a (n - 1) * b]; [0.0f; aToN] ]

    let loadAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Matrix) (B:Matrix) ->

            let finalHeight = height A
            let finalWidth = width B

            let A = padToMultiplesOf blockSize A
            let B = padToMultiplesOf blockSize B

            let hA = height A
            let wA = width A
            let hB = height B
            let wB = width B
            let wC = wB
            let hC = height A

            let A = flattenMatrix A
            let B = flattenMatrix B

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let lp = createMultiplyLp blockSize hA wA hB wB
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            result |> rebuildMatrix wC finalHeight finalWidth

    let loadAndMultiplyByTranspose (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Matrix) (B:Matrix) ->

            let finalHeight = height A
            let finalWidth = height B

            let A = padToMultiplesOf blockSize A
            let B = padToMultiplesOf blockSize B

            let hA = height A
            let wA = width A
            let hB = height B
            let wB = width B
            let wC = hB
            let hC = height A

            let A = flattenMatrix A
            let B = flattenMatrix B

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let lp = createMultiplyByTransposeLp blockSize hA wA hB wB
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            result |> rebuildMatrix wC finalHeight finalWidth

    let loadTransposeAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Matrix) (B:Matrix) ->

            let finalHeight = width A
            let finalWidth = width B

            let A = padToMultiplesOf blockSize A
            let B = padToMultiplesOf blockSize B

            let hA = height A
            let wA = width A
            let hB = height B
            let wB = width B
            let wC = wB
            let hC = width A

            let A = flattenMatrix A
            let B = flattenMatrix B

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let lp = createTransposeAndMultiplyLp blockSize hA wA hB wB
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            result |> rebuildMatrix wC finalHeight finalWidth

    let loadAndMultiplyTemplate (blockSize:int) = cuda {
        let! kernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (B : Matrix) ->
                loadAndMultiply blockSize worker kernel A B
            ) }

    let loadAndMultiplyByTransposeTemplate (blockSize:int) = cuda {
        let! kernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (B : Matrix) ->
                loadAndMultiplyByTranspose blockSize worker kernel A B
            ) }

    let loadTransposeAndMultiplyTemplate (blockSize:int) = cuda {
        let! kernel = transposeAndMultiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (B : Matrix) ->
                loadTransposeAndMultiply blockSize worker kernel A B
            ) }

    let multiplyVectorByMatrixTemplate (blockSize:int) = cuda {
        let! kernel = multiplyVectorByMatrixKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (x : Vector) ->
                let size = height A

                let A = padToMultiplesOf blockSize A
                let x = padToMultipleOf blockSize x

                let hA = height A
                let wA = width A

                use A = A |> flattenMatrix |> worker.Malloc
                use x = x |> worker.Malloc
                use y = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByMatrixLp blockSize hA wA
                kernel.Launch lp y.Ptr A.Ptr x.Ptr hA wA

                y.Gather() |> subvector size
            ) }

    let multiplyVectorByMatrixAndTransformTwiceTemplate (transformation1 : TransformationFunction) (transformation2 : SecondOrderTransformationFunction) (blockSize:int) = cuda {
        let! kernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize transformation1 transformation2 |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (x : Vector) ->
                let size = height A

                let A = padToMultiplesOf blockSize A
                let x = padToMultipleOf blockSize x

                let hA = height A
                let wA = width A

                use A = A |> flattenMatrix |> worker.Malloc
                use x = x |> worker.Malloc
                use y1 = worker.Malloc<float32>(hA)
                use y2 = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByMatrixLp blockSize hA wA
                kernel.Launch lp y2.Ptr y1.Ptr A.Ptr x.Ptr hA wA

                (y1.Gather() |> subvector size, y2.Gather() |> subvector size)
            ) }

    let multiplyVectorByMatrixAndTransformTemplate (transformation : TransformationFunction) (blockSize:int) = cuda {
        let! kernel = multiplyVectorByMatrixAndTransformKernel blockSize transformation |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (x : Vector) ->
                let size = height A

                let A = padToMultiplesOf blockSize A
                let x = padToMultipleOf blockSize x

                let hA = height A
                let wA = width A

                use A = A |> flattenMatrix |> worker.Malloc
                use x = x |> worker.Malloc
                use y = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByMatrixLp blockSize hA wA
                kernel.Launch lp y.Ptr A.Ptr x.Ptr hA wA

                y.Gather() |> subvector size
            ) }
            
    let multiplyVectorByTransposeOfMatrixTemplate (blockSize:int) = cuda {
        let! kernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (x : Vector) ->
                let size = width A

                let A = padToMultiplesOf blockSize A
                let x = padToMultipleOf blockSize x

                let hA = height A
                let wA = width A

                use A = A |> flattenMatrix |> worker.Malloc
                use x = x |> worker.Malloc
                use y = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByTransposeOfMatrixLp blockSize hA wA
                kernel.Launch lp y.Ptr A.Ptr x.Ptr hA wA

                y.Gather() |> subvector size
            ) }

    // This template, which finds the n-th power of a square matrix,
    // shows how launch logic can be reused within the CUDA monad.
    // The same launch parameters are used in each iteration, and the
    // inputs of the launcher are addresses in the GPU memory.  This
    // means that there is no copying of data from the CPU to the GPU
    // throughout the loop.
    let powerOfNTemplate (blockSize : int) = cuda {
        let! kernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) n ->
                let originalSize = width A
                let A = padToMultiplesOf blockSize A
                let paddedSize = width A
                let A = flattenMatrix A
                let Ai = identityMatrix paddedSize |> flattenMatrix

                use A = worker.Malloc(A)
                use Ai = worker.Malloc(Ai)

                let threads = dim3(blockSize, blockSize)
                let grid = dim3(paddedSize / threads.x |> max 1, paddedSize / threads.y |> max 1)
                let lp = LaunchParam(grid, threads)

                for i = 1 to n do
                    kernel.Launch lp Ai.Ptr A.Ptr Ai.Ptr paddedSize paddedSize paddedSize paddedSize
                Ai.Gather() |> rebuildMatrix paddedSize originalSize originalSize
            ) }

    let addVectorTemplate (blockSize : int) = cuda {
        let! addVectorKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let addVectorKernel = program.Apply addVectorKernel

            fun (x : Vector) (y : Vector) ->
                Common.binaryVectorOperation blockSize x y addVectorKernel worker
        )
    }

    let subtractVectorTemplate (blockSize : int) = cuda {
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let subtractVectorKernel = program.Apply subtractVectorKernel

            fun (x : Vector) (y : Vector) ->
                Common.binaryVectorOperation blockSize x y subtractVectorKernel worker
        )
    }

    let pointwiseMultiplyVectorTemplate (blockSize : int) = cuda {
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel

            fun (x : Vector) (y : Vector) ->
                Common.binaryVectorOperation blockSize x y pointwiseMultiplyVectorKernel worker
        )
    }

    let outerProductTemplate (blockSize : int) = cuda {
        let! outerProductKernel = outerProductKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let outerProductKernel = program.Apply outerProductKernel

            fun (v : Vector) (w : Vector) ->
                let sizeV = Array.length v
                let sizeW = Array.length w
                let paddedV = padToMultipleOf blockSize v
                let paddedW = padToMultipleOf blockSize w
                let sizePaddedV = Array.length paddedV
                let sizePaddedW = Array.length paddedW

                use paddedV = worker.Malloc paddedV
                use paddedW = worker.Malloc paddedW
                use result = worker.Malloc<float32> (sizePaddedV * sizePaddedW)

                let lp = createSimpleMatrixOperationLp blockSize sizePaddedV sizePaddedW
                outerProductKernel.Launch lp result.Ptr paddedV.Ptr paddedW.Ptr sizePaddedW
                result.Gather() |> rebuildMatrix sizePaddedW sizeV sizeW
        )
    }

    let addMatrixTemplate (blockSize : int) = cuda {
        let! addMatrixKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let addMatrixKernel = program.Apply addMatrixKernel

            fun (A : Matrix) (B : Matrix) ->
                Common.binaryMatrixOperation blockSize A B addMatrixKernel worker
        )
    }

    let subtractMatrixTemplate (blockSize : int) = cuda {
        let! subtractMatrixKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let subtractMatrixKernel = program.Apply subtractMatrixKernel

            fun (A : Matrix) (B : Matrix) ->
                Common.binaryMatrixOperation blockSize A B subtractMatrixKernel worker
        )
    }

    let scalarMultiplyMatrixTemplate (blockSize : int) = cuda {
        let! scalarMultiplyMatrixKernel =  scalarMultiplyMatrixKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let scalarMultiplyMatrixKernel = program.Apply scalarMultiplyMatrixKernel

            fun (A : Matrix) (lambda : float32) ->
                let hA = height A
                let wA = width A
                let paddedA = padToMultiplesOf blockSize A
                let hPaddedA = height paddedA
                let wPaddedA = width paddedA
                let flattenedA = flattenMatrix paddedA

                use flattenedA = worker.Malloc flattenedA

                let lp = createSimpleMatrixOperationLp blockSize hPaddedA wPaddedA
                scalarMultiplyMatrixKernel.Launch lp flattenedA.Ptr lambda

                flattenedA.Gather() |> rebuildMatrix wPaddedA hA wA
        )
    }

    let loadAndMultiplyMatricesBlock1Program = 1 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let loadAndMultiplyMatricesBlock32Program = 32 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let loadAndMultiplyByTransposeProgram = 2 |> loadAndMultiplyByTransposeTemplate |> Compiler.load Worker.Default
    let loadTransposeAndMultiplyProgram = 2 |> loadTransposeAndMultiplyTemplate |> Compiler.load Worker.Default
    let powerProgram = 32 |> powerOfNTemplate |> Compiler.load Worker.Default
    let addMatrixProgram = 2 |> addMatrixTemplate |> Compiler.load Worker.Default
    let subtractMatrixProgram = 2 |> subtractMatrixTemplate |> Compiler.load Worker.Default
    let addVectorProgram = 2 |> addVectorTemplate |> Compiler.load Worker.Default
    let subtractVectorProgram = 2 |> subtractVectorTemplate |> Compiler.load Worker.Default
    let pointwiseMultiplyVectorProgram = 2 |> pointwiseMultiplyVectorTemplate |> Compiler.load Worker.Default
    let scalarMultiplyMatrixProgram = 2 |> scalarMultiplyMatrixTemplate |> Compiler.load Worker.Default
    let multiplyVectorByMatrixBlock1Program = 1 |> multiplyVectorByMatrixTemplate |> Compiler.load Worker.Default
    let multiplyVectorByMatrixBlock32Program = 32 |> multiplyVectorByMatrixTemplate |> Compiler.load Worker.Default
    let multiplyVectorByMatrixAndTransformBlock1Program = 1 |> multiplyVectorByMatrixAndTransformTemplate <@ sigmoid @> |> Compiler.load Worker.Default
    let multiplyVectorByMatrixAndTransformBlock32Program = 32 |> multiplyVectorByMatrixAndTransformTemplate <@ sigmoid @> |> Compiler.load Worker.Default
    let multiplyVectorByMatrixAndTransformTwiceBlock1Program = 1 |> multiplyVectorByMatrixAndTransformTwiceTemplate <@ sigmoid @> <@ dSigmoid2 @> |> Compiler.load Worker.Default
    let multiplyVectorByMatrixAndTransformTwiceBlock32Program = 32 |> multiplyVectorByMatrixAndTransformTwiceTemplate <@ sigmoid @> <@ dSigmoid2 @> |> Compiler.load Worker.Default
    let multiplyVectorByTransposeOfMatrixBlock1Program = 1 |> multiplyVectorByTransposeOfMatrixTemplate |> Compiler.load Worker.Default
    let multiplyVectorByTransposeOfMatrixBlock32Program = 32 |> multiplyVectorByTransposeOfMatrixTemplate |> Compiler.load Worker.Default
    let outerProductBlock1Program = 1 |> outerProductTemplate |> Compiler.load Worker.Default
    let outerProductBlock32Program = 32 |> outerProductTemplate |> Compiler.load Worker.Default

    [<Fact>] member test.
        ``The outer product of a and c is computed with a block size of 1.``() =
            outerProductBlock1Program.Run a c |> should equal aOuterProductc

    [<Fact>] member test.
        ``The outer product of c and a is computed with a block size of 1.``() =
            outerProductBlock1Program.Run c a |> should equal cOuterProducta

    [<Fact>] member test.
        ``The outer product of a and c is computed with a block size of 32.``() =
            outerProductBlock32Program.Run a c |> should equal aOuterProductc

    [<Fact>] member test.
        ``The outer product of c and a is computed with a block size of 32.``() =
            outerProductBlock32Program.Run c a |> should equal cOuterProducta

    [<Fact>] member test.
        ``The addVectorTemplate adds a to b.``() =
            addVectorProgram.Run a b |> should equal aPlusb

    [<Fact>] member test.
        ``The subtractVectorTemplate subtracts a from b.``() =
            subtractVectorProgram.Run b a |> should equal bMinusa

    [<Fact>] member test.
        ``The pointwiseMultiplyVectorTemplate multiplies a by b.``() =
            pointwiseMultiplyVectorProgram.Run a b |> should equal aPointwiseTimesb

    [<Fact>] member test.
        ``The multiplyVectorByMatrixTemplate multiplies A by x with a block size of 1.``() =
            multiplyVectorByMatrixBlock1Program.Run A x |> should equal y

    [<Fact>] member test.
        ``The multiplyVectorByMatrixTemplate multiplies A by x with a block size of 32.``() =
            multiplyVectorByMatrixBlock32Program.Run A x |> should equal y

    [<Fact>] member test.
        ``The multiplyVectorByTransposeOfMatrixTemplate multiplies the transpose of At by x with a block size of 1.``() =
            multiplyVectorByTransposeOfMatrixBlock1Program.Run At x |> should equal y

    [<Fact>] member test.
        ``The multiplyVectorByTransposeOfMatrixTemplate multiplies the transpose of At by x with a block size of 32.``() =
            multiplyVectorByTransposeOfMatrixBlock32Program.Run At x |> should equal y

    [<Fact>] member test.
        ``The loadAndMultiplyTemplate multiplies A by B with a block size of 1.``() =
            loadAndMultiplyMatricesBlock1Program.Run A B |> should equal C

    [<Fact>] member test.
        ``The loadAndMultiplyTemplate multiplies D by E with a block size of 1.``() =
            loadAndMultiplyMatricesBlock1Program.Run D E |> should equal DE

    [<Fact>] member test.
        ``The loadAndMultiplyTemplate multiplies A by B with a block size of 32.``() =
            loadAndMultiplyMatricesBlock32Program.Run A B |> should equal C

    [<Fact>] member test.
        ``The loadAndMultiplyTemplate multiplies D by E with a block size of 32.``() =
            loadAndMultiplyMatricesBlock32Program.Run D E |> should equal DE

    [<Fact>] member test.
        ``The powerOfNTemplate raises M to the power of 10.``() =
            powerProgram.Run M 10 |> should equal (MtoN 10)

    [<Fact>] member test.
        ``The powerOfNTemplate raises an Upper Triangular matrix to the power of 10.``() =
            UpperTriangle 2.0f 3.0f |> fun m -> powerProgram.Run m 10 |> should equal (UpperTriangleToN 10 2.0f 3.0f)

    [<Fact>] member test.
        ``The loadAndMultiplyByTransposeTemplate multiplies A by the transpose of (B Transpose) to give AB.``() =
            loadAndMultiplyByTransposeProgram.Run A Bt |> should equal C

    [<Fact>] member test.
        ``The loadAndMultiplyByTransposeTemplate multiplies D by the transpose of (E Transpose) to give DE.``() =
            loadAndMultiplyByTransposeProgram.Run D Et |> should equal DE

    [<Fact>] member test.
        ``The loadTransposeAndMultiplyTemplate multiplies the transpose of (A Transpose) by B to give AB.``() =
            loadTransposeAndMultiplyProgram.Run At B |> should equal C

    [<Fact>] member test.
        ``The loadTransposeAndMultiplyTemplate multiplies the transpose of (D Transpose) by E to give DE.``() =
            loadTransposeAndMultiplyProgram.Run Dt E |> should equal DE

    [<Fact>] member test.
        ``The addMatrixTemplate adds A to 2A to give 3A.``() =
            addMatrixProgram.Run A ATimes2 |> should equal ATimes3

    [<Fact>] member test.
        ``The subtractMatrixTemplate subtracts A from 3A to give 2A.``() =
            subtractMatrixProgram.Run ATimes3 A |> should equal ATimes2

    [<Fact>] member test.
        ``The scalarMultiplyTemplate multiplies A by 3 to give 3A.``() =
            scalarMultiplyMatrixProgram.Run A 3.0f |> should equal ATimes3

    [<Fact>] member test.
        ``Multipliying the large random matrix by the large random vector gives matching results for the CPU and the GPU.``()=
            multiplyVectorByMatrixBlock32Program.Run largeRandomMatrix largeRandomVector 
            |> arraysMatch (multiplyVectorByMatrix largeRandomMatrix largeRandomVector)
            |> should equal true

    [<Fact>] member test.
        ``Multipliying the transpose of the transposed large random matrix by the large random vector gives matching results for the CPU and the GPU.``()=
            multiplyVectorByTransposeOfMatrixBlock32Program.Run transposeOfLargeRandomMatrix largeRandomVector 
            |> arraysMatch (multiplyVectorByMatrix largeRandomMatrix largeRandomVector)
            |> should equal true

    [<Fact>] member test.
        ``Multipliying the large random matrix by the large random vector and transforming gives matching results for the CPU and the GPU (Block Size 1).``()=
            multiplyVectorByMatrixAndTransformBlock1Program.Run largeRandomMatrix largeRandomVector 
            |> arraysMatch ((multiplyVectorByMatrix largeRandomMatrix largeRandomVector) |> Array.map sigmoid)
            |> should equal true

    [<Fact>] member test.
        ``Multipliying the large random matrix by the large random vector and transforming gives matching results for the CPU and the GPU (Block Size 32).``()=
            multiplyVectorByMatrixAndTransformBlock32Program.Run largeRandomMatrix largeRandomVector 
            |> arraysMatch ((multiplyVectorByMatrix largeRandomMatrix largeRandomVector) |> Array.map sigmoid)
            |> should equal true

    [<Fact>] member test.
        ``Multipliying the large random matrix by the large random vector and transforming twice gives matching results for the CPU and the GPU (Block Size 1).``()=
            multiplyVectorByMatrixAndTransformTwiceBlock1Program.Run largeRandomMatrix largeRandomVector 
            |> fun result -> (fst result |> arraysMatch ((multiplyVectorByMatrix largeRandomMatrix largeRandomVector) |> Array.map sigmoid), snd result |> arraysMatch ((multiplyVectorByMatrix largeRandomMatrix largeRandomVector) |> Array.map dSigmoid1))
            |> should equal (true, true)

    [<Fact>] member test.
        ``Multipliying the large random matrix by the large random vector and transforming twice gives matching results for the CPU and the GPU (Block Size 32).``()=
            multiplyVectorByMatrixAndTransformTwiceBlock32Program.Run largeRandomMatrix largeRandomVector 
            |> fun result -> (fst result |> arraysMatch ((multiplyVectorByMatrix largeRandomMatrix largeRandomVector) |> Array.map sigmoid), snd result |> arraysMatch ((multiplyVectorByMatrix largeRandomMatrix largeRandomVector) |> Array.map dSigmoid1))
            |> should equal (true, true)
