#load "MnistDataLoad.fs"

open DeepBelief
open MnistDataLoad
open System.IO

Directory.SetCurrentDirectory(@"C:\Users\white_000\Documents\GitHub\Vulpes\DeepBelief")

loadMnistImage MnistTestImageData
loadMnistImage MnistTrainingImageData

loadMnistLabel MnistTestLabelData
loadMnistLabel MnistTrainingLabelData
