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
open DeepBelief.DeepBeliefNet
open DeepBelief.CudaTemplates
open DeepBelief.Kernels
open DeepBelief.Utils
open System

module Common =
    type BinaryMatrixOperationKernelSignature = deviceptr<float32> -> deviceptr<float32> -> deviceptr<float32> -> unit
    let binaryMatrixOperation blockSize A B (kernel : Kernel<BinaryMatrixOperationKernelSignature>) (worker : Worker) =
        let hA = height A
        let wA = width A
        let paddedA = padToMultiplesOf blockSize A
        let paddedB = padToMultiplesOf blockSize B
        let hPaddedA = height paddedA
        let wPaddedA = width paddedA
        let flattenedA = flattenMatrix paddedA
        let flattenedB = flattenMatrix paddedB

        use flattenedA = worker.Malloc flattenedA
        use flattenedB = worker.Malloc flattenedB
        use result = worker.Malloc<float32> flattenedA.Length

        let lp = createSimpleMatrixOperationLp blockSize hPaddedA wPaddedA
        kernel.Launch lp result.Ptr flattenedA.Ptr flattenedB.Ptr

        result.Gather() |> rebuildMatrix wPaddedA hA wA

    type BinaryVectorOperationKernelSignature = deviceptr<float32> -> deviceptr<float32> -> deviceptr<float32> -> unit
    let binaryVectorOperation blockSize x y (kernel : Kernel<BinaryVectorOperationKernelSignature>) (worker : Worker) =
        let size = Array.length x
        let paddedX = padToMultipleOf blockSize x
        let paddedY = padToMultipleOf blockSize y

        use paddedX = worker.Malloc paddedX
        use paddedY = worker.Malloc paddedY
        use result = worker.Malloc<float32> paddedX.Length

        let lp = createSimpleVectorOperationLp blockSize paddedX.Length
        kernel.Launch lp result.Ptr paddedX.Ptr paddedY.Ptr

        let result = result.Gather() 
        Array.sub result 0 size

