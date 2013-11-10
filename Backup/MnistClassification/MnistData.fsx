#r "../packages/MathNet.Numerics.2.6.1/lib/net40/MathNet.Numerics.dll"
#r "../packages/MathNet.Numerics.FSharp.2.6.0/lib/net40/MathNet.Numerics.FSharp.dll"

#load "MnistDataLoad.fs"

open DeepBelief
open MnistDataLoad
open System.IO

Directory.SetCurrentDirectory(@"C:\Users\white_000\Documents\GitHub\Vulpes\MnistClassification")

let testImages = loadMnistImage MnistTestImageData
let trainingImages = loadMnistImage MnistTrainingImageData

printfn "testImages is a %d by %d matrix." testImages.RowCount testImages.ColumnCount
printfn "trainingImages is a %d by %d matrix." trainingImages.RowCount trainingImages.ColumnCount

let testLabels = loadMnistLabel MnistTestLabelData
let trainingLabels = loadMnistLabel MnistTrainingLabelData

printfn "testLabels is a %d by %d matrix." testLabels.RowCount testLabels.ColumnCount
printfn "trainingLabels is a %d by %d matrix." trainingLabels.RowCount trainingLabels.ColumnCount
