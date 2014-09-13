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
    let (|?) lhs rhs = (if lhs = null then rhs else lhs)

    let readImageSet (imageSets : ConcurrentDictionary<string, Lazy<ImageSet>>) key =
        let lazyImageSet = imageSets.GetOrAdd(key, (fun k -> new Lazy<ImageSet>(readers.[k])))
        lazyImageSet.Value

    let getCachedImageSets() =
        let imageSets = new ConcurrentDictionary<string, Lazy<ImageSet>>()
        HttpContext.Current.Cache.Insert("imagesets", imageSets)
        imageSets

    [<Remote>]
    let TrainingData page =
        async {
            let imageSets = HttpContext.Current.Cache.Get <| "imagesets" :?> ConcurrentDictionary<string, Lazy<ImageSet>> |? getCachedImageSets()
            let mnist = readImageSet imageSets "mnist-classification"
            return "You said: " + page
        }
