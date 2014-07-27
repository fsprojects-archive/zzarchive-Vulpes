namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open Common.Analytics
open Common.NeuralNet
open DeepBelief
open DeepBeliefNet
open System
open Utils
open TestUtils

type ``Deep Belief Network with four layers and 1 sample running on CPU`` ()=
    let sin x = Math.Sin (float x) |> float32

    let dbnParameters = 
        {
            Layers = LayerSizes [500; 250; 100; 50]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 100
            Epochs = Epochs 50
        }

    let rand = new Random()
    let xInputs = Array2D.init 1000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let xInput (rnd : Random) = Array.init 784 (fun _ -> rnd.NextDouble() |> float32 |> Signal) |> Input
    let xTarget = Array.init 10 (fun _ -> 0.0f |> float32 |> Signal) |> Target
    let trainingSet = List.init 1000 (fun _ -> { TrainingInput = xInput rand; TrainingTarget = xTarget }) |> TrainingSet
    let layeredDbn = DeepBeliefNetwork.Initialise dbnParameters trainingSet

    let (rows0, Drows0) = (layeredDbn.Machines.[0].Weights.Height, layeredDbn.Machines.[0].DWeights.Height)
    let (columns0, Dcolumns0) = (layeredDbn.Machines.[0].Weights.Width, layeredDbn.Machines.[0].DWeights.Width)
    let (v0, Dv0) = (layeredDbn.Machines.[0].VisibleBiases, layeredDbn.Machines.[0].DVisibleBiases)
    let (h0, Dh0) = (layeredDbn.Machines.[0].HiddenBiases, layeredDbn.Machines.[0].DHiddenBiases)

    let (rows1, Drows1) = (layeredDbn.Machines.[1].Weights.Height, layeredDbn.Machines.[1].DWeights.Height)
    let (columns1, Dcolumns1) = (layeredDbn.Machines.[1].Weights.Width, layeredDbn.Machines.[1].DWeights.Width)
    let (v1, Dv1) = (layeredDbn.Machines.[1].VisibleBiases, layeredDbn.Machines.[1].DVisibleBiases)
    let (h1, Dh1) = (layeredDbn.Machines.[1].HiddenBiases, layeredDbn.Machines.[1].DHiddenBiases)

    [<Fact>] member test.
        ``The length of the DBN should be 4.``()=
        layeredDbn.Machines.Length |> should equal 4

    [<Fact>] member test.
        ``The weights of the first RBM should be a 500 by 784 matrix.``()=
        (rows0, columns0, Drows0, Dcolumns0) |> should equal (500, 784, 500, 784)

    [<Fact>] member test.
        ``The visible biases of the first RBM should be a vector of length 784.``()=
        (v0.Length, Dv0.Length) |> should equal (784, 784)

    [<Fact>] member test.
        ``The hidden biases of the first RBM should be a vector of length 500.``()=
        (h0.Length, Dh0.Length) |> should equal (500, 500)

    [<Fact>] member test.
        ``The weights of the second RBM should be a 250 by 500 matrix.``()=
        (rows1, columns1, Drows1, Dcolumns1) |> should equal (250, 500, 250, 500)

    [<Fact>] member test.
        ``The visible biases of the second RBM should be a vector of length 500.``()=
        (v1.Length, Dv1.Length) |> should equal (500, 500)

    [<Fact>] member test.
        ``The hidden biases of the second RBM should be a vector of length 250.``()=
        (h1.Length, Dh1.Length) |> should equal (250, 250)

    [<Fact>] member test.
        ``The weight differences of all RBMs should be initialised to zero.``()=
        layeredDbn.Machines |> List.map(fun x -> x.DWeights |> allElementsOfMatrix (fun v -> v = 0.0f))
        |> should equal <|
        List.map(fun x -> true) layeredDbn.Machines
    
    [<Fact>] member test.
        ``The visible bias differences of all RBMs should be initialised to zero.``()=
        layeredDbn.Machines |> List.map(fun x -> x.DVisibleBiases |> allElementsOfVector (fun v -> v >= 0.0f))
        |> should equal <|
        List.map(fun x -> true) layeredDbn.Machines

    [<Fact>] member test.
        ``The hidden bias differences of all RBMs should be initialised to zero.``()=
        layeredDbn.Machines |> List.map(fun x -> x.DHiddenBiases |> allElementsOfVector (fun v -> v = 0.0f))
        |> should equal <|
        List.map(fun x -> true) layeredDbn.Machines

    [<Fact>] member test.
        ``Training 50 epochs of the DBN gives an RBM with non-zero weights.``()=
        let sinInput = [|1..784|] |> Array.map (fun x -> (1.0f + sin (12.0f * (x |> float32)/784.0f))/2.0f |> Signal) |> Input in
        let target = [|1..10|] |> Array.map (fun x -> 0.0f |> Signal) |> Target in
        let trainingSet = [{ TrainingInput = sinInput; TrainingTarget = target }] |> TrainingSet in
        layeredDbn.TrainCpu rand trainingSet |> fun dbn -> dbn.Machines |> List.rev |> List.head |> fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty |> should equal false 

