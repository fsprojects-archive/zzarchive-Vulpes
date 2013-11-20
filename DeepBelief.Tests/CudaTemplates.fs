namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief.DeepBeliefNet
open DeepBelief.CudaTemplates
open DeepBelief.Kernels
open DeepBelief.Utils

module Common =
    type SimpleMatrixOperationKernelSignature = deviceptr<float32> -> deviceptr<float32> -> int -> int -> unit
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
        kernel.Launch lp flattenedA.Ptr flattenedB.Ptr hPaddedA wPaddedA

        flattenedA.Gather() |> rebuildMatrix wPaddedA |> topLeftSubmatrix hA wA

type ``CUDA Matrix Multiplication``()=

    let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ]
    let B = array2D [ [1.0f; 2.0f]; [3.0f; 4.0f]; [5.0f; 6.0f] ]
    let C = array2D [ [22.0f; 28.0f]; [49.0f; 64.0f] ]
    
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
            rebuildMatrix wC result |> topLeftSubmatrix finalHeight finalWidth

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
            rebuildMatrix wC result |> topLeftSubmatrix finalHeight finalWidth

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
            rebuildMatrix wC result |> topLeftSubmatrix finalHeight finalWidth

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
                Ai.Gather() |> rebuildMatrix paddedSize |> topLeftSubmatrix originalSize originalSize
            ) }

    let addTemplate (blockSize : int) = cuda {
        let! addKernel =  addKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let addKernel = program.Apply addKernel

            fun (A : Matrix) (B : Matrix) ->
                Common.simpleMatrixOperation blockSize A B addKernel worker
        )
    }

    let subtractTemplate (blockSize : int) = cuda {
        let! subtractKernel =  subtractKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let subtractKernel = program.Apply subtractKernel

            fun (A : Matrix) (B : Matrix) ->
                Common.simpleMatrixOperation blockSize A B subtractKernel worker
        )
    }

    let scalarMultiplyTemplate (blockSize : int) = cuda {
        let! scalarMultiplyKernel =  scalarMultiplyKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let scalarMultiplyKernel = program.Apply scalarMultiplyKernel

            fun (A : Matrix) (lambda : float32) ->
                let hA = height A
                let wA = width A
                let paddedA = padToMultiplesOf blockSize A
                let hPaddedA = height paddedA
                let wPaddedA = width paddedA
                let flattenedA = flattenMatrix paddedA

                use flattenedA = worker.Malloc flattenedA

                let lp = createSimpleMatrixOperationLp blockSize hPaddedA wPaddedA
                scalarMultiplyKernel.Launch lp flattenedA.Ptr lambda hPaddedA wPaddedA

                flattenedA.Gather() |> rebuildMatrix wPaddedA |> topLeftSubmatrix hA wA
        )
    }

    let loadAndMultiplyMatricesBlock1Program = 1 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let loadAndMultiplyMatricesBlock32Program = 32 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let loadAndMultiplyByTransposeProgram = 2 |> loadAndMultiplyByTransposeTemplate |> Compiler.load Worker.Default
    let loadTransposeAndMultiplyProgram = 2 |> loadTransposeAndMultiplyTemplate |> Compiler.load Worker.Default
    let powerProgram = 32 |> powerOfNTemplate |> Compiler.load Worker.Default
    let addProgram = 2 |> addTemplate |> Compiler.load Worker.Default
    let subtractProgram = 2 |> subtractTemplate |> Compiler.load Worker.Default
    let scalarMultiplyProgram = 2 |> scalarMultiplyTemplate |> Compiler.load Worker.Default

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
        ``The addTemplate adds A to 2A to give 3A.``() =
            addProgram.Run A ATimes2 |> should equal ATimes3

    [<Fact>] member test.
        ``The subtractTemplate subtracts A from 3A to give 2A.``() =
            subtractProgram.Run ATimes3 A |> should equal ATimes2

    [<Fact>] member test.
        ``The scalarMultiplyTemplate multiplies A by 3 to give 3A.``() =
            scalarMultiplyProgram.Run A 3.0f |> should equal ATimes3

