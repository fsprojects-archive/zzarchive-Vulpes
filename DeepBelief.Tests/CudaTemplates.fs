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

    let M = array2D [ [2.0f; 0.0f]; [0.0f; 2.0f] ]
    let MtoN n =
        array2D [ [pown 2.0f n; 0.0f]; [0.0f; pown 2.0f n] ]

    let multiplyMatricesBlock1Program = 1 |> matrixMulTemplate |> Compiler.load Worker.Default
    let multiplyMatricesBlock32Program = 32 |> matrixMulTemplate |> Compiler.load Worker.Default
    let productOfMatricesBlock1 = multiplyMatricesBlock1Program.Run A B
    let productOfMatricesBlock32 = multiplyMatricesBlock32Program.Run A B

    let MToThePowerOf10Program = 32 |> powerOfNTemplate |> Compiler.load Worker.Default
    let MToThePowerOf10 = MToThePowerOf10Program.Run M 10

    [<Fact>] member test.
        ``The matrixMulTemplate multiplies A by B with a block size of 1.``() =
            productOfMatricesBlock1 |> should equal C

    [<Fact>] member test.
        ``The matrixMulTemplate multiplies A by B with a block size of 32.``() =
            productOfMatricesBlock32 |> should equal C

    [<Fact>] member test.
        ``The powerOfNTemplate raises M to the power of 10.``() =
            MToThePowerOf10 |> should equal (MtoN 10)
