namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief.Utils
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic

type ``Numerical Utilities``() =
    let M = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ]
    let X = [| 1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f |]
    
    let MPaddedTo5 =
        array2D   [ [1.0f; 2.0f; 3.0f; 0.0f; 0.0f];  
                    [4.0f; 5.0f; 6.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f] ]   
    
    let MPaddedTo3 =
        array2D   [ [1.0f; 2.0f; 3.0f;];  
                    [4.0f; 5.0f; 6.0f;];  
                    [0.0f; 0.0f; 0.0f;]; ]   

    [<Fact>] member test.
        ``The height of M is 2``() =
            height M |> should equal 2

    [<Fact>] member test.
        ``The width of M is 3``() =
            width M |> should equal 3

    [<Fact>] member test.
        ``M flattens to the 1 to 6 array``() =
            flatten M |> should equal X

    [<Fact>] member test.
        ``The 1 to 6 array stacks up to M``() =
            stackRows 3 X |> should equal M

    [<Fact>] member test.
        ``M is padded out to multiples of 5 correctly.``() =
            padToMultiplesOf 5 M |> should equal MPaddedTo5

    [<Fact>] member test.
        ``M is padded out to multiples of 1 correctly.``() =
            padToMultiplesOf 1 M |> should equal M

    [<Fact>] member test.
        ``M is padded out to multiples of 3 correctly.``() =
            padToMultiplesOf 3 M |> should equal MPaddedTo3

    [<Fact>] member test.
        ``The padded versions of M reduce back to M.``() =
            (topLeftSubmatrix 2 3 MPaddedTo3, topLeftSubmatrix 2 3 MPaddedTo5) |> should equal (M, M)

    [<Fact>] member test.
        ``6 padded out to a multiple of 1 is 6.``() =
            nextMultipleOf 1 6 |> should equal 6

    [<Fact>] member test.
        ``6 padded out to a multiple of 3 is 6.``() =
            nextMultipleOf 3 6 |> should equal 6

    [<Fact>] member test.
        ``7 padded out to a multiple of 3 is 9.``() =
            nextMultipleOf 3 7 |> should equal 9

    [<Fact>] member test.
        ``The sumOfRows function maps the identity matrix to a vector of ones.``()=
            DenseMatrix.Identity 10 |> sumOfRows |> Vector.forall(fun i -> i = 1.0) |> should equal true
