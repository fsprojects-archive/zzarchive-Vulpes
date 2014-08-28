namespace DeepBelief.Tests

open System
open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open Xunit.Extensions
open FsUnit.Xunit
open Common.Analytics
open Common.CudaTemplates
open Common.Kernels
open DeepBelief.CudaDeepBeliefNet
open DeepBelief.CudaTemplates
open DeepBelief.Kernels
open DeepBelief.Utils
open DeepBelief.Tests.CudaCommon
open TestUtils

type ``CUDA Matrix Multiplication``()=

    let rand = new Random()
    let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ] |> Matrix
    let B = array2D [ [1.0f; 2.0f]; [3.0f; 4.0f]; [5.0f; 6.0f] ] |> Matrix
    let C = array2D [ [22.0f; 28.0f]; [49.0f; 64.0f] ] |> Matrix
    let x = [|7.0f; 8.0f; 9.0f|] |> Vector
    let y = [|50.0f; 122.0f|] |> Vector
    
    let a = [|1.0f; 2.0f; 3.0f|] |> Vector
    let b = [|4.0f; 5.0f; 6.0f|] |> Vector
    let c = [|4.0f; 5.0f|] |> Vector

    let aPlusb = [|5.0f; 7.0f; 9.0f|] |> Vector
    let bMinusa = [|3.0f; 3.0f; 3.0f|] |> Vector
    let aPointwiseTimesb = [|4.0f; 10.0f; 18.0f|] |> Vector

    let aOuterProductc = array2D [ [4.0f; 5.0f]
                                   [8.0f; 10.0f]
                                   [12.0f; 15.0f] ] |> Matrix
    let cOuterProducta = array2D [ [4.0f; 8.0f; 12.0f]
                                   [5.0f; 10.0f; 15.0f] ] |> Matrix

    let D = array2D [ [1.0f; 2.0f;]
                      [3.0f; 4.0f;] 
                      [5.0f; 6.0f;] ] |> Matrix
    let E = array2D [ [1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f; 7.0f; 8.0f];
                      [2.0f; 4.0f; 6.0f; 8.0f; 1.0f; 3.0f; 5.0f; 7.0f] ] |> Matrix

    let ATimes2 = array2D [ [2.0f; 4.0f; 6.0f]; [8.0f; 10.0f; 12.0f] ] |> Matrix
    let ATimes3 = array2D [ [3.0f; 6.0f; 9.0f]; [12.0f; 15.0f; 18.0f] ] |> Matrix

    let At = array2D [ [1.0f; 4.0f]; [2.0f; 5.0f]; [3.0f; 6.0f] ] |> Matrix
    let Bt = array2D [ [1.0f; 3.0f; 5.0f]; [2.0f; 4.0f; 6.0f] ] |> Matrix

    let Dt = array2D [ [1.0f; 3.0f; 5.0f];
                       [2.0f; 4.0f; 6.0f] ] |> Matrix
    let Et = array2D [ [1.0f; 2.0f];
                       [2.0f; 4.0f];
                       [3.0f; 6.0f];
                       [4.0f; 8.0f];
                       [5.0f; 1.0f];
                       [6.0f; 3.0f];
                       [7.0f; 5.0f];
                       [8.0f; 7.0f] ] |> Matrix
    let DE = array2D [ [5.0f;  10.0f; 15.0f; 20.0f; 7.0f;  12.0f; 17.0f; 22.0f ];
                       [11.0f; 22.0f; 33.0f; 44.0f; 19.0f; 30.0f; 41.0f; 52.0f ];
                       [17.0f; 34.0f; 51.0f; 68.0f; 31.0f; 48.0f; 65.0f; 82.0f ] ] |> Matrix
    
    let M = array2D [ [2.0f; 0.0f]; [0.0f; 2.0f] ] |> Matrix
    let MtoN n = array2D [ [pown 2.0f n; 0.0f]; [0.0f; pown 2.0f n] ] |> Matrix

    let largeRandomMatrix (rand : Random) = Array2D.init 100 150 (fun _ _ -> rand.NextDouble() |> float32) |> Matrix
    let largeRandomVector (rand : Random) = Array.init 150 (fun _ -> rand.NextDouble() |> float32) |> Vector
    let lrm = largeRandomMatrix rand
    let lrv = largeRandomVector rand
    let transposeOfLargeRandomMatrix = lrm.Transpose

    let UpperTriangle a b =
        array2D [ [a; b]; [0.0f; a] ] |> Matrix

    let UpperTriangleToN n a b =
        let aToN = pown a n
        array2D [ [aToN; (float32 n) * pown a (n - 1) * b]; [0.0f; aToN] ] |> Matrix

    let loadAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Matrix) (B:Matrix) ->

            let finalHeight = A.Height
            let finalWidth = B.Width

            let A = A.PadToMultiplesOf blockSize
            let B = B.PadToMultiplesOf blockSize

            let hA = A.Height
            let wA = A.Width
            let hB = B.Height
            let wB = B.Width
            let wC = wB
            let hC = A.Height

            let A = A.ToRowMajorFormat
            let B = B.ToRowMajorFormat

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let lp = createMultiplyLp blockSize hA wA hB wB
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            (result |> Matrix.FromRowMajorFormat wC).Submatrix 0 0 finalHeight finalWidth

    let loadAndMultiplyByTranspose (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Matrix) (B:Matrix) ->

            let finalHeight = A.Height
            let finalWidth = B.Height

            let A = A.PadToMultiplesOf blockSize
            let B = B.PadToMultiplesOf blockSize

            let hA = A.Height
            let wA = A.Width
            let hB = B.Height
            let wB = B.Width
            let wC = hB
            let hC = A.Height

            let A = A.ToRowMajorFormat
            let B = B.ToRowMajorFormat

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let lp = createMultiplyByTransposeLp blockSize hA wA hB wB
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            (result |> Matrix.FromRowMajorFormat wC).Submatrix 0 0 finalHeight finalWidth

    let loadTransposeAndMultiply (blockSize:int) (worker:Worker) (kernel:Kernel<MatrixMulKernelSignature>) =
        fun (A:Matrix) (B:Matrix) ->

            let finalHeight = A.Width
            let finalWidth = B.Width

            let A = A.PadToMultiplesOf blockSize
            let B = B.PadToMultiplesOf blockSize

            let hA = A.Height
            let wA = A.Width
            let hB = B.Height
            let wB = B.Width
            let wC = wB
            let hC = A.Width

            let A = A.ToRowMajorFormat
            let B = B.ToRowMajorFormat

            use A = worker.Malloc(A)
            use B = worker.Malloc(B)
            use C = worker.Malloc<float32>(wC * hC)

            let lp = createTransposeAndMultiplyLp blockSize hA wA hB wB
            kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
            let result = C.Gather()
            (result |> Matrix.FromRowMajorFormat wC).Submatrix 0 0 finalHeight finalWidth

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
                let size = A.Height

                let A = A.PadToMultiplesOf blockSize
                let x = x.PadToMultipleOf blockSize

                let hA = A.Height
                let wA = A.Width

                use A = A.ToRowMajorFormat |> worker.Malloc
                use x = x |> worker.Malloc
                use y = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByMatrixLp blockSize hA wA
                kernel.Launch lp y.Ptr A.Ptr x.Ptr hA wA

                y.Gather() |> subvector size |> Vector
            ) }

    let multiplyVectorByMatrixAndTransformTwiceTemplate (transformation1 : TransformationFunction) (transformation2 : SecondOrderTransformationFunction) (blockSize:int) = cuda {
        let! kernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize transformation1 transformation2 |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (x : Vector) ->
                let size = A.Height

                let A = A.PadToMultiplesOf blockSize
                let x = x.PadToMultipleOf blockSize

                let hA = A.Height
                let wA = A.Width

                use A = A.ToRowMajorFormat |> worker.Malloc
                use x = x |> worker.Malloc
                use y1 = worker.Malloc<float32>(hA)
                use y2 = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByMatrixLp blockSize hA wA
                kernel.Launch lp y2.Ptr y1.Ptr A.Ptr x.Ptr hA wA

                (y1.Gather() |> subvector size |> Vector, y2.Gather() |> subvector size |> Vector)
            ) }

    let multiplyVectorByMatrixAndTransformTemplate (transformation : TransformationFunction) (blockSize:int) = cuda {
        let! kernel = multiplyVectorByMatrixAndTransformKernel blockSize transformation |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (x : Vector) ->
                let size = A.Height

                let A = A.PadToMultiplesOf blockSize
                let x = x.PadToMultipleOf blockSize

                let hA = A.Height
                let wA = A.Width

                use A = A.ToRowMajorFormat |> worker.Malloc
                use x = x |> worker.Malloc
                use y = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByMatrixLp blockSize hA wA
                kernel.Launch lp y.Ptr A.Ptr x.Ptr hA wA

                y.Gather() |> subvector size |> Vector
            ) }
            
    let multiplyVectorByTransposeOfMatrixTemplate (blockSize:int) = cuda {
        let! kernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Matrix) (x : Vector) ->
                let size = A.Width

                let A = A.PadToMultiplesOf blockSize
                let x = x.PadToMultipleOf blockSize

                let hA = A.Height
                let wA = A.Width

                use A = A.ToRowMajorFormat |> worker.Malloc
                use x = x |> worker.Malloc
                use y = worker.Malloc<float32>(hA)

                let lp = createMultiplyVectorByTransposeOfMatrixLp blockSize hA wA
                kernel.Launch lp y.Ptr A.Ptr x.Ptr hA wA

                y.Gather() |> subvector size |> Vector
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
                let originalSize = A.Width
                let A = A.PadToMultiplesOf blockSize
                let paddedSize = A.Width
                let A = A.ToRowMajorFormat
                let Ai = ((identityMatrix paddedSize) |> Matrix).ToRowMajorFormat

                use A = worker.Malloc(A)
                use Ai = worker.Malloc(Ai)

                let threads = dim3(blockSize, blockSize)
                let grid = dim3(paddedSize / threads.x |> max 1, paddedSize / threads.y |> max 1)
                let lp = LaunchParam(grid, threads)

                for i = 1 to n do
                    kernel.Launch lp Ai.Ptr A.Ptr Ai.Ptr paddedSize paddedSize paddedSize paddedSize
                (Ai.Gather() |> Matrix.FromRowMajorFormat paddedSize).Submatrix 0 0 originalSize originalSize
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
                let sizeV = v.Length
                let sizeW = w.Length
                let paddedV = v.PadToMultipleOf blockSize
                let paddedW = w.PadToMultipleOf blockSize
                let sizePaddedV = Array.length paddedV
                let sizePaddedW = Array.length paddedW

                use paddedV = worker.Malloc paddedV
                use paddedW = worker.Malloc paddedW
                use result = worker.Malloc<float32> (sizePaddedV * sizePaddedW)

                let lp = createOuterProductLp blockSize sizePaddedV sizePaddedW
                outerProductKernel.Launch lp result.Ptr paddedV.Ptr paddedW.Ptr sizePaddedW
                (result.Gather() |> Matrix.FromRowMajorFormat sizePaddedW).Submatrix 0 0 sizeV sizeW
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
                let hA = A.Height
                let wA = A.Width
                let paddedA = A.PadToMultiplesOf blockSize
                let hPaddedA = paddedA.Height
                let wPaddedA = paddedA.Width
                let flattenedA = paddedA.ToRowMajorFormat
                use flattenedA = worker.Malloc flattenedA

                let lp = createSimpleMatrixOperationLp blockSize hPaddedA wPaddedA
                scalarMultiplyMatrixKernel.Launch lp flattenedA.Ptr lambda

                (flattenedA.Gather() |> Matrix.FromRowMajorFormat wPaddedA).Submatrix 0 0 hA wA
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
        use loadAndMultiplyMatricesProgram = i |> loadAndMultiplyTemplate |> Compiler.load Worker.Default in
        loadAndMultiplyMatricesProgram.Run A B |> should equal C

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
        use multiplyVectorByMatrixProgram = i |> multiplyVectorByMatrixTemplate |> Compiler.load Worker.Default in
        multiplyVectorByMatrixProgram.Run lrm lrv
        |> vectorsMatch (lrm * lrv)
        |> should equal true

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``Multiplying the transpose of the transposed large random matrix by the large random vector gives matching results for the CPU and the GPU.``(i)=
        use multiplyVectorByTransposeOfMatrixProgram = i |> multiplyVectorByTransposeOfMatrixTemplate |> Compiler.load Worker.Default in
        multiplyVectorByTransposeOfMatrixProgram.Run transposeOfLargeRandomMatrix lrv
        |> vectorsMatch (lrm * lrv)
        |> should equal true

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``Multiplying the large random matrix by the large random vector and transforming gives matching results for the CPU and the GPU.``(i)=
        use multiplyVectorByMatrixAndTransformBlock1Program = i |> multiplyVectorByMatrixAndTransformTemplate <@ sigmoid @> |> Compiler.load Worker.Default in
        multiplyVectorByMatrixAndTransformBlock1Program.Run lrm lrv
        |> vectorsMatch ((lrm * lrv).Map sigmoid)
        |> should equal true

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``Multiplying the large random matrix by the large random vector and transforming twice gives matching results for the CPU and the GPU.``(i)=
        use multiplyVectorByMatrixAndTransformTwiceProgram = i |> multiplyVectorByMatrixAndTransformTwiceTemplate <@ sigmoid @> <@ dSigmoid @> |> Compiler.load Worker.Default in
        multiplyVectorByMatrixAndTransformTwiceProgram.Run lrm lrv 
        |> fun result -> (fst result |> vectorsMatch ((lrm * lrv).Map sigmoid), snd result |> vectorsMatch ((lrm * lrv).Map (fun x -> dSigmoid (sigmoid x) 0.0f)))
        |> should equal (true, true)
