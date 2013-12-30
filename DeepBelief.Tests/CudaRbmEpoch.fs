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

type ``CUDA RBM Epoch``() =

    let sizes = [500; 250; 100; 50]
    let alpha = 0.5f
    let momentum = 0.9f
    let xInputs = Array2D.init 3000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let layeredDbn = dbn sizes xInputs
    let firstRbm = layeredDbn.[0]

    let runEpoch rbm =
        use cudaRbmEpochProgram = 32 |> runRbmEpochTemplate |> Compiler.load Worker.Default
        cudaRbmEpochProgram.Run alpha momentum 1000 rbm xInputs

    let result = [1..5] |> List.fold (fun acc element -> runEpoch acc) firstRbm

    [<Fact>] member test.
        ``The RBM Epoch template runs 5 epochs on the GPU.``()=
            (height result.Weights, width result.Weights) |> should equal (500, 784)

    [<Fact>] member test.
        ``The output of the 5 Epoch run is valid.``()=
            result.Weights |> flattenMatrix |> Array.filter Single.IsNaN |> Array.length |> should equal 0




