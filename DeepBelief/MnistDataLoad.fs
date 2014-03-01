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

module MnistDataLoad =

    open Microsoft.FSharp.Collections
    open System
    open System.IO

    [<Literal>]
    let MnistTrainingImageData = @"train-images.idx3-ubyte"

    [<Literal>]
    let MnistTestImageData = @"t10k-images.idx3-ubyte"

    [<Literal>]
    let MnistTrainingLabelData = @"train-labels.idx1-ubyte"

    [<Literal>]
    let MnistTestLabelData = @"t10k-labels.idx1-ubyte"

    let readInt (b : BinaryReader) =
        [1..4] |> List.fold (fun res item -> (res <<< 8) ||| (int)(b.ReadByte())) 0

    let readImage (b : BinaryReader) =
        (b.ReadByte() |> int |> float32)/255.0f

    let readLabel (b : BinaryReader) =
        let digit = int (b.ReadByte())
        [|0..9|] |> Array.map(fun i -> if i = digit then 1.0f else 0.0f)

    // TRAINING SET IMAGE FILE (train-images-idx3-ubyte):
    // [offset] [type]          [value]          [description] 
    // 0000     32 bit integer  0x00000803(2051) magic number 
    // 0004     32 bit integer  60000            number of images 
    // 0008     32 bit integer  28               number of rows 
    // 0012     32 bit integer  28               number of columns 
    // 0016     unsigned byte   ??               pixel 
    // 0017     unsigned byte   ??               pixel 
    // ........ 
    // xxxx     unsigned byte   ??               pixel

    // TEST SET IMAGE FILE (t10k-images-idx3-ubyte):
    // [offset] [type]          [value]          [description] 
    // 0000     32 bit integer  0x00000803(2051) magic number 
    // 0004     32 bit integer  10000            number of images 
    // 0008     32 bit integer  28               number of rows 
    // 0012     32 bit integer  28               number of columns 
    // 0016     unsigned byte   ??               pixel 
    // 0017     unsigned byte   ??               pixel 
    // ........ 
    // xxxx     unsigned byte   ??               pixel
    // Pixels are organized row-wise. Pixel values are 0 to 255. 0 means background (white), 255 means foreground (black).
    let loadMnistImage file =
        use stream = File.Open(file, FileMode.Open)
        use reader = new BinaryReader(stream)
        let magicNumber = readInt(reader)
        let nImages = readInt(reader)
        let nRows = readInt(reader)
        let nCols = readInt(reader)
        Array2D.init nImages (nRows * nCols) (fun _ _ -> readImage reader) |> Utils.prependColumnOfOnes

    // TRAINING SET LABEL FILE (train-labels-idx1-ubyte):
    // [offset] [type]          [value]          [description] 
    // 0000     32 bit integer  0x00000801(2049) magic number (MSB first) 
    // 0004     32 bit integer  60000            number of items 
    // 0008     unsigned byte   ??               label 
    // 0009     unsigned byte   ??               label 
    // ........ 
    // xxxx     unsigned byte   ??               label

    // TEST SET LABEL FILE (t10k-labels-idx1-ubyte):
    // [offset] [type]          [value]          [description] 
    // 0000     32 bit integer  0x00000801(2049) magic number (MSB first) 
    // 0004     32 bit integer  10000            number of items 
    // 0008     unsigned byte   ??               label 
    // 0009     unsigned byte   ??               label 
    // ........ 
    // xxxx     unsigned byte   ??               label
    // The labels' values are 0 to 9.
    let loadMnistLabel file =
        use stream = File.Open(file, FileMode.Open)
        use reader = new BinaryReader(stream)
        let magicNumber = readInt(reader)
        let nLabels = readInt(reader)
        [|1..nLabels|] |> Array.map (fun _ -> readLabel reader)
