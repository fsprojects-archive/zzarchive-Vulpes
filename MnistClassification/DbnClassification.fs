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

module DbnClassification =

    open System
    open DeepBelief.NeuralNet
    open DeepBelief.Utils
    open DeepBelief.DeepBeliefNet
    open DeepBelief.CudaDeepBeliefNet
    open DeepBelief.CudaTemplates
    open DeepBelief.ImageClassification
    open MnistDataLoad

    let mnistTrainingImages = loadMnistImage MnistTrainingImageData |> to2dFloat32Array
    let mnistTrainingLabels = loadMnistLabel MnistTrainingLabelData |> Array.map (fun x -> value x)

    let mnistTestImages = loadMnistImage MnistTestImageData |> to2dFloat32Array
    let mnistTestLabels = loadMnistLabel MnistTestLabelData |> Array.map (fun x -> value x)

    let trainedMnistDbn dbnSizes dbnAlpha dbnMomentum batchSize epochs = 
        let mnistDbn sizes = initDbn sizes mnistTrainingImages
        let rand = new Random()
        gpuDbnTrain dbnAlpha dbnMomentum batchSize epochs rand (mnistDbn dbnSizes) mnistTrainingImages

    let mnistRbmProps dbnSizes dbnAlpha dbnMomentum batchSize epochs = 
        trainedMnistDbn dbnSizes dbnAlpha dbnMomentum batchSize epochs
        |> fun dbn -> dbn.Machines
        |> List.map (fun rbm -> (prependColumn rbm.HiddenBiases rbm.Weights, DifferentiableFunction (FloatingPointFunction sigmoidFunction, FloatingPointDerivative sigmoidDerivative)))
        |> List.unzip |> fun (weights, activations) -> { Weights = weights; Activations = activations }

    let mnistTrainingSet = Array.zip (toArray mnistTrainingImages) mnistTrainingLabels
    let mnistTestSet = Array.zip (toArray mnistTestImages) mnistTestLabels
