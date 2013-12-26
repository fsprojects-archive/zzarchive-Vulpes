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

    let sizes = [500; 250; 100; 50]
    let alpha = 0.5f
    let momentum = 0.9f
    let xInputs = Array2D.init 1000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let layeredDbn = dbn sizes xInputs

    let (rows0, Drows0) = (height layeredDbn.[0].Weights, height layeredDbn.[0].DWeights)
    let (columns0, Dcolumns0) = (width layeredDbn.[0].Weights, width layeredDbn.[0].DWeights)
    let (v0, Dv0) = (layeredDbn.[0].VisibleBiases, layeredDbn.[0].DVisibleBiases)
    let (h0, Dh0) = (layeredDbn.[0].HiddenBiases, layeredDbn.[0].DHiddenBiases)

    let (rows1, Drows1) = (height layeredDbn.[1].Weights, height layeredDbn.[1].DWeights)
    let (columns1, Dcolumns1) = (width layeredDbn.[1].Weights, width layeredDbn.[1].DWeights)
    let (v1, Dv1) = (layeredDbn.[1].VisibleBiases, layeredDbn.[1].DVisibleBiases)
    let (h1, Dh1) = (layeredDbn.[1].HiddenBiases, layeredDbn.[1].DHiddenBiases)
    
    [<Fact>] member test.
        ``Training 10 epochs of the DBN gives an RBM with non-zero weights.``()=
        gpuDbnTrain alpha momentum 100 10 layeredDbn xInputs |> List.map (fun r -> r.Weights |> nonZeroEntries |> Array.length |> float32) |> Array.ofList |> allElementsOfVector (fun e -> e > 0.0f) |> should equal true 
