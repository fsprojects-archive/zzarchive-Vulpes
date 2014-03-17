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
namespace DeepBelief

module ImageClassification =

    open System

    type IWrappedType<'T> = 
        abstract Value : 'T

    type ImageWidth = ImageWidth of int with
        interface IWrappedType<int> with
            member this.Value = let (ImageWidth i) = this in i

    and ImageHeight = ImageHeight of int with
        interface IWrappedType<int> with
            member this.Value = let (ImageHeight i) = this in i

    and ImagePixel = ImagePixel of float32 with
        interface IWrappedType<float32> with
            member this.Value = let (ImagePixel x) = this in x

    and ImagePixels = ImagePixels of ImagePixel[,] with
        interface IWrappedType<ImagePixel[,]> with
            member this.Value = let (ImagePixels x) = this in x

    and ImageLabel = ImageLabel of float32[] with
        interface IWrappedType<float32[]> with
            member this.Value = let (ImageLabel x) = this in x

    and ImageData =
        {
            Height : ImageHeight
            Width : ImageWidth
            Pixels : ImagePixels
        }

    and LabelledImage =
        {
            Image : ImageData
            Label : ImageLabel
        }

    let max (x : float32) (y : float32) = Math.Max(x, y)
    let min (x : float32) (y : float32) = Math.Min(x, y)

    let createImagePixel value =
        value |> max 0.0f |> min 0.1f |> ImagePixel

    let createLabel value =
        value |> ImageLabel

    let apply f (s:IWrappedType<'T>) = 
        s.Value |> f 

    let value x = apply id x

    let to2dFloat32Array (imagePixels : ImagePixels) =
        let image = value imagePixels
        let h = Array2D.length1 image
        let w = Array2D.length2 image
        Array2D.init h w (fun i j -> value image.[i, j])