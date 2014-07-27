namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open FsUnit.Xunit
open DeepBelief
open DeepBeliefNet
open DeepBelief.CudaTemplates
open DeepBelief.Kernels
open DeepBelief.Utils
open System

//type ``CUDA RBM Epoch``() =
//
//    let rand = new Random()
//    let xInputs = Array2D.init 3000 784 (fun _ _ -> rand.NextDouble() |> float32)
//    let dbnParameters = 
//        {
//            Layers = LayerSizes [500; 250; 100; 10]
//            LearningRate = LearningRate 0.9f
//            Momentum = Momentum 0.2f
//            BatchSize = BatchSize 1000
//            Epochs = Epochs 2
//        }
//
//    let layeredDbn = initDbn dbnParameters xInputs
//    let firstRbm = layeredDbn.Machines.[0]
//
//    let trainRbmEpoch rbm =
//        use cudaRbmEpochProgram = 32 |> trainRbmEpochTemplate |> Compiler.load Worker.Default
//        cudaRbmEpochProgram.Run rand rbm xInputs
//
//    let result = [1..5] |> List.fold (fun acc element -> trainRbmEpoch acc) firstRbm
//
//    [<Fact>] member test.
//        ``The RBM Epoch template runs 5 epochs on the GPU.``()=
//            (height result.Weights, width result.Weights) |> should equal (500, 784)
//
//    [<Fact>] member test.
//        ``The output of the 5 Epoch run is valid.``()=
//            result.Weights |> flattenMatrix |> Array.filter Single.IsNaN |> Array.length |> should equal 0
