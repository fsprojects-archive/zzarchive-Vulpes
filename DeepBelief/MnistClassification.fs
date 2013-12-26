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

    let dbnSizes = [500; 300; 10]
    let alpha = 0.5f
    let momentum = 0.9f

    let mnistDbn = dbn dbnSizes mnistTrainingImages
    let trainedDbn = gpuDbnTrain alpha momentum 500 3 mnistDbn mnistTrainingImages

    let rbmProps = 
        mnistDbn 
        |> List.map (fun rbm -> (prependColumn rbm.HiddenBiases rbm.Weights, sigmoidFunction))
        |> List.unzip |> fun (weights, activations) -> { Weights = weights; Activations = activations }

    let props = { Weights = rbmProps.Weights; Activations = rbmProps.Activations }

    let trainingSet = Array.zip (toArray mnistTrainingImages) mnistTrainingLabels
    let testSet = Array.zip (toArray mnistTestImages) mnistTestLabels
