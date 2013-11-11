namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief
open DeepBeliefNet
open System
open Utils
open TestUtils

type ``Given a Deep belief network with four layers`` ()=
    let sin x = Math.Sin (float x) |> float32

    let sizes = [500; 250; 100; 50]
    let alpha = 0.5f
    let momentum = 0.9f
    let xInputs = Array2D.init 1000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let sinInput = [|1..784|] |> Array.map (fun x -> (1.0f + sin (12.0f * (x |> float32)/784.0f))/2.0f) |> fun row -> array2D [|row|]
    let layeredDbn = dbn sizes xInputs
    let sinTrainedRbm = rbmTrain rand alpha momentum 1 2 layeredDbn.[0] sinInput

    let (rows0, Drows0) = (height layeredDbn.[0].Weights, height layeredDbn.[0].DWeights)
    let (columns0, Dcolumns0) = (width layeredDbn.[0].Weights, width layeredDbn.[0].DWeights)
    let (v0, Dv0) = (layeredDbn.[0].VisibleBiases, layeredDbn.[0].DVisibleBiases)
    let (h0, Dh0) = (layeredDbn.[0].HiddenBiases, layeredDbn.[0].DHiddenBiases)

    let (rows1, Drows1) = (height layeredDbn.[1].Weights, height layeredDbn.[1].DWeights)
    let (columns1, Dcolumns1) = (width layeredDbn.[1].Weights, width layeredDbn.[1].DWeights)
    let (v1, Dv1) = (layeredDbn.[1].VisibleBiases, layeredDbn.[1].DVisibleBiases)
    let (h1, Dh1) = (layeredDbn.[1].HiddenBiases, layeredDbn.[1].DHiddenBiases)

    let batch = Array2D.init 10 784 (fun i j -> (i - 5) * (j - 392) |> float32)
    let inputs = Array2D.init 100 784 (fun i j -> rand.NextDouble() |> float32)

    [<Fact>] member test.
        ``The length of the DBN should be 4.``()=
        layeredDbn.Length |> should equal 4

    [<Fact>] member test.
        ``The weights of the first RBM should be a 500 by 784 matrix.``()=
        (rows0, columns0, Drows0, Dcolumns0) |> should equal (500, 784, 500, 784)

    [<Fact>] member test.
        ``The visible biases of the first RBM should be a vector of length 784.``()=
        (Array.length v0, Array.length Dv0) |> should equal (784, 784)

    [<Fact>] member test.
        ``The hidden biases of the first RBM should be a vector of length 500.``()=
        (Array.length h0, Array.length Dh0) |> should equal (500, 500)

    [<Fact>] member test.
        ``The weights of the second RBM should be a 250 by 500 matrix.``()=
        (rows1, columns1, Drows1, Dcolumns1) |> should equal (250, 500, 250, 500)

    [<Fact>] member test.
        ``The visible biases of the second RBM should be a vector of length 500.``()=
        (Array.length v1, Array.length Dv1) |> should equal (500, 500)

    [<Fact>] member test.
        ``The hidden biases of the second RBM should be a vector of length 250.``()=
        (Array.length h1, Array.length Dh1) |> should equal (250, 250)

    [<Fact>] member test.
        ``The weight differences of all RBMs should be initialised to zero.``()=
        layeredDbn |> List.map(fun x -> x.DWeights |> allElementsOfMatrix (fun v -> v = 0.0f))
        |> should equal (layeredDbn |> List.map(fun x -> true))
    
    [<Fact>] member test.
        ``The visible bias differences of all RBMs should be initialised to zero.``()=
        layeredDbn |> List.map(fun x -> x.DVisibleBiases |> allElementsOfVector (fun v -> v >= 0.0f))
        |> should equal (layeredDbn |> List.map(fun x -> true))    

    [<Fact>] member test.
        ``The hidden bias differences of all RBMs should be initialised to zero.``()=
        layeredDbn |> List.map(fun x -> x.DHiddenBiases |> allElementsOfVector (fun v -> v = 0.0f))
        |> should equal (layeredDbn |> List.map(fun x -> true))

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix of the same size.``()=
        activate rand sigmoid batch |> (fun x -> (height x, width x)) |> should equal (10, 784)

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix containing ones and zeroes.``()=
        activate rand sigmoid batch |> allElementsOfMatrix (fun x -> x * (x - 1.0f) = 0.0f) |> should equal true

    [<Fact>] member test.
        ``The forward iteration of the first RBM converts ten samples of length 784 into ten samples of size 500.``()=
         batch |> forward layeredDbn.[0] |> (fun x -> (height x, width x)) |> should equal (500, 10)

    [<Fact>] member test.
        ``The backward iteration of the first RBM converts ten samples of length 500 into ten samples of length 784.``()=
         batch |> forward layeredDbn.[0] |> activate rand sigmoid |> backward layeredDbn.[0] |> (fun x -> (height x, width x)) |> should equal (10, 784)

    [<Fact>] member test.
        ``The first epoch gives a positive error.``()=
        rbmEpoch rand alpha momentum 10 layeredDbn.[0] inputs |> fst |> should greaterThan 0.0f

    [<Fact>] member test.
        ``The first epoch gives an RBM with non-zero weights.``()=
        rbmEpoch rand alpha momentum 10 layeredDbn.[0] inputs |> snd |> (fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty) |> should equal false 

    [<Fact>] member test.
        ``Training 50 epochs of the first RBM gives an RBM with non-zero weights.``()=
        rbmTrain rand alpha momentum 1 50 layeredDbn.[0] sinInput |> fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty |> should equal false 

    [<Fact>] member test.
        ``Training 50 epochs of the DBN gives an RBM with non-zero weights.``()=
        dbnTrain rand alpha momentum 1 50 layeredDbn sinInput |> List.rev |> List.head |> fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty |> should equal false 

    [<Fact>] member test.
        ``The flattened RBM is in the correct format.``()=
        layeredDbn.[0] |> flattenRbm 
        |> fun x -> (x.[0..499], x.[500..999], x.[1000..1783], x.[1784..2567], x.[2568..394567], x.[394568..786567]) 
        |> should equal 
            (layeredDbn.[0].HiddenBiases, layeredDbn.[0].DHiddenBiases,
            layeredDbn.[0].VisibleBiases, layeredDbn.[0].DVisibleBiases,
            flattenMatrix layeredDbn.[0].Weights, flattenMatrix layeredDbn.[0].DWeights)

    [<Fact>] member test.
        ``The size of the flattened RBM agrees with the sizeOfRbm function.``()=
        layeredDbn.[0] |> flattenRbm |> Array.length |> should equal (sizeOfRbm layeredDbn.[0])

    [<Fact>] member test.
        ``The flattened RBM is subsequently rebuilt correctly.``()=
        layeredDbn.[0] |> flattenRbm |> rebuildRbm 784 500 |> should equal layeredDbn.[0]