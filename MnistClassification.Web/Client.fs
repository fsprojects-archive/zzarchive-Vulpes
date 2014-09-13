namespace MnistClassification.Web

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.Html5

[<JavaScript>]
module Client =

    // Since IE does not support canvas natively. Initialization of the 
    // canvas element is done through the excanvas.js library.
    [<Inline "G_vmlCanvasManager.initElement($elem)">]
    let Initialize (elem: CanvasElement) : unit = ()

    let Start k =
        async {
            let! data = Remoting.LoadMnistDataSet()
            return k data
        }
        |> Async.Start

    let MnistControls () =
        let label = Div [Text ""]
        Div [
            Button [Text "Train MNIST Dataset"]
            |>! OnClick (fun x y ->
                label.Text <- "Fetching training set."
                Start (fun out -> label.Text <- out))
            label
        ]

    type Margin =
        {
            Top    : float
            Right  : float
            Bottom : float
            Left   : float
        }

    let TrainingSet () =
        let margin  = { Top = 10.; Right = 10.; Bottom = 100.; Left = 40. }
        let width   = 800. - margin.Left - margin.Right
        let height  = 500. - margin.Top - margin.Bottom

        let Example (draw: CanvasRenderingContext2D -> unit) width height caption =
            let element = HTML5.Tags.Canvas []
            let canvas  = As<CanvasElement> element.Dom
            // Conditional initialization for the case of IE.
            if (JavaScript.Get "getContext" canvas = JavaScript.Undefined) then
                Initialize canvas
            canvas.Height <- height
            canvas.Width  <- width
            draw (canvas.GetContext "2d")
            Div [Attr.Style "float: left"] -< [
                element
                P [Align "center"] -< [
                    I [Text ("Example " + caption)]
                ]
            ]

        //let context = canvas.Node().TextContent

        Div []
