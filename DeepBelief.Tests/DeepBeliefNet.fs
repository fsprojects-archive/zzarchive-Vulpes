namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief.DeepBeliefNet
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
open System

type ``Given a Deep belief network with two layers`` ()=
    let sizes = [100; 50]
    let alpha = 1.0
    let momentum = 0.0
    let xInputs = DenseMatrix.zeroCreate 60000 784
    let twoLayerDbn = dbn sizes alpha momentum xInputs

    let (rows0, Drows0) = (twoLayerDbn.[0].Weights.RowCount, twoLayerDbn.[0].DWeights.RowCount)
    let (columns0, Dcolumns0) = (twoLayerDbn.[0].Weights.ColumnCount, twoLayerDbn.[0].DWeights.ColumnCount)
    let (v0, Dv0) = (twoLayerDbn.[0].VisibleBiases, twoLayerDbn.[0].DVisibleBiases)
    let (h0, Dh0) = (twoLayerDbn.[0].HiddenBiases, twoLayerDbn.[0].DHiddenBiases)

    let (rows1, Drows1) = (twoLayerDbn.[1].Weights.RowCount, twoLayerDbn.[1].DWeights.RowCount)
    let (columns1, Dcolumns1) = (twoLayerDbn.[1].Weights.ColumnCount, twoLayerDbn.[1].DWeights.ColumnCount)
    let (v1, Dv1) = (twoLayerDbn.[1].VisibleBiases, twoLayerDbn.[1].DVisibleBiases)
    let (h1, Dh1) = (twoLayerDbn.[1].HiddenBiases, twoLayerDbn.[1].DHiddenBiases)

    let batch = DenseMatrix.init 10 784 (fun i j -> (i - 50) * (j - 392) |> float)

    [<Fact>] member test.
        ``The length of the DBN should be 2.``()=
        twoLayerDbn.Length |> should equal 2

    [<Fact>] member test.
        ``The weights of the first RBM should be a 100 by 784 matrix.``()=
        (rows0, columns0, Drows0, Dcolumns0) |> should equal (100, 784, 100, 784)

    [<Fact>] member test.
        ``The visible biases of the first RBM should be a vector of length 784.``()=
        (v0.Count, Dv0.Count) |> should equal (784, 784)

    [<Fact>] member test.
        ``The hidden biases of the first RBM should be a vector of length 100.``()=
        (h0.Count, Dh0.Count) |> should equal (100, 100)

    [<Fact>] member test.
        ``The weights of the second RBM should be a 50 by 100 matrix.``()=
        (rows1, columns1, Drows1, Dcolumns1) |> should equal (50, 100, 50, 100)

    [<Fact>] member test.
        ``The visible biases of the second RBM should be a vector of length 100.``()=
        (v1.Count, Dv1.Count) |> should equal (100, 100)

    [<Fact>] member test.
        ``The hidden biases of the second RBM should be a vector of length 50.``()=
        (h1.Count, Dh1.Count) |> should equal (50, 50)

    [<Fact>] member test.
        ``Each RBM should be initialised to have zero momentum and unit alpha.``()=
        twoLayerDbn |> List.map (fun x -> (x.Alpha, x.Momentum)) 
        |> List.forall (fun x -> x = (1.0, 0.0)) 
        |> should equal true

    [<Fact>] member test.
        ``The weights of all RBMs should be initialised to zero.``()=
        twoLayerDbn |> List.map(fun x -> (x.Weights |> Matrix.forall (fun v -> v = 0.0), x.DWeights |> Matrix.forall (fun v -> v = 0.0)))
        |> should equal (twoLayerDbn |> List.map(fun x -> (true, true)))
    
    [<Fact>] member test.
        ``The visible biases of all RBMs should be initialised to zero.``()=
        twoLayerDbn |> List.map(fun x -> (x.VisibleBiases |> Vector.forall (fun v -> v = 0.0), x.DVisibleBiases |> Vector.forall (fun v -> v = 0.0)))
        |> should equal (twoLayerDbn |> List.map(fun x -> (true, true)))    

    [<Fact>] member test.
        ``The hidden biases of all RBMs should be initialised to zero.``()=
        twoLayerDbn |> List.map(fun x -> (x.HiddenBiases |> Vector.forall (fun v -> v = 0.0), x.DHiddenBiases |> Vector.forall (fun v -> v = 0.0)))
        |> should equal (twoLayerDbn |> List.map(fun x -> (true, true)))

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix of the same size.``()=
        activate rand sigmoid batch |> (fun x -> (x.RowCount, x.ColumnCount)) |> should equal (10, 784)

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix containing ones and zeroes.``()=
        activate rand sigmoid batch |> Matrix.forall(fun x -> x * (x - 1.0) = 0.0) |> should equal true

    [<Fact>] member test.
        ``The forward iteration of the first RBM converts ten samples of length 784 into ten samples of length 100.``()=
         batch |> forward twoLayerDbn.[0] |> (fun x -> (x.RowCount, x.ColumnCount)) |> should equal (10, 100)

    [<Fact>] member test.
        ``The backward iteration of the first RBM converts ten samples of length 100 into ten samples of length 784.``()=
         batch |> forward twoLayerDbn.[0] |> activate rand sigmoid |> backward twoLayerDbn.[0] |> (fun x -> (x.RowCount, x.ColumnCount)) |> should equal (10, 784)

    [<Fact>] member test.
        ``Permute 10 gives an array of length 10 containing each of the digits 0 to 9.``()=
        permute rand 10 |> List.sort |> should equal [0..9] 

    [<Fact>] member test.
        ``The permuteRows method preserves the dimensions of the batch matrix.``()=
        permuteRows rand batch |> (fun x -> (x.RowCount, x.ColumnCount)) |> should equal (10, 784)
