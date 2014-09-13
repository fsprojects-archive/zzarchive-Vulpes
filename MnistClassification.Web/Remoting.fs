namespace MnistClassification.Web

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Sitelets

open System.Web

module Remoting =
    open DeepBelief.ImageClassification
    open MnistClassification.MnistDataLoad
    open System.Collections.Concurrent
    open System

    let pageKey page = "td" + page
    let readers = dict [ ("mnist-classification", fun () -> loadMnistDataSet TrainingData) ]
    let imageSets = new ConcurrentDictionary<string, Lazy<ImageSet>>()

    let readImageSet key =
        let lazyImageSet = imageSets.GetOrAdd(key, (fun k -> new Lazy<ImageSet>(readers.[k])))
        lazyImageSet.Value

    let getCachedImageSets() =
        let imageSets = new ConcurrentDictionary<string, Lazy<ImageSet>>()
        HttpContext.Current.Cache.Insert("imagesets", imageSets)
        imageSets

    [<Remote>]
    let LoadMnistDataSet() =
        async {
            let mnist = readImageSet "mnist-classification"
            return "MNIST dataset loaded."
        }
