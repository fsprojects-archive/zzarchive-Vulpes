namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief
open DeepBeliefNet
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
open System
open Utils

type ``Given a Deep belief network with four layers`` ()=
    let sizes = [500; 250; 100; 50]
    let alpha = 0.5
    let momentum = 0.9
    let xInputs = DenseMatrix.init 1000 784 (fun _ _ -> rand.NextDouble())
    let sinInput = [1..784] |> List.map (fun x -> (1.0 + Math.Sin(12.0 * (x |> float)/784.0))/2.0) |> DenseVector.ofList |> fun row -> DenseMatrix.ofRowVectors [row]
    let layeredDbn = dbn sizes alpha momentum xInputs
    let sinTrainedRbm = rbmTrain rand 1 2 layeredDbn.[0] sinInput

    let (rows0, Drows0) = (layeredDbn.[0].Weights.RowCount, layeredDbn.[0].DWeights.RowCount)
    let (columns0, Dcolumns0) = (layeredDbn.[0].Weights.ColumnCount, layeredDbn.[0].DWeights.ColumnCount)
    let (v0, Dv0) = (layeredDbn.[0].VisibleBiases, layeredDbn.[0].DVisibleBiases)
    let (h0, Dh0) = (layeredDbn.[0].HiddenBiases, layeredDbn.[0].DHiddenBiases)

    let (rows1, Drows1) = (layeredDbn.[1].Weights.RowCount, layeredDbn.[1].DWeights.RowCount)
    let (columns1, Dcolumns1) = (layeredDbn.[1].Weights.ColumnCount, layeredDbn.[1].DWeights.ColumnCount)
    let (v1, Dv1) = (layeredDbn.[1].VisibleBiases, layeredDbn.[1].DVisibleBiases)
    let (h1, Dh1) = (layeredDbn.[1].HiddenBiases, layeredDbn.[1].DHiddenBiases)

    let batch = DenseMatrix.init 10 784 (fun i j -> (i - 5) * (j - 392) |> float)
    let inputs = DenseMatrix.init 100 784 (fun i j -> rand.NextDouble() |> float)

    [<Fact>] member test.
        ``The length of the DBN should be 4.``()=
        layeredDbn.Length |> should equal 4

    [<Fact>] member test.
        ``The weights of the first RBM should be a 500 by 784 matrix.``()=
        (rows0, columns0, Drows0, Dcolumns0) |> should equal (500, 784, 500, 784)

    [<Fact>] member test.
        ``The visible biases of the first RBM should be a vector of length 784.``()=
        (v0.Count, Dv0.Count) |> should equal (784, 784)

    [<Fact>] member test.
        ``The hidden biases of the first RBM should be a vector of length 500.``()=
        (h0.Count, Dh0.Count) |> should equal (500, 500)

    [<Fact>] member test.
        ``The weights of the second RBM should be a 250 by 500 matrix.``()=
        (rows1, columns1, Drows1, Dcolumns1) |> should equal (250, 500, 250, 500)

    [<Fact>] member test.
        ``The visible biases of the second RBM should be a vector of length 500.``()=
        (v1.Count, Dv1.Count) |> should equal (500, 500)

    [<Fact>] member test.
        ``The hidden biases of the second RBM should be a vector of length 250.``()=
        (h1.Count, Dh1.Count) |> should equal (250, 250)

    [<Fact>] member test.
        ``Each RBM should be initialised with the same momentum and alpha.``()=
        layeredDbn |> List.map (fun x -> (x.Alpha, x.Momentum)) 
        |> List.forall (fun x -> x = (0.5, 0.9)) 
        |> should equal true

    [<Fact>] member test.
        ``The weight differences of all RBMs should be initialised to zero.``()=
        layeredDbn |> List.map(fun x -> x.DWeights |> Matrix.forall (fun v -> v = 0.0))
        |> should equal (layeredDbn |> List.map(fun x -> true))
    
    [<Fact>] member test.
        ``The visible bias differences of all RBMs should be initialised to zero.``()=
        layeredDbn |> List.map(fun x -> x.DVisibleBiases |> Vector.forall (fun v -> v >= 0.0))
        |> should equal (layeredDbn |> List.map(fun x -> true))    

    [<Fact>] member test.
        ``The hidden bias differences of all RBMs should be initialised to zero.``()=
        layeredDbn |> List.map(fun x -> x.DHiddenBiases |> Vector.forall (fun v -> v = 0.0))
        |> should equal (layeredDbn |> List.map(fun x -> true))

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix of the same size.``()=
        activate rand sigmoid batch |> (fun x -> (x.RowCount, x.ColumnCount)) |> should equal (10, 784)

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix containing ones and zeroes.``()=
        activate rand sigmoid batch |> Matrix.forall(fun x -> x * (x - 1.0) = 0.0) |> should equal true

    [<Fact>] member test.
        ``The forward iteration of the first RBM converts ten samples of length 784 into ten samples of length 500.``()=
         batch |> forward layeredDbn.[0] |> (fun x -> (x.RowCount, x.ColumnCount)) |> should equal (10, 500)

    [<Fact>] member test.
        ``The backward iteration of the first RBM converts ten samples of length 500 into ten samples of length 784.``()=
         batch |> forward layeredDbn.[0] |> activate rand sigmoid |> backward layeredDbn.[0] |> (fun x -> (x.RowCount, x.ColumnCount)) |> should equal (10, 784)

    [<Fact>] member test.
        ``Permute 10 gives an array of length 10 containing each of the digits 0 to 9.``()=
        permute rand 10 |> List.sort |> should equal [0..9] 

    [<Fact>] member test.
        ``The permuteRows method preserves the dimensions of the batch matrix.``()=
        permuteRows rand batch |> (fun x -> (x.Length, x.Head.Length)) |> should equal (10, 784)

    [<Fact>] member test.
        ``The first epoch gives a positive error.``()=
        epoch rand 10 layeredDbn.[0] inputs |> fst |> should greaterThan 0.0

    [<Fact>] member test.
        ``The first epoch gives an RBM with non-zero weights.``()=
        epoch rand 10 layeredDbn.[0] inputs |> snd |> (fun r -> r.Weights |> Matrix.nonZeroEntries |> Seq.isEmpty) |> should equal false 

    [<Fact>] member test.
        ``The batchesOf function splits 1 to 10 up correctly.``()=
        [1..10] |> batchesOf 3 |> should equal [[1;2;3];[4;5;6];[7;8;9];[10]]

    [<Fact>] member test.
        ``Training 50 epochs of the first RBM gives an RBM with non-zero weights.``()=
        rbmTrain rand 1 50 layeredDbn.[0] sinInput |> fun r -> r.Weights |> Matrix.nonZeroEntries |> Seq.isEmpty |> should equal false 

    [<Fact>] member test.
        ``Training 50 epochs of the DBN gives an RBM with non-zero weights.``()=
        dbnTrain rand 1 50 layeredDbn sinInput |> List.rev |> List.head |> fun r -> r.Weights |> Matrix.nonZeroEntries |> Seq.isEmpty |> should equal false 

    [<Fact>] member test.
        ``The addVisibleBiases function adds the visible biases of an RBM to a row of zeroes.``()=
        DenseMatrix.zeroCreate 1 784 |> addVisibleBiases sinTrainedRbm |> fun m -> m.Row(0) |> should equal sinTrainedRbm.VisibleBiases

    [<Fact>] member test.
        ``The addHiddenBiases function adds the hidden biases of an RBM to a column of zeroes.``()=
        DenseMatrix.zeroCreate 500 1 |> addHiddenBiases sinTrainedRbm |> fun m -> m.Column(0) |> should equal sinTrainedRbm.HiddenBiases

    [<Fact>] member test.
        ``The proportionOfVisible units function gives 0.2 for the vector [0,1,0,0,0,0,0,1,0,0]``()=
        [0.0;1.0;0.0;0.0;0.0;0.0;0.0;1.0;0.0;0.0] |> DenseVector.ofList |> proportionOfVisibleUnits |> should equal 0.2