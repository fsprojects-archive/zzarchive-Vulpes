namespace Backpropagation

module Parameters =

    open Common.NeuralNet
    open Common.Analytics

    type InputSignal = InputSignal of Signals

    type OutputLabel = OutputLabel of Signals

    type TrainingData = TrainingData of (InputSignal * OutputLabel)[]

    type BackPropagationLayer = {
        Weights : Matrix
        Activation : DifferentiableFunction
    }

    type BackPropagationParameters = {
        LearningRate : LearningRate
        Momentum : Momentum
        Epochs : Epochs
    }

    type BackPropagationNetwork = {
        Parameters : BackPropagationParameters
        Layers : BackPropagationLayer list
    }
