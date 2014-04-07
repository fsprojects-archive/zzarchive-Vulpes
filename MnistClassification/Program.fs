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
namespace MnistClassification

module Main =

    open System
    open DeepBelief
    open DeepBeliefNet
    open CudaDeepBeliefNet
    open CudaNeuralNet
    open NeuralNet
    open DbnClassification
    open Utils

    let dbnParameters = 
        {
            Layers = LayerSizes [500; 300; 150; 60; 10]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 30
            Epochs = Epochs 10
        }

    let backPropagationParameters =
        {
            LearningRate = LearningRate 0.8f
            Momentum = Momentum 0.25f
            Epochs = Epochs 10
        }

    [<EntryPoint>]
    let main argv = 
        let rand = new Random()
        let network = mnistNetwork rand backPropagationParameters dbnParameters
        let fpResults = gpuComputeNnetResults network mnistTrainingSet mnistTestSet rand backPropagationParameters
        let intResults = fpResults |> Array.map (fun r -> 
            let m = Array.max r
            r |> Array.map (fun e -> if e = m then 1.0f else 0.0f))

        let targets = mnistTestSet |> Array.map (fun x -> snd x)

        let fpTestError = 
            Array.zip targets fpResults
            |> Array.fold (fun E (x, t) -> 
                let En = error t x
                E + En) 0.0f

        let intTestError = 
            Array.zip targets intResults
            |> Array.fold (fun E (x, t) -> 
                let En = error t x
                E + En) 0.0f

        printfn "%A" (fpTestError / float32 targets.Length)
        printfn "%A" (intTestError / float32 targets.Length)
        0