namespace Common

module NeuralNet = 
        
    type LayerSizes = LayerSizes of int list

    and ScaledLearningRate = ScaledLearningRate of float32

    and LearningRate = LearningRate of float32 with
        static member (/) (LearningRate lr, rhs : int) =
            ScaledLearningRate (lr / (float32 rhs))

    and Momentum = Momentum of float32
    
    and BatchSize = BatchSize of int

    and Epochs = Epochs of int
