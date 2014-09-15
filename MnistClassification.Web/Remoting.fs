namespace MnistClassification.Web

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5
open IntelliFactory.WebSharper.Sitelets

open System.Web

module Remoting =
    open Common.NeuralNet
    open Common.Utils
    open DeepBelief.DeepBeliefNet
    open DeepBelief.CudaDeepBeliefNet
    open DeepBelief.ImageClassification
    open MnistClassification.MnistDataLoad
    open System.Net.Sockets
    open System.Net
    open System.Collections.Concurrent
    open System

    let pageKey page = "td" + page
    let readers = dict [ ("mnist-classification", fun () -> loadMnistDataSet TrainingData) ]
    let imageSets = new ConcurrentDictionary<string, Lazy<ImageSet>>()

    let readImageSet key =
        let lazyImageSet = imageSets.GetOrAdd(key, (fun k -> new Lazy<ImageSet>(readers.[k])))
        lazyImageSet.Value

    type SampleError = {
        EpochIndex : int
        BatchIndex : int
        Time : int
        Value : float32
    }

    let sampleErrors = new ConcurrentQueue<SampleError>()

    let TrainDbn layerSizes learningRate momentum batchSize epochs =
        async {
            let dbnParameters = 
                {
                    Layers = LayerSizes layerSizes
                    LearningRate = LearningRate learningRate
                    Momentum = Momentum momentum
                    BatchSize = BatchSize batchSize
                    Epochs = Epochs epochs
                }
            let mnist = readImageSet "mnist-classification"
            let rnd = new RandomSingle(0)
            let trainingSet = mnist.ToTrainingSet
            let dbn = DeepBeliefNetwork.Initialise dbnParameters trainingSet
            dbn.TrainGpu rnd trainingSet (SampleFrequency 50) (fun errorReport -> errorReport |> ignore) |> ignore
        }

    [<Remote>]
    let TrainMnist(learningRate, momentum, batchSize, epochs) =
        async {
            let layerSizes = [500; 300; 150; 60; 10]
            let learningRate = System.Single.Parse learningRate
            let momentum = System.Single.Parse momentum
            let batchSize = System.Int32.Parse batchSize
            let epochs = System.Int32.Parse epochs
            TrainDbn layerSizes learningRate momentum batchSize epochs |> Async.Start 
            return "Unsupervised training started."
        }

    [<Rpc>]
    let Poll (time: int) =
        async {
            return [| for se in sampleErrors do if se.Time > time then yield se |]
        }
