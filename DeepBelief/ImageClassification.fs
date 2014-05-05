namespace DeepBelief

module ImageClassification =

    open System
    open Utils

    type ImageWidth = ImageWidth of int

    and ImageHeight = ImageHeight of int

    and ImagePixel = ImagePixel of float32

    and ImagePixels = ImagePixels of ImagePixel[,]

    and ImageLabel = ImageLabel of float32[]

    and LabelSize = LabelSize of int

    and LabelledImage =
        {
            Image : ImagePixels
            Label : ImageLabel
        }

    and ImageSet =
        {
            Height : ImageHeight
            Width : ImageWidth
            LabelDimension : LabelSize
            Images : LabelledImage[]
        }

    and DataSetType = TrainingData | TestData

    let max (x : float32) (y : float32) = Math.Max(x, y)
    let min (x : float32) (y : float32) = Math.Min(x, y)

    let createImagePixel value =
        value |> max 0.0f |> min 1.0f |> ImagePixel

    let createLabel value =
        value |> ImageLabel

    let toDbnInputs (imageSet : ImageSet) =
        let toDbnInput height width (labelledImage : LabelledImage) =
            let to2DArray (ImageHeight height) (ImageWidth width) (ImagePixels image) =
                let pixelValue (ImagePixel pValue) = pValue
                Array2D.init height width (fun i j -> pixelValue image.[i, j])
            let pixels = labelledImage.Image
            to2DArray height width pixels |> toArray
        let imageDimension (ImageHeight height) (ImageWidth width) = height * width
        let array = imageSet.Images |> Array.fold (fun acc element -> Array.concat [acc; toDbnInput imageSet.Height imageSet.Width element]) [||]
        Array2D.init (Array.length imageSet.Images) (imageDimension imageSet.Height imageSet.Width) (fun i j -> array.[i].[j]) 

    let toBackPropagationInput (imageSet : ImageSet) =
        let pixelValue (ImagePixel pValue) = pValue
        let toLabelArray (ImageLabel label) = label
        let flattenImage (ImageHeight height) (ImageWidth width) (ImagePixels pixels) =
            Array.init (height * width) (fun i -> pixels.[i / width, i % width] |> pixelValue)
        imageSet.Images |> Array.map (fun labelledImage -> (flattenImage imageSet.Height imageSet.Width labelledImage.Image, toLabelArray labelledImage.Label))
