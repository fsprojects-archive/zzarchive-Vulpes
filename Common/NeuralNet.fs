namespace Common

module NeuralNet = 
        
    type LayerSizes = LayerSizes of int list

    and ScaledLearningRate = ScaledLearningRate of float32

    and LearningRate = LearningRate of float32 with
        static member (/) (LearningRate lr, rhs : int) =
            ScaledLearningRate (lr / (float32 rhs))

    and Momentum = Momentum of float32
    
    and BatchSize = BatchSize of int

    and Signal = Signal of float32

    and Output = Output of Signal[]

    and Target = Target of Signal[]

    and Input = Input of Signal[] with
        member this.Size = match this with Input signals -> signals.Length

    and LayerInputs = LayerInputs of Input list

    and TrainingExample = { TrainingInput : Input; TrainingTarget : Target } 

    and TrainingSet = TrainingSet of TrainingExample list

    and TestCase = { TestInput : Input; TestTarget : Target }

    and TestSet = TestSet of TestCase list with
        member this.Targets = match this with TestSet testSet -> testSet |> List.map (fun testCase -> testCase.TestTarget)

    and Epochs = Epochs of int with
        member this.NumberOfSteps (TrainingSet trainingSet) = match this with Epochs epochs -> epochs * trainingSet.Length

    and TestOutput = TestOutput of Output list

