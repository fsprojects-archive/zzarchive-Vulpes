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

    let LoadMnist learningRate momentum batchSize epochs k =
        async {
            let! data = Remoting.TrainMnist(learningRate, momentum, batchSize, epochs)
            return k data
        }
        |> Async.Start

    let MnistControls () =
        let label = Div [Text ""]
        let progress = Div [Text ""]
        let learningRateInput = Input [Attr.Type "number"; Attr.Value "0.9"; Attr.NewAttr "min" "0.0"; Attr.NewAttr "max" "1.0"; Attr.NewAttr "step" "0.01"]
        let momentumInput = Input [Attr.Type "number"; Attr.Value "0.1"; Attr.NewAttr "min" "0.0"; Attr.NewAttr "max" "1.0"; Attr.NewAttr "step" "0.01" ]
        let batchSizeInput = Input [Attr.Type "number"; Attr.Value "100"]
        let epochsInput = Input [Attr.Type "number"; Attr.Value "10"; Attr.NewAttr "min" "1"; Attr.NewAttr "max" "50"]
        Div [
            Div [
                Span [Text "Learning Rate"]
                Span [learningRateInput]
            ]
            Div [
                Span [Text "Momentum"]
                Span [momentumInput]
            ]
            Div [
                Span [Text "Batch Size"]
                Span [batchSizeInput]
            ]
            Div [
                Span [Text "Number of Epochs"]
                Span [epochsInput]
            ]
            Button [Text "Train MNIST Dataset"]
            |>! OnClick (fun x y ->
                label.Text <- "Fetching training set."
                LoadMnist learningRateInput.Value momentumInput.Value batchSizeInput.Value epochsInput.Value (fun out -> 
                    label.Text <- out + " Started unsupervised training."))
            label
            progress
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
