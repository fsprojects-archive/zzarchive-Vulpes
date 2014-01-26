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
