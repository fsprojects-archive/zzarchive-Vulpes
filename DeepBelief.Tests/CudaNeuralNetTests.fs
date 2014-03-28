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

module CudaNeuralNetTests =

    open CudaCommon
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Xunit
    open Xunit.Extensions
    open FsUnit.Xunit
    open DeepBelief.CudaNeuralNet
    open DeepBelief.CudaTemplates
    open DeepBelief.DeepBeliefNet
    open DeepBelief.Utils
    open DeepBelief.Kernels
    open DeepBelief.NeuralNet
    open TestUtils
    open System

    type ``CUDA Neural Net: Sigmoid``()=
    
        let vector = [|0.1f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.8f; 0.9f|]
        let restrictedVector = [|0.0f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.0f; 0.0f|]
        let logitVector = vector |> Array.map logitFunction

        let sizes = [500; 300; 100; 10]
        let alpha = 0.5f
        let momentum = 0.9f
        let rand = new Random()
        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
        let gpuInputs = [| (xInput |> prependForBias, None) |]
        let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
        let layeredDbn = initDbn sizes xInputs
        let nnetProps = 
            {
                Weights = layeredDbn.Machines |> List.map (fun rbm -> prependColumn rbm.HiddenBiases rbm.Weights);
                Activations = layeredDbn.Machines |> List.map (fun _ -> DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid))
            }
    
        let mod10Plus1 i = 1 + i % 10
        let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
        let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
        let trainingSet = [|1..50|] |> Array.map (fun i -> (sineCurve i, label i))
        let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))

        let outputsMatch result =
            arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

        let levelResultsMatch results =
            List.forall (fun result -> outputsMatch result) results

        let resultsMatch cpu gpu =
            List.zip cpu gpu |> levelResultsMatch

        let cpuRand = new Random()

        let gpuRand = new Random()

        [<Theory>]
        [<InlineData(1)>]
        [<InlineData(2)>]
        [<InlineData(32)>]
        member test.``The sigmoid program maps the logit vector to the original vector.``(i)=
            use sigmoidProgram = i |> sigmoidTemplate |> Compiler.load Worker.Default in
            sigmoidProgram.Run logitVector 0 8 |> should equal vector


    type ``CUDA Neural Net: Feed Forward``()=

        let sizes = [500; 300; 100; 10]
        let alpha = 0.5f
        let momentum = 0.9f
        let rand = new Random()
        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
        let gpuInputs = [| (xInput |> prependForBias, None) |]
        let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
        let layeredDbn = initDbn sizes xInputs
        let nnetProps = 
            {
                Weights = layeredDbn.Machines |> List.map (fun rbm -> prependColumn rbm.HiddenBiases rbm.Weights);
                Activations = layeredDbn.Machines |> List.map (fun _ -> DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid))
            }

        let outputsMatch result =
            arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

        let levelResultsMatch results =
            List.forall (fun result -> outputsMatch result) results

        let resultsMatch cpu gpu =
            List.zip cpu gpu |> levelResultsMatch

        let cpuRand = new Random()

        let gpuRand = new Random()

        [<Theory>]
        [<InlineData(1)>]
        [<InlineData(2)>]
        [<InlineData(32)>]
        let ``The feedForward GPU program matches the outputs of the feedForward CPU function.``(i)=
            use feedForwardProgram = i |> feedForwardTemplate |> Compiler.load Worker.Default in
            resultsMatch (feedForward nnetProps xInput |> List.rev) ((feedForwardProgram.Run nnetProps gpuInputs).[0]) |> should equal true


    type ``CUDA Neural Net: Compute Results``()=

        let sizes = [500; 300; 100; 10]
        let alpha = 0.5f
        let momentum = 0.9f
        let rand = new Random()
        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
        let gpuInputs = [| (xInput |> prependForBias, None) |]
        let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
        let layeredDbn = initDbn sizes xInputs
        let nnetProps = 
            {
                Weights = layeredDbn.Machines |> List.map (fun rbm -> prependColumn rbm.HiddenBiases rbm.Weights);
                Activations = layeredDbn.Machines |> List.map (fun _ -> DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid))
            }
    
        let mod10Plus1 i = 1 + i % 10
        let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
        let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
        let trainingSet = [|1..50|] |> Array.map (fun i -> (sineCurve i, label i))
        let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))

        let outputsMatch result =
            arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

        let levelResultsMatch results =
            List.forall (fun result -> outputsMatch result) results

        let resultsMatch cpu gpu =
            List.zip cpu gpu |> levelResultsMatch

        let cpuRand = new Random()

        let gpuRand = new Random()

        [<Fact>] member test.
            ``The gpuComputeNnetResults function generates the same output as the cpuComputeNnetResults function.``()=
                let cpuOutput = cpuComputeNnetResults nnetProps trainingSet testSet 0.8f 0.25f gpuRand 1 in
                let gpuOutput = gpuComputeNnetResults nnetProps trainingSet testSet 0.8f 0.25f cpuRand 1 in
                cpuOutput |> should equal gpuOutput


    type ``CUDA Neural Net: Error Signals``()=

        let sizes = [500; 300; 100; 10]
        let alpha = 0.5f
        let momentum = 0.9f
        let rand = new Random()
        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
        let gpuInputs = [| (xInput |> prependForBias, None) |]
        let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
        let layeredDbn = initDbn sizes xInputs
        let nnetProps = 
            {
                Weights = layeredDbn.Machines |> List.map (fun rbm -> prependColumn rbm.HiddenBiases rbm.Weights);
                Activations = layeredDbn.Machines |> List.map (fun _ -> DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid))
            }
    
        let mod10Plus1 i = 1 + i % 10
        let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
        let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
        let trainingSet = [|1..50|] |> Array.map (fun i -> (sineCurve i, label i))
        let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))

        [<Theory>]
        [<InlineData(1)>]
        [<InlineData(2)>]
        [<InlineData(32)>]
        member test.``The errorSignals GPU template generates the same output as the CPU errorSignals function.``(i)=
            use feedForwardProgram = 32 |> feedForwardTemplate |> Compiler.load Worker.Default in
            let layerOutputs = (feedForwardProgram.Run nnetProps gpuInputs).[0] |> List.rev in

            use errorSignalsProgram = i |> errorSignalsTemplate |> Compiler.load Worker.Default in
            let gpuOutput = errorSignalsProgram.Run nnetProps.Weights layerOutputs (snd trainingSet.[0]) in
            let cpuOutput = cpuErrorSignals nnetProps.Weights layerOutputs (snd trainingSet.[0])
            for pair in List.zip cpuOutput gpuOutput |> List.rev do
                arraysMatch (fst pair) (snd pair) |> should equal true
