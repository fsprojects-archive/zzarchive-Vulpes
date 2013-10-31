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

    let blockSize = 32
    let multiplyMatricesProgram = blockSize |> matrixMulTemplate |> Compiler.load Worker.Default
    let productOfMatrices = multiplyMatricesProgram.Run A B

    [<Fact>] member test.
        ``The matrixMulModule multiplies A by B.``() =
            productOfMatrices |> should equal C

