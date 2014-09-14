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

    [<Remote>]
    let LoadMnistDataSet() =
        async {
            let mnist = readImageSet "mnist-classification"
            return "MNIST dataset loaded."
        }

//    [<Rpc>]
//    let Poll (time: int) =
//        let s = State.Get()
//        s.Cleanup ()
//        lock State.Lock (
//            fun () ->
//                s.Users.[auth.Name] <- DateTime.Now
//        )
//        let m =
//            [|
//                for m in s.Messages do
//                    if m.Time > time then
//                        yield m
//            |]
//        let u = [| for u in s.Users -> u.Key |]
//        async { return (!s.Time, m, u) }

    [<Remote>]
    let TrainMnistUnsupervised layerSizes learningRate momentum batchSize epochs =
        let dbnParameters = 
            {
                Layers = LayerSizes layerSizes
                LearningRate = LearningRate learningRate
                Momentum = Momentum momentum
                BatchSize = BatchSize batchSize
                Epochs = Epochs epochs
            }
        async {
            let mnist = readImageSet "mnist-classification"
            let rnd = new RandomSingle(0)
            let trainingSet = mnist.ToTrainingSet
            let ws = WebSocket("ws://localhost:9998/updateweights")
            // ws.Send "Hello"
            return "Unsupervised training started."
        }
