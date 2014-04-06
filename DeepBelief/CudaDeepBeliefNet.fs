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

module CudaDeepBeliefNet =

    open DeepBeliefNet
    open CudaTemplates
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Utils

    let gpuRbmUp weightsAndBiases activation xInputs =
        use multiplyProgram = 32 |> multiplyTemplate |> Compiler.load Worker.Default
        multiplyProgram.Run xInputs (transpose weightsAndBiases) |> mapMatrix activation

    let gpuRbmTrain rand (rbm : RestrictedBoltzmannMachine) (xInputs : Matrix) =
        use cudaRbmEpochProgram = 32 |> trainRbmEpochTemplate |> Compiler.load Worker.Default
        let epochs = value rbm.Parameters.Epochs
        [1..epochs] |> List.fold(fun acc i -> cudaRbmEpochProgram.Run rand acc xInputs) rbm

    let gpuDbnTrain rand (dbn : DeepBeliefNetwork) xInputs =
        let prependedInputs = xInputs |> prependColumnOfOnes
        let start = gpuRbmTrain rand (List.head dbn.Machines) prependedInputs
        { 
            Parameters = dbn.Parameters
            Machines = dbn.Machines.Tail |> List.fold(fun acc element -> 
                let currentTuple = List.head acc
                let x = gpuRbmUp (fst currentTuple |> toWeightsAndBiases) sigmoidFunction (snd currentTuple)
                let nextRbm = gpuRbmTrain rand element x
                (nextRbm, x) :: acc) [(start, prependedInputs)]
                |> List.map fst |> List.rev 
        }
