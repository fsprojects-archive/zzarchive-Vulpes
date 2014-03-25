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
    let layeredDbn = initDbn sizes xInputs
    let firstRbm = layeredDbn.Machines.[0]

    let trainRbmEpoch rbm =
        use cudaRbmEpochProgram = 32 |> trainRbmEpochTemplate |> Compiler.load Worker.Default
        cudaRbmEpochProgram.Run alpha momentum 1000 rbm xInputs

    let result = [1..5] |> List.fold (fun acc element -> trainRbmEpoch acc) firstRbm

    [<Fact>] member test.
        ``The RBM Epoch template runs 5 epochs on the GPU.``()=
            (height result.Weights, width result.Weights) |> should equal (500, 784)

    [<Fact>] member test.
        ``The output of the 5 Epoch run is valid.``()=
            result.Weights |> flattenMatrix |> Array.filter Single.IsNaN |> Array.length |> should equal 0
