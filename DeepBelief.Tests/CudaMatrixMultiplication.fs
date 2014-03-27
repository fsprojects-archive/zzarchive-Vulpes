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

open System
open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open Xunit.Extensions
open FsUnit.Xunit
open Common
open DeepBelief.CudaTemplates
open DeepBelief.Kernels
open DeepBelief.Utils
open TestUtils

type ``CUDA Matrix Multiplication``()=

    let rand = new Random()
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

    let largeRandomMatrix (rand : Random) = Array2D.init 50 100 (fun _ _ -> rand.NextDouble() |> float32)
    let largeRandomVector (rand : Random) = Array.init 100 (fun _ -> rand.NextDouble() |> float32)
    let lrm = largeRandomMatrix rand
    let lrv = largeRandomVector rand
    let transposeOfLargeRandomMatrix = lrm |> transpose

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
                CudaCommon.binaryVectorOperation blockSize x y addVectorKernel worker
        )
    }

    let subtractVectorTemplate (blockSize : int) = cuda {
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let subtractVectorKernel = program.Apply subtractVectorKernel

            fun (x : Vector) (y : Vector) ->
                CudaCommon.binaryVectorOperation blockSize x y subtractVectorKernel worker
        )
    }

    let pointwiseMultiplyVectorTemplate (blockSize : int) = cuda {
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel

            fun (x : Vector) (y : Vector) ->
                CudaCommon.binaryVectorOperation blockSize x y pointwiseMultiplyVectorKernel worker
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
                CudaCommon.binaryMatrixOperation blockSize A B addMatrixKernel worker
        )
    }

    let subtractMatrixTemplate (blockSize : int) = cuda {
        let! subtractMatrixKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let subtractMatrixKernel = program.Apply subtractMatrixKernel

            fun (A : Matrix) (B : Matrix) ->
                CudaCommon.binaryMatrixOperation blockSize A B subtractMatrixKernel worker
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

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The outer product of c and a is computed.``(i) =
        use outerProductProgram = i |> outerProductTemplate |> Compiler.load Worker.Default in
        outerProductProgram.Run c a |> should equal cOuterProducta

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The outer product of a and c is computed.``(i) =
        use outerProductProgram = i |> outerProductTemplate |> Compiler.load Worker.Default in
        outerProductProgram.Run a c |> should equal aOuterProductc

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The addVectorTemplate adds a to b.``(i) =
        use addVectorProgram = i |> addVectorTemplate |> Compiler.load Worker.Default in
        addVectorProgram.Run a b |> should equal aPlusb

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The subtractVectorTemplate subtracts a from b.``(i) =
        use subtractVectorProgram = i |> subtractVectorTemplate |> Compiler.load Worker.Default in
        subtractVectorProgram.Run b a |> should equal bMinusa

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The pointwiseMultiplyVectorTemplate multiplies a by b.``(i) =
        use pointwiseMultiplyVectorProgram = i |> pointwiseMultiplyVectorTemplate |> Compiler.load Worker.Default in
        pointwiseMultiplyVectorProgram.Run a b |> should equal aPointwiseTimesb

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The multiplyVectorByMatrixTemplate multiplies A by x.``(i) =
        use multiplyVectorByMatrixBlock1Program = i |> multiplyVectorByMatrixTemplate |> Compiler.load Worker.Default in
        multiplyVectorByMatrixBlock1Program.Run A x |> should equal y

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The multiplyVectorByTransposeOfMatrixTemplate multiplies the transpose of At by x.``(i) =
        use multiplyVectorByTransposeOfMatrixBlock1Program = i |> multiplyVectorByTransposeOfMatrixTemplate |> Compiler.load Worker.Default in
        multiplyVectorByTransposeOfMatrixBlock1Program.Run At x |> should equal y

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The loadAndMultiplyTemplate multiplies A by B.``(i) =
        use loadAndMultiplyMatricesBlock1Program = i |> loadAndMultiplyTemplate |> Compiler.load Worker.Default in
        loadAndMultiplyMatricesBlock1Program.Run A B |> should equal C

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The loadAndMultiplyTemplate multiplies D by E.``(i) =
        use loadAndMultiplyMatricesBlock1Program = i |> loadAndMultiplyTemplate |> Compiler.load Worker.Default in
        loadAndMultiplyMatricesBlock1Program.Run D E |> should equal DE

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The powerOfNTemplate raises M to the power of 10.``(i) =
        use powerProgram = i |> powerOfNTemplate |> Compiler.load Worker.Default in
        powerProgram.Run M 10 |> should equal (MtoN 10)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The powerOfNTemplate raises an Upper Triangular matrix to the power of 10.``(i) =
        use powerProgram = i |> powerOfNTemplate |> Compiler.load Worker.Default in
        UpperTriangle 2.0f 3.0f |> fun m -> powerProgram.Run m 10 |> should equal (UpperTriangleToN 10 2.0f 3.0f)

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The loadAndMultiplyByTransposeTemplate multiplies A by the transpose of (B Transpose) to give AB.``(i) =
        use loadAndMultiplyByTransposeProgram = i |> loadAndMultiplyByTransposeTemplate |> Compiler.load Worker.Default in
        loadAndMultiplyByTransposeProgram.Run A Bt |> should equal C

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The loadAndMultiplyByTransposeTemplate multiplies D by the transpose of (E Transpose) to give DE.``(i) =
        use loadAndMultiplyByTransposeProgram = i |> loadAndMultiplyByTransposeTemplate |> Compiler.load Worker.Default in
        loadAndMultiplyByTransposeProgram.Run D Et |> should equal DE

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The loadTransposeAndMultiplyTemplate multiplies the transpose of (A Transpose) by B to give AB.``(i) =
        use loadTransposeAndMultiplyProgram = i |> loadTransposeAndMultiplyTemplate |> Compiler.load Worker.Default in
        loadTransposeAndMultiplyProgram.Run At B |> should equal C

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The loadTransposeAndMultiplyTemplate multiplies the transpose of (D Transpose) by E to give DE.``(i) =
        use loadTransposeAndMultiplyProgram = i |> loadTransposeAndMultiplyTemplate |> Compiler.load Worker.Default in
        loadTransposeAndMultiplyProgram.Run Dt E |> should equal DE

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The addMatrixTemplate adds A to 2A to give 3A.``(i) =
        use addMatrixProgram = i |> addMatrixTemplate |> Compiler.load Worker.Default in
        addMatrixProgram.Run A ATimes2 |> should equal ATimes3

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The subtractMatrixTemplate subtracts A from 3A to give 2A.``(i) =
        use subtractMatrixProgram = i |> subtractMatrixTemplate |> Compiler.load Worker.Default in
        subtractMatrixProgram.Run ATimes3 A |> should equal ATimes2

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The scalarMultiplyTemplate multiplies A by 3 to give 3A.``(i) =
        use scalarMultiplyMatrixProgram = i |> scalarMultiplyMatrixTemplate |> Compiler.load Worker.Default in
        scalarMultiplyMatrixProgram.Run A 3.0f |> should equal ATimes3

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``Multiplying the large random matrix by the large random vector gives matching results for the CPU and the GPU.``(i)=
        use multiplyVectorByMatrixBlock32Program = i |> multiplyVectorByMatrixTemplate |> Compiler.load Worker.Default in
        multiplyVectorByMatrixBlock32Program.Run lrm lrv
        |> arraysMatch (multiplyVectorByMatrix lrm lrv)
        |> should equal true

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``Multiplying the transpose of the transposed large random matrix by the large random vector gives matching results for the CPU and the GPU.``(i)=
        use multiplyVectorByTransposeOfMatrixBlock32Program = i |> multiplyVectorByTransposeOfMatrixTemplate |> Compiler.load Worker.Default in
        multiplyVectorByTransposeOfMatrixBlock32Program.Run transposeOfLargeRandomMatrix lrv
        |> arraysMatch (multiplyVectorByMatrix lrm lrv)
        |> should equal true

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``Multiplying the large random matrix by the large random vector and transforming gives matching results for the CPU and the GPU.``(i)=
        use multiplyVectorByMatrixAndTransformBlock1Program = i |> multiplyVectorByMatrixAndTransformTemplate <@ sigmoid @> |> Compiler.load Worker.Default in
        multiplyVectorByMatrixAndTransformBlock1Program.Run lrm lrv
        |> arraysMatch ((multiplyVectorByMatrix lrm lrv) |> Array.map sigmoid)
        |> should equal true

    [<Fact>] member test.
        ``Multiplying the large random matrix by the large random vector and transforming twice gives matching results for the CPU and the GPU (Block Size 1).``()=
            use multiplyVectorByMatrixAndTransformTwiceBlock1Program = 1 |> multiplyVectorByMatrixAndTransformTwiceTemplate <@ sigmoid @> <@ dSigmoid @> |> Compiler.load Worker.Default in
            multiplyVectorByMatrixAndTransformTwiceBlock1Program.Run lrm lrv 
            |> fun result -> (fst result |> arraysMatch ((multiplyVectorByMatrix lrm lrv) |> Array.map sigmoid), snd result |> arraysMatch ((multiplyVectorByMatrix lrm lrv) |> Array.map (fun x -> dSigmoid (sigmoid x) 0.0f)))
            |> should equal (true, true)

    [<Fact>] member test.
        ``Multiplying the large random matrix by the large random vector and transforming twice gives matching results for the CPU and the GPU (Block Size 32).``()=
            use multiplyVectorByMatrixAndTransformTwiceBlock32Program = 32 |> multiplyVectorByMatrixAndTransformTwiceTemplate <@ sigmoid @> <@ dSigmoid @> |> Compiler.load Worker.Default in
            multiplyVectorByMatrixAndTransformTwiceBlock32Program.Run lrm lrv 
            |> fun result -> (fst result |> arraysMatch ((multiplyVectorByMatrix lrm lrv) |> Array.map sigmoid), snd result |> arraysMatch ((multiplyVectorByMatrix lrm lrv) |> Array.map (fun x -> dSigmoid (sigmoid x) 0.0f)))
            |> should equal (true, true)
