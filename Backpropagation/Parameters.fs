namespace Backpropagation

module Parameters =

    type InputSignal = InputSignal of float32[]

    type OutputLabel = OutputLabel of float32[]

    type TrainingData = TrainingData of (InputSignal * OutputLabel)[]
