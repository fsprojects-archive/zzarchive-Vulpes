namespace MnistClassification

module DbnClassification =

    open System
    open DeepBelief.NeuralNet
    open DeepBelief.Utils
    open DeepBelief.DeepBeliefNet
    open DeepBelief.CudaDeepBeliefNet
    open DeepBelief.CudaTemplates
    open DeepBelief.ImageClassification
    open MnistDataLoad

    let trainMnistDbn rand (deepBeliefParameters : DeepBeliefParameters) mnistTrainingData = 
        let xInputs = toDbnInputs mnistTrainingData
        let mnistDbn = initDbn deepBeliefParameters xInputs
        gpuDbnTrain rand mnistDbn xInputs