type ``CUDA Matrix Activation``()=
    
    let A2By2 = array2D [ [0.1f; 0.2f];
                          [0.3f; 0.4f] ]
                          |> mapMatrix logitFunction
    let rnd2By2 = array2D [ [0.05f; 0.25f];
                            [0.42f; 0.38f] ]
    let res2By2 = array2D [ [1.0f; 0.0f];
                            [0.0f; 1.0f] ]

    let A2By4 = array2D [ [0.1f; 0.2f; 0.3f; 0.4f];
                          [0.5f; 0.6f; 0.7f; 0.8f] ]
                          |> mapMatrix logitFunction
    let rnd2By4 = array2D [ [0.05f; 0.67f; 0.12f; 0.75f];
                            [0.95f; 0.37f; 0.65f; 0.12f] ]
    let res2By4 = array2D [ [1.0f; 0.0f; 1.0f; 0.0f];
                            [0.0f; 1.0f; 1.0f; 1.0f] ]

    let A4By2 = array2D [ [0.1f; 0.5f];
                          [0.2f; 0.6f];
                          [0.3f; 0.7f];
                          [0.4f; 0.8f] ]
                          |> mapMatrix logitFunction
    let rnd4By2 = array2D [ [0.05f; 0.95f];
                            [0.67f; 0.37f];
                            [0.12f; 0.65f];
                            [0.75f; 0.12f] ]
    let res4By2 = array2D [ [1.0f; 0.0f];
                            [0.0f; 1.0f];
                            [1.0f; 1.0f];
                            [0.0f; 1.0f] ]

    let rnd2By4With3FirstRowActivations = array2D [ [1.00f; 1.00f; 1.00f; 0.00f];
                                                    [0.95f; 0.37f; 0.65f; 0.12f] ]
    let rnd4By2With3FirstRowActivations = array2D [ [1.00f; 1.00f];
                                                    [0.67f; 0.37f];
                                                    [0.12f; 0.65f];
                                                    [0.75f; 0.12f] ]

    let rnd2By4With3FirstColumnActivations = array2D [ [1.00f; 0.67f; 0.12f; 0.75f];
                                                       [1.00f; 0.37f; 0.65f; 0.12f] ]
    let rnd4By2With3FirstColumnActivations = array2D [ [1.00f; 0.95f];
                                                       [1.00f; 0.37f];
                                                       [1.00f; 0.65f];
                                                       [0.00f; 0.12f] ]

    let activateFirstRowTemplate (blockSize : int) (nActivations : int) = cuda {
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let activateFirstRowKernel = program.Apply activateFirstRowKernel

            fun (A : Matrix) ->
                let hA = height A
                let wA = width A
                let paddedA = padToMultiplesOf blockSize A
                let hPaddedA = height paddedA
                let wPaddedA = width paddedA
                let flattenedA = flattenMatrix paddedA

                use flattenedA = worker.Malloc flattenedA
                let lp = createActivateFirstRowLp blockSize hPaddedA wPaddedA
                activateFirstRowKernel.Launch lp flattenedA.Ptr wPaddedA nActivations

                flattenedA.Gather() |> rebuildMatrix wPaddedA |> topLeftSubmatrix hA wA
        )
    }

    let activateFirstColumnTemplate (blockSize : int) (nActivations : int) = cuda {
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let activateFirstColumnKernel = program.Apply activateFirstColumnKernel

            fun (A : Matrix) ->
                let hA = height A
                let wA = width A
                let paddedA = padToMultiplesOf blockSize A
                let hPaddedA = height paddedA
                let wPaddedA = width paddedA
                let flattenedA = flattenMatrix paddedA

                use flattenedA = worker.Malloc flattenedA
                let lp = createActivateFirstColumnLp blockSize hPaddedA wPaddedA
                activateFirstColumnKernel.Launch lp flattenedA.Ptr hPaddedA wPaddedA nActivations

                flattenedA.Gather() |> rebuildMatrix wPaddedA |> topLeftSubmatrix hA wA
        )
    }

    let activateTemplate (blockSize : int) = cuda {
        let! activateKernel =  <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let activateKernel = program.Apply activateKernel

            fun (A : Matrix) (rnd : Matrix) ->
                Common.simpleMatrixOperation blockSize A rnd activateKernel worker
        )
    }

    let activateProgram = 2 |> activateTemplate |> Compiler.load Worker.Default
    let activateFirstRowProgram = activateFirstRowTemplate 3 3 |> Compiler.load Worker.Default
    let activateFirstColumnProgram = activateFirstColumnTemplate 3 3 |> Compiler.load Worker.Default

    [<Fact>] member test.
        ``The activate template activates the 2 by 2 matrix correctly.``()=
            activateProgram.Run A2By2 rnd2By2 |> should equal res2By2

    [<Fact>] member test.
        ``The activate template activates the 2 by 4 matrix correctly.``()=
            activateProgram.Run A2By4 rnd2By4 |> should equal res2By4

    [<Fact>] member test.
        ``The activate template activates the 4 by 2 matrix correctly.``()=
            activateProgram.Run A4By2 rnd4By2 |> should equal res4By2

    [<Fact>] member test.
        ``The activateFirstRow template activates the top row of a 2 by 4 matrix correctly.``()=
            activateFirstRowProgram.Run rnd2By4 |> should equal rnd2By4With3FirstRowActivations

    [<Fact>] member test.
        ``The activateFirstRow template activates the top row of a 4 by 2 matrix correctly.``()=
            activateFirstRowProgram.Run rnd4By2 |> should equal rnd4By2With3FirstRowActivations

    [<Fact>] member test.
        ``The activateFirstColumn template activates the left column of a 2 by 4 matrix correctly.``()=
            activateFirstColumnProgram.Run rnd2By4 |> should equal rnd2By4With3FirstColumnActivations

    [<Fact>] member test.
        ``The activateFirstColumn template activates the left column of a 4 by 2 matrix correctly.``()=
            activateFirstColumnProgram.Run rnd4By2 |> should equal rnd4By2With3FirstColumnActivations

type ``CUDA DBN Epoch``() =

    let sizes = [500; 250; 100; 50]
    let alpha = 0.5f
    let momentum = 0.9f
    let xInputs = Array2D.init 100 784 (fun _ _ -> rand.NextDouble() |> float32)
    let layeredDbn = dbn sizes xInputs
    let firstRbm = layeredDbn.[0]

    let cudaDbnEpochProgram = 32 |> runDbnEpochTemplate |> Compiler.load Worker.Default
    let result = [1..5] |> List.fold (fun acc element -> cudaDbnEpochProgram.Run alpha momentum 10 acc xInputs) firstRbm

    [<Fact>] member test.
        ``The DBN Epoch template runs an epoch on the GPU.``()=
            (height result.Weights, width result.Weights) |> should equal (500, 784)
