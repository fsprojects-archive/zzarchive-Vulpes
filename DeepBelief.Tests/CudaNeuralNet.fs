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

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief.CudaTemplates
open DeepBelief.DeepBeliefNet
open DeepBelief.Utils
open DeepBelief.Kernels
open DeepBelief.NeuralNet
open TestUtils
open System

type ``CUDA Neural Net``()=
    
    let vector = [|0.1f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.8f; 0.9f|]
    let restrictedVector = [|0.0f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.0f; 0.0f|]
    let logitVector = vector |> Array.map logitFunction

    let sizes = [500; 300; 10]
    let alpha = 0.5f
    let momentum = 0.9f
    let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
    let gpuInputs = [| (xInput |> prependForBias, None) |]
    let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
    let layeredDbn = initDbn sizes xInputs
    let nnetProps = 
        {
            Weights = layeredDbn.Machines |> List.map (fun rbm -> prependColumn rbm.HiddenBiases rbm.Weights);
            Activations = layeredDbn.Machines |> List.map (fun _ -> (sigmoid, dSigmoid1))
        }

    let sigmoidTemplate (blockSize:int) = cuda {
        let! sigmoidKernel = <@ sigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let sigmoidKernel = program.Apply sigmoidKernel

            fun (vector : Vector) start length -> 

                let size = vector.Length
                let vector = vector |> padToMultipleOf blockSize
                let simpleVectorLp = createSimpleVectorOperationLp blockSize vector.Length

                let vector = worker.Malloc vector

                sigmoidKernel.Launch simpleVectorLp vector.Ptr vector.Ptr start length

                Array.sub (vector.Gather()) 0 8
        ) }

    let sigmoidProgramBlock1 = 1 |> sigmoidTemplate |> Compiler.load Worker.Default
    let sigmoidProgramBlock2 = 2 |> sigmoidTemplate |> Compiler.load Worker.Default
    let sigmoidProgramBlock32 = 32 |> sigmoidTemplate |> Compiler.load Worker.Default

    let feedForwardProgramBlock1 = 1 |> feedForwardTemplate |> Compiler.load Worker.Default
    let feedForwardProgramBlock2 = 2 |> feedForwardTemplate |> Compiler.load Worker.Default
    let feedForwardProgramBlock32 = 32 |> feedForwardTemplate |> Compiler.load Worker.Default

    let cpuFeedForwardOutputs = feedForward nnetProps xInput |> List.rev
    let lastPrependedFeedForwardOutput = (prependForBias (fst cpuFeedForwardOutputs.[0]), prepend 0.0f (snd cpuFeedForwardOutputs.[0]))

    let outputsMatch result =
        arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

    let levelResultsMatch results =
        List.forall (fun result -> outputsMatch result) results

    let resultsMatch cpu gpu =
        List.zip cpu gpu |> levelResultsMatch

    let temp = (feedForwardProgramBlock1.Run nnetProps gpuInputs).[0]

    [<Fact>] member test.
        ``The feedForward block 1 GPU program matches the outputs of the feedForward CPU function.``()=
            resultsMatch cpuFeedForwardOutputs (temp) |> should equal true

    [<Fact>] member test.
        ``The feedForward block 2 GPU program matches the outputs of the feedForward CPU function.``()=
            resultsMatch cpuFeedForwardOutputs ((feedForwardProgramBlock2.Run nnetProps gpuInputs).[0]) |> should equal true

    [<Fact>] member test.
        ``The feedForward block 32 GPU program matches the outputs of the feedForward CPU function.``()=
            resultsMatch cpuFeedForwardOutputs ((feedForwardProgramBlock32.Run nnetProps gpuInputs).[0]) |> should equal true

    [<Fact>] member test.
        ``The sigmoid block 1 program maps the logit vector to the original vector.``()=
            sigmoidProgramBlock1.Run logitVector 0 8 |> should equal vector

    [<Fact>] member test.
        ``The sigmoid block 2 program maps the logit vector to the original vector.``()=
            sigmoidProgramBlock2.Run logitVector 0 8 |> should equal vector

    [<Fact>] member test.
        ``The sigmoid block 32 program maps the logit vector to the original vector.``()=
            sigmoidProgramBlock32.Run logitVector 0 8 |> should equal vector

    [<Fact>] member test.
        ``The sigmoid block 1 program reproduces the restricted vector.``()=
            sigmoidProgramBlock1.Run logitVector 1 5 |> should equal restrictedVector

    [<Fact>] member test.
        ``The sigmoid block 2 program reproduces the restricted vector.``()=
            sigmoidProgramBlock2.Run logitVector 1 5 |> should equal restrictedVector

    [<Fact>] member test.
        ``The sigmoid block 32 program reproduces the restricted vector.``()=
            sigmoidProgramBlock32.Run logitVector 1 5 |> should equal restrictedVector
