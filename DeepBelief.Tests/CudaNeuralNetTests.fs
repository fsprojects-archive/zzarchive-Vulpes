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
open DeepBelief.CudaNeuralNet
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

    let feedForwardTemplate (blockSize:int) = cuda {
        let! multiplyVectorByMatrixAndTransformTwiceKernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize <@ sigmoid @> <@ dSigmoid @> |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByMatrixAndTransformTwiceKernel = program.Apply multiplyVectorByMatrixAndTransformTwiceKernel
            let coerceKernel = program.Apply coerceKernel

            fun (netProps : NnetProperties) data -> 
                let paddedWeights = netProps.Weights |> List.map (prependRowOfZeroes >> padToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize (height w) (width w))
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (height w))

                let inputs0 = worker.Malloc<float32>(width paddedWeights.[0])
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))

                // The contents of these lists will need to be disposed at the end of the run.
                let weights = paddedWeights |> List.map (flattenMatrix >> worker.Malloc)
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))

                let mutable result = []
                let N = weights.Length - 1
                for i in 0..Array.length data - 1 do
                    inputs0.Scatter(fst data.[i] |> padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch coerceLp lastOutput.Ptr 0 1.0f
                        multiplyVectorByMatrixAndTransformTwiceKernel.Launch forwardLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (height paddedWeights.[j]) (width paddedWeights.[j])

                    let zippedOutputs = List.zip outputs dOutputs
                    let gatheredOutputs = zippedOutputs |> List.mapi (fun iw (output, dOutput) -> (Array.sub (output.Gather()) 1 (height netProps.Weights.[iw]), Array.sub (dOutput.Gather()) 1 (height netProps.Weights.[iw])))
                    result <- gatheredOutputs :: result

                disposeAll [|weights; dOutputs|]
                result
       ) }

    let errorSignalsTemplate (blockSize:int) = cuda {

        return Entry(fun program ->
            fun Ws layeroutputs (target : Vector) ->
                let N = List.length Ws - 1
                0.0
       ) }

    let sigmoidProgramBlock1 = 1 |> sigmoidTemplate |> Compiler.load Worker.Default
    let sigmoidProgramBlock2 = 2 |> sigmoidTemplate |> Compiler.load Worker.Default
    let sigmoidProgramBlock32 = 32 |> sigmoidTemplate |> Compiler.load Worker.Default

    let feedForwardProgramBlock1 = 1 |> feedForwardTemplate |> Compiler.load Worker.Default
    let feedForwardProgramBlock2 = 2 |> feedForwardTemplate |> Compiler.load Worker.Default
    let feedForwardProgramBlock32 = 32 |> feedForwardTemplate |> Compiler.load Worker.Default

    let outputsMatch result =
        arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

    let levelResultsMatch results =
        List.forall (fun result -> outputsMatch result) results

    let resultsMatch cpu gpu =
        List.zip cpu gpu |> levelResultsMatch

    let cpuRand = new Random()

    let gpuRand = new Random()

    [<Fact>] member test.
        ``The feedForward block 1 GPU program matches the outputs of the feedForward CPU function.``()=
            resultsMatch (feedForward nnetProps xInput |> List.rev) ((feedForwardProgramBlock1.Run nnetProps gpuInputs).[0]) |> should equal true

    [<Fact>] member test.
        ``The feedForward block 2 GPU program matches the outputs of the feedForward CPU function.``()=
            resultsMatch (feedForward nnetProps xInput |> List.rev) ((feedForwardProgramBlock2.Run nnetProps gpuInputs).[0]) |> should equal true

    [<Fact>] member test.
        ``The feedForward block 32 GPU program matches the outputs of the feedForward CPU function.``()=
            resultsMatch (feedForward nnetProps xInput |> List.rev) ((feedForwardProgramBlock32.Run nnetProps gpuInputs).[0]) |> should equal true

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

    // This is not testing anything in the app; 
    // just making sure that the inputs to the CPU and GPU neural nets are the same.
    [<Fact>] member test.
        ``The CPU and GPU random inputs match.``()=
            [1..100] |> List.map (fun _ -> cpuRand.Next(200)) |> should equal ([1..100] |> List.map (fun _ -> gpuRand.Next(200)))

    [<Fact>] member test.
        ``The gpuComputeNnetResults function generates the same output as the cpuComputeNnetResults function.``()=
            cpuComputeNnetResults nnetProps trainingSet testSet 0.8f 0.25f gpuRand 1 
            |> should equal (gpuComputeNnetResults nnetProps trainingSet testSet 0.8f 0.25f cpuRand 1)