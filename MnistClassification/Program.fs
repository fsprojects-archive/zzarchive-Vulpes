namespace MnistClassification

module Main =

    open System
    open DeepBelief
    open DeepBeliefNet
    open CudaDeepBeliefNet
    open CudaNeuralNet
    open NeuralNet
    open DbnClassification
    open ImageClassification
    open MnistDataLoad
    open Utils

    // Pretraining parameters
    let dbnParameters = 
        {
            Layers = LayerSizes [500; 300; 150; 60; 10]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 200
            Epochs = Epochs 10
        }

    // Fine tuning parameters
    let backPropagationParameters =
        {
            LearningRate = LearningRate 0.8f
            Momentum = Momentum 0.25f
            Epochs = Epochs 10
        }

    [<EntryPoint>]
    let main argv = 

        let mnistTrainingData = loadMnistDataSet TrainingData
        let mnistTestData = loadMnistDataSet TestData

        let rand = new Random()

        let trainedMnistDbn = trainMnistDbn rand dbnParameters mnistTrainingData

        let backPropagationNetwork = toBackPropagationNetwork backPropagationParameters trainedMnistDbn
        let backPropagationResults = gpuComputeNnetResults backPropagationNetwork (toBackPropagationInput mnistTrainingData) (toBackPropagationInput mnistTestData) rand backPropagationParameters
        
        let intResults = backPropagationResults |> Array.map (fun r -> 
            let m = Array.max r
            r |> Array.map (fun e -> if e = m then 1.0f else 0.0f))

        let targets = (toBackPropagationInput mnistTestData) |> Array.map (fun x -> snd x)
        let fpTestError = 
            Array.zip targets backPropagationResults
            |> Array.fold (fun E (x, t) -> 
                let En = error t x
                E + En) 0.0f
        let intTestError = 
            Array.zip targets intResults
            |> Array.fold (fun E (x, t) -> 
                let En = error t x
                E + En) 0.0f

        printfn "%A" (fpTestError / float32 targets.Length)
        printfn "%A" (intTestError / float32 targets.Length)
        0