namespace MnistClassification.Web

open IntelliFactory.Html
open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Sitelets
open IntelliFactory.WebSharper.Sitelets.Http

type Action =
    | Home
    | About
    | TrainingSet
    | TrainingData

module Controls =

    [<Sealed>]
    type EntryPoint() =
        inherit Web.Control()

        [<JavaScript>]
        override __.Body =
            Client.Main() :> _

module Skin =
    open System.Web

    type Page =
        {
            Title : string
            Body : list<Content.HtmlElement>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("body", fun x -> x.Body)

    let WithTemplate title body : Content<Action> =
        Content.WithTemplate MainTemplate <| fun context ->
            {
                Title = title
                Body = body context
            }

module Site =

    let ( => ) text url =
        A [HRef url] -< [Text text]

    let Links (ctx: Context<Action>) =
        UL [
            LI ["Home" => ctx.Link Home]
            LI ["About" => ctx.Link About]
            LI ["Training Set" => ctx.Link TrainingSet]
        ]

    let HomePage =
        Skin.WithTemplate "HomePage" <| fun ctx ->
            [
                Div [Text "HOME"]
                Div [new Controls.EntryPoint()]
                Links ctx
            ]

    let AboutPage =
        Skin.WithTemplate "AboutPage" <| fun ctx ->
            [
                Div [Text "ABOUT"]
                Links ctx
            ]

    let TrainingSetPage =
        Skin.WithTemplate "TrainingSetPage" <| fun ctx ->
            [
                Div [Text "TRAINING SET"]
                Links ctx
            ]

    let TrainingData : Content<Action> =
        CustomContent <| fun context ->
            {
                Status = Http.Status.Ok
                Headers = [Http.Header.Custom "Content-Type" "application/json"]
                WriteBody = fun stream ->
                    use tw = new System.IO.StreamWriter(stream)
                    tw.WriteLine "{X: 10, Y: 20}"
            }

    let Main =
        Sitelet.Sum [
            Sitelet.Content "/" Home HomePage
            Sitelet.Content "/About" About AboutPage
            Sitelet.Content "/TrainingSet" TrainingSet TrainingSetPage
        ]

[<Sealed>]
type Website() =
    interface IWebsite<Action> with
        member this.Sitelet = Site.Main
        member this.Actions = [Home; About; TrainingSet]

type Global() =
    inherit System.Web.HttpApplication()

    member g.Application_Start(sender: obj, args: System.EventArgs) =
        ()

[<assembly: Website(typeof<Website>)>]
do ()
