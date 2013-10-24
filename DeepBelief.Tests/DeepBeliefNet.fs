namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief.DeepBeliefNet
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
open System

type ``Given a Deep belief network with two layers`` ()=
    let sizes = [100; 100]
    let alpha = 1.0
    let momentum = 0.0
    let xInputs = DenseMatrix.zeroCreate 60000 784
    let twoLayerDbn = dbn sizes alpha momentum xInputs

    let rows0 = twoLayerDbn.Head.Weights.RowCount
    let columns0 = twoLayerDbn.Head.Weights.ColumnCount
    let v0 = twoLayerDbn.Head.VisibleBiases
    let h0 = twoLayerDbn.Head.HiddenBiases

    let rows1 = twoLayerDbn.[1].Weights.RowCount
    let columns1 = twoLayerDbn.[1].Weights.ColumnCount
    let v1 = twoLayerDbn.[1].VisibleBiases
    let h1 = twoLayerDbn.[1].HiddenBiases

    [<Fact>] member test.
        ``The length of the DBN should be 2.``()=
        twoLayerDbn.Length |> should equal 2

    [<Fact>] member test.
        ``The weights of the first RBM should be a 100 by 784 matrix.``()=
        (rows0, columns0) |> should equal (100, 784)

    [<Fact>] member test.
        ``The visible biases of the first RBM should be a vector of length 784.``()=
        v0.Count |> should equal 784

    [<Fact>] member test.
        ``The hidden biases of the first RBM should be a vector of length 100.``()=
        h0.Count |> should equal 784

    [<Fact>] member test.
        ``The weights of the second RBM should be a 100 by 100 matrix.``()=
        (rows1, columns1) |> should equal (100, 100)

    [<Fact>] member test.
        ``The visible biases of the second RBM should be a vector of length 100.``()=
        v1.Count |> should equal 100

    [<Fact>] member test.
        ``The hidden biases of the second RBM should be a vector of length 100.``()=
        h1.Count |> should equal 100

    [<Fact>] member test.
        ``Each RBM should have zero momentum and unit alpha.``()=
        twoLayerDbn |> List.map (fun x -> (x.Alpha, x.Momentum)) 
        |> List.forall (fun x -> x = (1.0, 0.0)) 
        |> should equal true
