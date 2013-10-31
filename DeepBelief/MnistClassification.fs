namespace DeepBelief

module MnistClassification =

    open Alea.CUDA
    open Alea.CUDA.Utilities
    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic
    open NeuralNet
    open Utils
    open MnistDataLoad
    open DeepBeliefNet
    open CudaTemplates

    let mnistTrainingImages = loadMnistImage MnistTrainingImageData |> SparseMatrix.ofArray2
    let mnistTrainingLabels = loadMnistLabel MnistTrainingLabelData

    let mnistTestImages = loadMnistImage MnistTestImageData |> DenseMatrix.ofArray2
    let mnistTestLabels = loadMnistLabel MnistTestLabelData

    let dbnSizes = [500; 250; 100; 50]
    let alpha = 0.5
    let momentum = 0.9

    let mnistDbn = dbn dbnSizes alpha momentum mnistTrainingImages
    //let trainedDbn = dbnTrain rand 100 10 mnistDbn mnistTrainingImages
    let trainedDbn = dbnTrain rand 100 2 mnistDbn mnistTrainingImages

    let rbmProps = 
        mnistDbn 
        |> List.map (fun rbm -> (prependColumn rbm.HiddenBiases rbm.Weights, sigmoid))
        |> List.unzip |> fun (weights, activations) -> { Weights = weights; Activations = activations }

    let props = { Weights = List.concat [|rbmProps.Weights; [initGaussianWeights 50 10]|]; Activations = sigmoid :: rbmProps.Activations }

    let trainingSet = List.zip (toRows mnistTrainingImages) (toRows mnistTrainingLabels) |> Array.ofList
    let testSet = List.zip (toRows mnistTestImages) (toRows mnistTestLabels) |> Array.ofList
