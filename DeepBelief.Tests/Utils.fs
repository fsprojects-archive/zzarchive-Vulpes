namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief.Utils
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic

type ``Numerical Utilities``() =
    let M = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ]

    [<Fact>] member test.
        ``The height of M is 2``() =
            height M |> should equal 2

    [<Fact>] member test.
        ``The width of M is 3``() =
            width M |> should equal 3

    [<Fact>] member test.
        ``M flattens to the 1 to 6 array``() =
            flatten M |> should equal [|1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f|]

    [<Fact>] member test.
        ``The sumOfRows function maps the identity matrix to a vector of ones.``()=
        DenseMatrix.Identity 10 |> sumOfRows |> Vector.forall(fun i -> i = 1.0) |> should equal true
