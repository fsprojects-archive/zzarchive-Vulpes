namespace Backpropagation

module Parameters =

    open Common.NeuralNet
    open Common.Analytics

    type InputSignal = InputSignal of Signals

    type OutputLabel = OutputLabel of Signals

    type TrainingData = TrainingData of (InputSignal * OutputLabel)[]

    /// precision for calculating the derivatives
    let prc = 1e-6f

    type BackPropagationLayer = {
        Weight : Matrix
        Activation : DifferentiableFunction
    }

    type BackPropagationLayer with
        member this.FeedForward(Vector values) =
            values |> Array.map (fun v -> Domain v |> this.Activation.Evaluate)

    type BackPropagationParameters = {
        LearningRate : LearningRate
        Momentum : Momentum
        Epochs : Epochs
    }

    type BackPropagationNetwork = {
        Parameters : BackPropagationParameters
        Layers : BackPropagationLayer list
    }
