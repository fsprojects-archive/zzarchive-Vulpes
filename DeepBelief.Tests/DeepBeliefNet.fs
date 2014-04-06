// The MIT License (MIT)
// 
// Copyright (c) 2014 SpiegelSoft Ltd
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
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
            LearningRateAlpha = LearningRate 0.5f
            MomentumEta = Momentum 0.9f
            BatchSize = BatchSize 100
            Epochs = Epochs 50
        }

    let rand = new Random()
    let xInputs = Array2D.init 1000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let sinInput = [|1..784|] |> Array.map (fun x -> (1.0f + sin (12.0f * (x |> float32)/784.0f))/2.0f) |> fun row -> array2D [|row|]
    let layeredDbn = initDbn dbnParameters xInputs
    let sinTrainedRbm = cpuRbmTrain rand layeredDbn.Machines.[0] (sinInput |> prependColumnOfOnes)

    let (rows0, Drows0) = (height layeredDbn.Machines.[0].Weights, height layeredDbn.Machines.[0].DWeights)
    let (columns0, Dcolumns0) = (width layeredDbn.Machines.[0].Weights, width layeredDbn.Machines.[0].DWeights)
    let (v0, Dv0) = (layeredDbn.Machines.[0].VisibleBiases, layeredDbn.Machines.[0].DVisibleBiases)
    let (h0, Dh0) = (layeredDbn.Machines.[0].HiddenBiases, layeredDbn.Machines.[0].DHiddenBiases)

    let (rows1, Drows1) = (height layeredDbn.Machines.[1].Weights, height layeredDbn.Machines.[1].DWeights)
    let (columns1, Dcolumns1) = (width layeredDbn.Machines.[1].Weights, width layeredDbn.Machines.[1].DWeights)
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
        cpuDbnTrain rand layeredDbn sinInput |> fun dbn -> dbn.Machines |> List.rev |> List.head |> fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty |> should equal false 

type ``Given a single RBM``()=
    let rand = new Random()
    let inputs = Array2D.init 100 784 (fun i j -> rand.NextDouble() |> float32) |> prependColumnOfOnes
    let sinInput = [|1..784|] |> Array.map (fun x -> (1.0f + sin (12.0f * (x |> float32)/784.0f))/2.0f) |> fun row -> array2D [|row|] |> prependColumnOfOnes

    let rbmParameters =
        {
            LearningRateAlpha = LearningRate 0.5f
            MomentumEta = Momentum 0.9f
            BatchSize = BatchSize 10
            Epochs = Epochs 50
        }
    let rbm = initRbm rbmParameters 784 500 |> fun rbm ->
        {
            Parameters = rbmParameters
            Weights = rbm.Weights
            DWeights = Array2D.init 500 784 (fun _ _ -> rand.NextDouble() |> float32)
            HiddenBiases = Array.init 500 (fun _ -> rand.NextDouble() |> float32)
            DHiddenBiases = Array.init 500 (fun _ -> rand.NextDouble() |> float32)
            VisibleBiases = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
            DVisibleBiases = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
        }
    let weightsAndBiases = toWeightsAndBiases rbm
    let dWeightsAndBiases = toDWeightsAndBiases rbm
    let batch = Array2D.init 10 784 (fun i j -> (i - 5) * (j - 392) |> float32)

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix of the same size.``()=
        activate rand sigmoidFunction batch |> (fun x -> (height x, width x)) |> should equal (10, 784)

    [<Fact>] member test.
        ``The activation function converts a matrix into a matrix containing ones and zeroes.``()=
        activate rand sigmoidFunction batch |> allElementsOfMatrix (fun x -> x * (x - 1.0f) = 0.0f) |> should equal true

    [<Fact>] member test.
        ``The first epoch gives a positive visible error.``()=
        rbmEpoch rand rbm inputs |> fst |> fst |> should greaterThan 0.0f

    [<Fact>] member test.
        ``The first epoch gives a positive hidden error.``()=
        rbmEpoch rand rbm inputs |> fst |> snd |> should greaterThan 0.0f

    [<Fact>] member test.
        ``The first epoch gives an RBM with non-zero weights.``()=
        rbmEpoch rand rbm inputs |> snd |> (fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty) |> should equal false 

    [<Fact>] member test.
        ``The forward iteration of the RBM converts ten samples of length 784 into ten samples of size 500.``()=
         batch |> prependColumnOfOnes |> forward (toWeightsAndBiases rbm) |> (fun x -> (height x, width x)) |> should equal (501, 10)

    [<Fact>] member test.
        ``The backward iteration of the RBM converts ten samples of length 500 into ten samples of length 784.``()=
         batch |> prependColumnOfOnes |> forward (toWeightsAndBiases rbm) |> activate rand sigmoidFunction |> backward (toWeightsAndBiases rbm) |> (fun x -> (height x, width x)) |> should equal (10, 785)

    [<Fact>] member test.
        ``Training 50 epochs of the RBM gives an RBM with non-zero weights.``()=
        cpuRbmTrain rand rbm sinInput |> fun r -> r.Weights |> nonZeroEntries |> Seq.isEmpty |> should equal false 

    [<Fact>] member test.
        ``toRbm reverses the toWeightsAndBiases and toDWeightsAndBiases functions.``()=
        toRbm rbmParameters (toWeightsAndBiases rbm) (toDWeightsAndBiases rbm) |> should equal rbm

    [<Fact>] member test.
        ``toWeightsAndBiases and toDWeightsAndBiases reverse the toRbm function.``()=
        toRbm rbmParameters weightsAndBiases dWeightsAndBiases |> fun r -> (toWeightsAndBiases r, toDWeightsAndBiases r) |> should equal (weightsAndBiases, dWeightsAndBiases)

    [<Fact>] member test.
        ``The toWeightsAndBiases function creates a matrix with one more row and one more column than the Weights matrix.``()=
        (toWeightsAndBiases rbm |> height, toWeightsAndBiases rbm |> width) |> should equal (1 + height rbm.Weights, 1 + width rbm.Weights)

    [<Fact>] member test.
        ``The toDWeightsAndBiases function creates a matrix with one more row and one more column than the DWeights matrix.``()=
        (toWeightsAndBiases rbm |> height, toWeightsAndBiases rbm |> width) |> should equal (1 + height rbm.DWeights, 1 + width rbm.DWeights)
