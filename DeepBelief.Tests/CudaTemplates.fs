namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief.CudaTemplates

type ``Matrix Multiplication`` () =

    let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ]
    let B = array2D [ [1.0f; 2.0f]; [3.0f; 4.0f]; [5.0f; 6.0f] ]
    let C = array2D [ [22.0f; 28.0f]; [49.0f; 64.0f] ]

    let At = array2D [ [1.0f; 4.0f]; [2.0f; 5.0f]; [3.0f; 6.0f] ]
    let Bt = array2D [ [1.0f; 3.0f; 5.0f]; [2.0f; 4.0f; 6.0f] ]

    let M = array2D [ [2.0f; 0.0f]; [0.0f; 2.0f] ]
    let MtoN n =
        array2D [ [pown 2.0f n; 0.0f]; [0.0f; pown 2.0f n] ]

    let UpperTriangle a b =
        array2D [ [a; b]; [0.0f; a] ]

    let UpperTriangleToN n a b =
        let aToN = pown a n
        array2D [ [aToN; (float32 n) * pown a (n - 1) * b]; [0.0f; aToN] ]

    let loadAndMultiplyMatricesBlock1Program = 1 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let loadAndMultiplyMatricesBlock32Program = 32 |> loadAndMultiplyTemplate |> Compiler.load Worker.Default
    let productOfMatricesBlock1 = loadAndMultiplyMatricesBlock1Program.Run A B
    let productOfMatricesBlock32 = loadAndMultiplyMatricesBlock32Program.Run A B

    let loadAndMultiplyByTransposeProgram = 32 |> loadAndMultiplyByTransposeTemplate |> Compiler.load Worker.Default
    let productOfAWithTransposeOfBTranspose = loadAndMultiplyByTransposeProgram.Run A Bt

    let loadTransposeAndMultiplyProgram = 32 |> loadTransposeAndMultiplyTemplate |> Compiler.load Worker.Default
    let productOfTransposeOfATransposeWithB = loadTransposeAndMultiplyProgram.Run At B

    let powerProgram = 32 |> powerOfNTemplate |> Compiler.load Worker.Default
    let MToThePowerOf10 = powerProgram.Run M 10
    let UTToThePowerOf10 = UpperTriangle 2.0f 3.0f |> fun m -> powerProgram.Run m 10

    [<Fact>] member test.
        ``The loadAndMultiplyTemplate multiplies A by B with a block size of 1.``() =
            productOfMatricesBlock1 |> should equal C

    [<Fact>] member test.
        ``The loadAndMultiplyTemplate multiplies A by B with a block size of 32.``() =
            productOfMatricesBlock32 |> should equal C

    [<Fact>] member test.
        ``The powerOfNTemplate raises M to the power of 10.``() =
            MToThePowerOf10 |> should equal (MtoN 10)

    [<Fact>] member test.
        ``The powerOfNTemplate raises an Upper Triangular matrix to the power of 10.``() =
            UTToThePowerOf10 |> should equal (UpperTriangleToN 10 2.0f 3.0f)

    [<Fact>] member test.
        ``The loadAndMultiplyByTransposeTemplate multiplies A by the transpose of (B Transpose) to give AB.``() =
            productOfAWithTransposeOfBTranspose |> should equal C

    [<Fact>] member test.
        ``The loadTransposeAndMultiplyTemplate multiplies the transpose of (A Transpose) by B to give AB.``() =
            productOfTransposeOfATransposeWithB |> should equal C
