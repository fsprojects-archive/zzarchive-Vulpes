namespace DeepBelief

module MnistDataLoad =

    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic
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

    let readImage (b : BinaryReader, rowArray, colArray) =
        rowArray |> Array.collect (fun r -> Array.map (fun c -> (b.ReadByte() |> int |> float)/255.0 ) colArray)

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

        let rowArray = [|1..nRows|]
        let colArray = [|1..nCols|]
        let images = [1..nImages] |> List.map (fun i -> readImage(reader, rowArray, colArray))
        DenseMatrix.ofRows nImages (nRows * nCols) images;

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
        [1..nLabels] |> List.map (fun i -> int (reader.ReadByte()))
