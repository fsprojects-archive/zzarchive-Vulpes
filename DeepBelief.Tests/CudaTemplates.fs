namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief.DeepBeliefNet
open DeepBelief.CudaTemplates
open DeepBelief.Utils

type ``CUDA Matrix Multiplication``()=

    let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ]
    let B = array2D [ [1.0f; 2.0f]; [3.0f; 4.0f]; [5.0f; 6.0f] ]
    let C = array2D [ [22.0f; 28.0f]; [49.0f; 64.0f] ]
    
    let D = array2D [ [1.0f; 2.0f;]
                      [3.0f; 4.0f;] 
                      [5.0f; 6.0f;] ];
    let E = array2D [ [1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f; 7.0f; 8.0f];
                      [2.0f; 4.0f; 6.0f; 8.0f; 1.0f; 3.0f; 5.0f; 7.0f] ]

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

    let loadAndMultiplyMatricesBlock1Program = 1 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let loadAndMultiplyMatricesBlock32Program = 32 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let loadAndMultiplyByTransposeProgram = 2 |> loadAndMultiplyByTransposeTemplate |> Compiler.load Worker.Default
    let loadTransposeAndMultiplyProgram = 2 |> loadTransposeAndMultiplyTemplate |> Compiler.load Worker.Default
    let powerProgram = 32 |> powerOfNTemplate |> Compiler.load Worker.Default

    let temp = loadTransposeAndMultiplyProgram.Run Dt E

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

type ``CUDA DBN Epoch``() =

    let sizes = [500; 250; 100; 50]
    let alpha = 0.5f
    let momentum = 0.9f
    let xInputs = Array2D.init 100 784 (fun _ _ -> rand.NextDouble() |> float32)
    let layeredDbn = dbn sizes xInputs
    let firstRbm = layeredDbn.[0]

    let cudaDbnEpochProgram = 32 |> runDbnEpochTemplate |> Compiler.load Worker.Default

    [<Fact>] member test.
        ``The DBN Epoch template runs an epoch on the GPU.``()=
            cudaDbnEpochProgram.Run alpha momentum 10 firstRbm xInputs |> should equal alpha
