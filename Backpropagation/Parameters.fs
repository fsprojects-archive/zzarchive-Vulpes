namespace Backpropagation

module Parameters =

    open Common.NeuralNet
    open Common.Analytics

    type BackPropagationLayer = {
        Weights : WeightsAndBiases
        Activation : DifferentiableFunction
    }

    type BackPropagationParameters = {
        LearningRate : ScaledLearningRate
        Momentum : Momentum
        Epochs : Epochs
    }

    type BackPropagationNetwork = {
        Parameters : BackPropagationParameters
        Layers : BackPropagationLayer list
    }
