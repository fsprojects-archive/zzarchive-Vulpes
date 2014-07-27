namespace DeepBelief

module ImageClassification =

    open System
    open Common.Analytics
    open Common.NeuralNet
    open Common.Utils
    open Utils

    type ImageWidth = ImageWidth of int

    and ImageHeight = ImageHeight of int

    and ImagePixel = ImagePixel of float32

    and ImagePixels = ImagePixels of ImagePixel[,]

    and ImageLabel = ImageLabel of ImagePixel[]

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

    type LabelledImage with
        member this.ToTrainingExample =
            let imageMatrix = match this.Image with ImagePixels pixels -> pixels |> Array2D.map (fun (ImagePixel pixel) -> pixel) |> Matrix
            let label = match this.Label with ImageLabel imageLabel -> imageLabel
            {
                TrainingInput = imageMatrix.ToRowMajorFormat |> Array.map (fun value -> Signal value) |> Input;
                TrainingTarget = label |> Array.map (fun (ImagePixel pixel) -> Signal pixel) |> Target
            }
        member this.ToTestCase =
            let imageMatrix = match this.Image with ImagePixels pixels -> pixels |> Array2D.map (fun (ImagePixel pixel) -> pixel) |> Matrix
            let label = match this.Label with ImageLabel imageLabel -> imageLabel
            {
                TestInput = imageMatrix.ToRowMajorFormat |> Array.map (fun value -> Signal value) |> Input;
                TestTarget = label |> Array.map (fun (ImagePixel pixel) -> Signal pixel) |> Target
            }

    let max (x : float32) (y : float32) = Math.Max(x, y)
    let min (x : float32) (y : float32) = Math.Min(x, y)

    let createImagePixel value =
        value |> max 0.0f |> min 1.0f |> ImagePixel

    let createLabel array =
        array |> Array.map (fun value -> createImagePixel value) |> ImageLabel

    type ImageSet with
        member imageSet.ToTrainingSet =
            imageSet.Images |> Array.map (fun image -> image.ToTrainingExample) |> List.ofArray |> TrainingSet
        member imageSet.ToTestSet =
            imageSet.Images |> Array.map (fun image -> image.ToTestCase) |> List.ofArray |> TestSet
