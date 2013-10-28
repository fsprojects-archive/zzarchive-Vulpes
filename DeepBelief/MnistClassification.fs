namespace DeepBelief

module MnistClassification =

    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic
    open NeuralNet
    open Utils
    open MnistDataLoad
    open DeepBeliefNet

    let mnistTrainingImages = loadMnistImage MnistTrainingImageData
    let mnistTrainingLabels = loadMnistImage MnistTrainingImageData

    let mnistTestImages = loadMnistImage MnistTestImageData
    let mnistTestLabels = loadMnistImage MnistTestLabelData

    let dbnSizes = [500; 250; 100; 50]
    let alpha = 0.5
    let momentum = 0.9

    let mnistDbn = dbn dbnSizes alpha momentum mnistTrainingImages
    let trainedDbn = dbnTrain rand 100 10 mnistDbn mnistTrainingImages

    let rbmProps = 
        mnistDbn 
        |> List.map (fun rbm -> (prependColumn rbm.HiddenBiases rbm.Weights, sigmoid))
        |> List.unzip |> fun (weights, activations) -> { Weights = weights; Activations = activations }

    let props = { Weights = List.concat [|rbmProps.Weights; [initGaussianWeights 50 10]|]; Activations = sigmoid :: rbmProps.Activations }

    let trainingSet = List.zip (toRows mnistTrainingImages) (toRows mnistTrainingLabels) |> Array.ofList
    let testSet = List.zip (toRows mnistTestImages) (toRows mnistTestLabels) |> Array.ofList
