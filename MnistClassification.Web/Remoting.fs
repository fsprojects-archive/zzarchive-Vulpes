namespace MnistClassification.Web

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Sitelets

open System.Web

module Remoting =
    open MnistClassification

    let pageKey page =
        "td" + page

    [<Remote>]
    let TrainingData page =
        async {
            let s = HttpContext.Current.Cache.Get <| pageKey page
            return "You said: " + page
        }
