namespace DeepBelief

module Classification =

    open NeuralNet
    open Utils
    open MnistDataLoad
    open DeepBeliefNet

    let mnistImages = loadMnistImage MnistTrainingImageData

    let dbnSizes = [500; 250; 100; 50]
    let alpha = 0.5
    let momentum = 0.9

    let mnistDbn = dbn dbnSizes alpha momentum mnistImages

    let props = 
        mnistDbn 
        |> List.map (fun rbm -> (prependColumn rbm.HiddenBiases rbm.Weights, sigmoid))
        |> List.unzip |> fun (weights, activations) -> { Weights = weights; Activations = activations }