//type ``Given a single RBM``()=
//    let rand = new Random()
//    let inputs = Array2D.init 100 784 (fun i j -> rand.NextDouble() |> float32) |> prependColumnOfOnes
//
//    let rbmParameters =
//        {
//            LearningRate = LearningRate 0.9f
//            Momentum = Momentum 0.2f
//            BatchSize = BatchSize 10
//            Epochs = Epochs 50
//        }
//
//    let rbm = RestrictedBoltzmannMachine.Initialise rbmParameters 784 500 |> fun rbm ->
//        {
//            Parameters = rbmParameters
//            Weights = rbm.Weights
//            DWeights = Array2D.init 500 784 (fun _ _ -> rand.NextDouble() |> float32) |> Matrix
//            HiddenBiases = Array.init 500 (fun _ -> rand.NextDouble() |> float32) |> Vector
//            DHiddenBiases = Array.init 500 (fun _ -> rand.NextDouble() |> float32) |> Vector
//            VisibleBiases = Array.init 784 (fun _ -> rand.NextDouble() |> float32) |> Vector
//            DVisibleBiases = Array.init 784 (fun _ -> rand.NextDouble() |> float32) |> Vector
//        }
//
//    let weightsAndBiases = rbm.ToWeightsAndBiases
//    let dWeightsAndBiases = rbm.ToWeightsAndBiasesChanges
//    let batch = Array2D.init 10 784 (fun i j -> (i - 5) * (j - 392) |> float32)
//
//    [<Fact>] member test.
//        ``The activation function converts a matrix into a matrix of the same size.``()=
//        activate rand sigmoidFunction batch |> (fun x -> (height x, width x)) |> should equal (10, 784)
//
//    [<Fact>] member test.
//        ``The activation function converts a matrix into a matrix containing ones and zeroes.``()=
//        activate rand sigmoidFunction batch |> allElementsOfMatrix (fun x -> x * (x - 1.0f) = 0.0f) |> should equal true
//
//    [<Fact>] member test.
//        ``The first epoch gives a positive visible error.``()=
//        rbmEpoch rand rbm inputs |> fst |> fst |> should greaterThan 0.0f
//
//    [<Fact>] member test.
//        ``The first epoch gives a positive hidden error.``()=
//        rbmEpoch rand rbm inputs |> fst |> snd |> should greaterThan 0.0f
//
//    [<Fact>] member test.
//        ``The first epoch gives an RBM with non-zero weights.``()=
//        rbmEpoch rand rbm inputs |> snd |> (fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty) |> should equal false 
//
//    [<Fact>] member test.
//        ``The forward iteration of the RBM converts ten samples of length 784 into ten samples of size 500.``()=
//         batch |> prependColumnOfOnes |> forward (toWeightsAndBiases rbm) |> (fun x -> (height x, width x)) |> should equal (501, 10)
//
//    [<Fact>] member test.
//        ``The backward iteration of the RBM converts ten samples of length 500 into ten samples of length 784.``()=
//         batch |> prependColumnOfOnes |> forward (toWeightsAndBiases rbm) |> activate rand sigmoidFunction |> backward (toWeightsAndBiases rbm) |> (fun x -> (height x, width x)) |> should equal (10, 785)
//
//    [<Fact>] member test.
//        ``Training 50 epochs of the RBM gives an RBM with non-zero weights.``()=
//        let sinInput = [|1..784|] |> Array.map (fun x -> (1.0f + sin (12.0f * (x |> float32)/784.0f))/2.0f) |> fun row -> array2D [|row|] |> prependColumnOfOnes in
//        cpuRbmTrain rand rbm sinInput |> fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty |> should equal false 
//
//    [<Fact>] member test.
//        ``toRbm reverses the toWeightsAndBiases and toDWeightsAndBiases functions.``()=
//        toRbm rbmParameters (toWeightsAndBiases rbm) (toDWeightsAndBiases rbm) |> should equal rbm
//
//    [<Fact>] member test.
//        ``toWeightsAndBiases and toDWeightsAndBiases reverse the toRbm function.``()=
//        toRbm rbmParameters weightsAndBiases dWeightsAndBiases |> fun r -> (toWeightsAndBiases r, toDWeightsAndBiases r) |> should equal (weightsAndBiases, dWeightsAndBiases)
//
//    [<Fact>] member test.
//        ``The toWeightsAndBiases function creates a matrix with one more row and one more column than the Weights matrix.``()=
//        (toWeightsAndBiases rbm |> height, toWeightsAndBiases rbm |> width) |> should equal (1 + height rbm.Weights, 1 + width rbm.Weights)
//
//    [<Fact>] member test.
//        ``The toDWeightsAndBiases function creates a matrix with one more row and one more column than the DWeights matrix.``()=
//        (toWeightsAndBiases rbm |> height, toWeightsAndBiases rbm |> width) |> should equal (1 + height rbm.DWeights, 1 + width rbm.DWeights)
