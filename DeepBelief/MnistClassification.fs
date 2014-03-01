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
namespace DeepBelief

module MnistClassification =

    open Alea.CUDA
    open Alea.CUDA.Utilities
    open NeuralNet
    open Utils
    open MnistDataLoad
    open DeepBeliefNet
    open CudaDeepBeliefNet
    open CudaTemplates

    let mnistTrainingImages = loadMnistImage MnistTrainingImageData
    let mnistTrainingLabels = loadMnistLabel MnistTrainingLabelData

    let mnistTestImages = loadMnistImage MnistTestImageData
    let mnistTestLabels = loadMnistLabel MnistTestLabelData

    let mnistDbn dbnSizes = dbn dbnSizes mnistTrainingImages
    let trainedDbn dbnSizes dbnAlpha dbnMomentum batchSize epochs = gpuDbnTrain dbnAlpha dbnMomentum batchSize epochs (mnistDbn dbnSizes) mnistTrainingImages

    let rbmProps dbnSizes dbnAlpha dbnMomentum batchSize epochs = 
        trainedDbn dbnSizes dbnAlpha dbnMomentum batchSize epochs
        |> List.map (fun rbm -> (prependColumn rbm.HiddenBiases rbm.Weights, (sigmoidFunction, sigmoidDerivative)))
        |> List.unzip |> fun (weights, activations) -> { Weights = weights; Activations = activations }

    let props dbnSizes dbnAlpha dbnMomentum batchSize epochs =
        let dbnOutput = rbmProps dbnSizes dbnAlpha dbnMomentum batchSize epochs
        { Weights = dbnOutput.Weights; Activations = dbnOutput.Activations }

    let trainingSet = Array.zip (toArray mnistTrainingImages) mnistTrainingLabels
    let testSet = Array.zip (toArray mnistTestImages) mnistTestLabels
