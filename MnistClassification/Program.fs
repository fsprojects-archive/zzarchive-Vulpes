namespace MnistClassification

module Main =

    open System
    open Backpropagation.CudaSupervisedLearning
    open Backpropagation.Parameters
    open Common.Analytics
    open Common.NeuralNet
    open Common.Utils
    open DeepBelief
    open DeepBeliefNet
    open CudaDeepBeliefNet
    open ImageClassification
    open MnistDataLoad

    // Pretraining parameters
    let dbnParameters = 
        {
            Layers = LayerSizes [500; 300; 150; 60; 10]
            LearningRate = LearningRate 0.8f
            Momentum = Momentum 0.1f
            BatchSize = BatchSize 100
            Epochs = Epochs 2
        }

    // Fine tuning parameters
    let backPropagationParameters =
        {
            BackPropagationParameters.LearningRate = ScaledLearningRate 0.8f
            Momentum = Momentum 0.25f
            Epochs = Epochs 1
        }

    [<EntryPoint>]
    let main argv = 

        let mnistTrainingData = loadMnistDataSet TrainingData
        let mnistTestData = loadMnistDataSet TestData

        let rnd = new RandomSingle(0)

        let trainingSet = mnistTrainingData.ToTrainingSet
        let dbn = DeepBeliefNetwork.Initialise dbnParameters trainingSet
        let trainedDbn = dbn.TrainGpu rnd trainingSet (SampleFrequency 10)

        let backPropagationNetwork = trainedDbn.ToBackPropagationNetwork backPropagationParameters
        let trainedBackPropagationNetwork = backPropagationNetwork.TrainGpu rnd trainingSet (SampleFrequency 100)
        let backPropagationResults = trainedBackPropagationNetwork.ReadTestSetGpu mnistTestData.ToTestSet
        
        let floatingPointOutput (TestOutput testOutput) =
            testOutput |> List.map (fun (Output output) -> 
                let outputValues = output |> Array.map (fun (Signal value) -> value)
                outputValues |> Array.map Signal |> Output)

        let intOutput (TestOutput testOutput) = 
            testOutput |> List.map (fun (Output output) -> 
                let outputValues = output |> Array.map (fun (Signal value) -> value)
                let m = Array.max outputValues
                outputValues |> Array.map (fun e -> (if e = m then 1.0f else 0.0f) |> Signal) |> Output)

        let sumOfSquares v = v |> Array.map (fun element -> element * element) |> Array.sum
        let error (Target target) (Output output) =
            (Array.zip target output |> Array.map (fun (Signal t, Signal o) -> t - o) |> sumOfSquares) / 2.0f

        let targets = match mnistTestData.ToTestSet with TestSet testSet -> testSet |> List.map (fun testCase -> testCase.TestTarget)

        let addError E (x, t) = 
            let En = error x t
            E + En
//        let fpTestError = 
//            List.zip targets (floatingPointOutput backPropagationResults)
//            |> List.fold addError 0.0f
//        let intTestError = 
//            List.zip targets (intOutput backPropagationResults)
//            |> List.fold addError 0.0f
//
//        printfn "%A" (fpTestError / float32 targets.Length)
//        printfn "%A" (intTestError / float32 targets.Length)
        0