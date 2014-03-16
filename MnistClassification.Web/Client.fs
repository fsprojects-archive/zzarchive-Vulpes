// The MIT License (MIT)
// 
// Copyright (c) 2014 SpiegelSoft Ltd
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace MnistClassification.Web

open IntelliFactory.WebSharper
open IntelliFactory.WebSharper.Html
open IntelliFactory.WebSharper.D3

[<JavaScript>]
module Client =

    let Start input k =
        async {
            let! data = Remoting.Process(input)
            return k data
        }
        |> Async.Start

    let Main () =

        let input = Input [Text ""]
        let label = Div [Text ""]
        Div [
            input
            label
            Button [Text "Click"]
            |>! OnClick (fun _ _ ->
                Start input.Value (fun out ->
                    label.Text <- out))
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

        let svg = 
            D3.Select("body").Append("svg")
                .Attr("width", width + margin.Left + margin.Right)
                .Attr("height", height + margin.Top + margin.Bottom)

        Div []
