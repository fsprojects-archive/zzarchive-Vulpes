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
open CudaDeepBeliefNet
open System
open Utils
open TestUtils

type ``Deep Belief Network with four layers and 1000 samples running on GPU`` ()=
    let sin x = Math.Sin (float x) |> float32

    let rand = new Random()
    let xInputs = Array2D.init 1000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let dbnParameters = 
        {
            Layers = LayerSizes [500; 250; 100; 50]
            LearningRateAlpha = LearningRate 0.5f
            MomentumEta = Momentum 0.9f
            BatchSize = BatchSize 10
            Epochs = Epochs 2
        }
    let layeredDbn = initDbn dbnParameters xInputs

    let (rows0, Drows0) = (height layeredDbn.Machines.[0].Weights, height layeredDbn.Machines.[0].DWeights)
    let (columns0, Dcolumns0) = (width layeredDbn.Machines.[0].Weights, width layeredDbn.Machines.[0].DWeights)
    let (v0, Dv0) = (layeredDbn.Machines.[0].VisibleBiases, layeredDbn.Machines.[0].DVisibleBiases)
    let (h0, Dh0) = (layeredDbn.Machines.[0].HiddenBiases, layeredDbn.Machines.[0].DHiddenBiases)

    let (rows1, Drows1) = (height layeredDbn.Machines.[1].Weights, height layeredDbn.Machines.[1].DWeights)
    let (columns1, Dcolumns1) = (width layeredDbn.Machines.[1].Weights, width layeredDbn.Machines.[1].DWeights)
    let (v1, Dv1) = (layeredDbn.Machines.[1].VisibleBiases, layeredDbn.Machines.[1].DVisibleBiases)
    let (h1, Dh1) = (layeredDbn.Machines.[1].HiddenBiases, layeredDbn.Machines.[1].DHiddenBiases)
    
    [<Fact>] member test.
        ``Training 10 epochs of the DBN gives an RBM with non-zero weights.``()=
        gpuDbnTrain rand layeredDbn xInputs |> fun dbn -> dbn.Machines |> List.map (fun r -> r.Weights |> nonZeroEntries |> Array.length |> float32) |> Array.ofList |> allElementsOfVector (fun e -> e > 0.0f) |> should equal true 
