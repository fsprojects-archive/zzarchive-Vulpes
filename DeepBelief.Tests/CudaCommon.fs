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

module CudaCommon =

    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Xunit
    open FsUnit.Xunit
    open DeepBelief.CudaNeuralNet
    open DeepBelief.CudaTemplates
    open DeepBelief.DeepBeliefNet
    open DeepBelief.Utils
    open DeepBelief.Kernels
    open DeepBelief.NeuralNet
    open TestUtils
    open System

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

    let sigmoidTemplate (blockSize:int) = cuda {
        let! sigmoidKernel = <@ sigmoid @> |> transformKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let sigmoidKernel = program.Apply sigmoidKernel

            fun (vector : Vector) start length -> 

                let size = vector.Length
                let vector = vector |> padToMultipleOf blockSize
                let simpleVectorLp = createSimpleVectorOperationLp blockSize vector.Length

                let vector = worker.Malloc vector

                sigmoidKernel.Launch simpleVectorLp vector.Ptr vector.Ptr start length

                Array.sub (vector.Gather()) 0 8
        ) }

    let feedForwardTemplate (blockSize:int) = cuda {
        let! multiplyVectorByMatrixAndTransformTwiceKernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize <@ sigmoid @> <@ dSigmoid @> |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByMatrixAndTransformTwiceKernel = program.Apply multiplyVectorByMatrixAndTransformTwiceKernel
            let coerceKernel = program.Apply coerceKernel

            fun (netProps : NnetProperties) data -> 
                let paddedWeights = netProps.Weights |> List.map (prependRowOfZeroes >> padToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize (height w) (width w))
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (height w))

                let inputs0 = worker.Malloc<float32>(width paddedWeights.[0])
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))

                // The contents of these lists will need to be disposed at the end of the run.
                let weights = paddedWeights |> List.map (flattenMatrix >> worker.Malloc)
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))

                let mutable result = []
                let N = weights.Length - 1
                for i in 0..Array.length data - 1 do
                    inputs0.Scatter(fst data.[i] |> padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch coerceLp lastOutput.Ptr 0 1.0f
                        multiplyVectorByMatrixAndTransformTwiceKernel.Launch forwardLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (height paddedWeights.[j]) (width paddedWeights.[j])

                    let zippedOutputs = List.zip outputs dOutputs
                    let gatheredOutputs = zippedOutputs |> List.mapi (fun iw (output, dOutput) -> (Array.sub (output.Gather()) 1 (height netProps.Weights.[iw]), Array.sub (dOutput.Gather()) 1 (height netProps.Weights.[iw])))
                    result <- gatheredOutputs :: result

                disposeAll [|weights; dOutputs|]
                result
        ) }

    let errorSignalsTemplate (blockSize:int) = cuda {
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByTransposeOfMatrixKernel = program.Apply multiplyVectorByTransposeOfMatrixKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel

            fun Ws (layerOutputs : (Vector * Vector) list) (target : Vector) ->
                let N = List.length Ws - 1
                let paddedWeights = Ws |> List.map (prependRowOfZeroes >> padToMultiplesOf blockSize)
                let paddedTarget = target |> (prepend 0.0f >> padToMultipleOf blockSize)
                let paddedOutputValues = layerOutputs |> List.map (fst >> prepend 0.0f >> padToMultipleOf blockSize)
                let paddedOutputDerivatives = layerOutputs |> List.map (snd >> prepend 0.0f >> padToMultipleOf blockSize)

                let errorSignalsLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (height w))
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize (height w) (width w))

                use paddedTargetDevice = worker.Malloc(paddedTarget)

                // The contents of these lists will need to be disposed at the end of the run.
                let errorSignalsDevice = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))
                let weightsDevice = paddedWeights |> List.map (flattenMatrix >> worker.Malloc)
                let paddedOutputValuesDevice = paddedOutputValues |> List.map (fun o -> worker.Malloc(o)) |> List.rev
                let paddedOutputDerivativesDevice = paddedOutputDerivatives |> List.map (fun o' -> worker.Malloc(o')) |> List.rev

                subtractVectorKernel.Launch errorSignalsLp.[N] errorSignalsDevice.[N].Ptr paddedTargetDevice.Ptr paddedOutputValuesDevice.[N].Ptr

                for j in N..(-1)..0 do
                    if j < N then
                        multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j + 1] errorSignalsDevice.[j].Ptr weightsDevice.[j + 1].Ptr errorSignalsDevice.[j + 1].Ptr (height paddedWeights.[j + 1]) (width paddedWeights.[j + 1])
                    pointwiseMultiplyVectorKernel.Launch errorSignalsLp.[j] errorSignalsDevice.[j].Ptr paddedOutputDerivativesDevice.[j].Ptr errorSignalsDevice.[j].Ptr

                let output = errorSignalsDevice |> List.mapi (fun i e -> e.Gather().[1..(fst layerOutputs.[N - i] |> Array.length)])
                disposeAll [|errorSignalsDevice; weightsDevice; paddedOutputValuesDevice; paddedOutputDerivativesDevice|]
                output                
        ) }

    let gradientsTemplate (blockSize:int) = cuda {
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! outerProductKernel = outerProductKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByTransposeOfMatrixKernel = program.Apply multiplyVectorByTransposeOfMatrixKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel
            let outerProductKernel = program.Apply outerProductKernel

            fun Ws (layerOutputs : (Vector * Vector) list) (target : Vector) ->
                let N = List.length Ws - 1
                let paddedWeights = Ws |> List.map (prependRowOfZeroes >> padToMultiplesOf blockSize)
                let paddedTarget = target |> (prepend 0.0f >> padToMultipleOf blockSize)
                let paddedOutputValues = layerOutputs |> List.map (fst >> prepend 0.0f >> padToMultipleOf blockSize)
                let paddedOutputDerivatives = layerOutputs |> List.map (snd >> prepend 0.0f >> padToMultipleOf blockSize)

                let errorSignalsLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (height w))
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize (height w) (width w))
                let simpleMatrixLp = paddedWeights |> List.map (fun w -> createSimpleMatrixOperationLp blockSize (height w) (width w))

                use paddedTargetDevice = worker.Malloc(paddedTarget)

                use inputs0Device = worker.Malloc<float32>(width paddedWeights.[0])

                // The contents of these lists will need to be disposed at the end of the run.
                let errorSignalsDevice = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w))
                let weightsDevice = paddedWeights |> List.map (flattenMatrix >> worker.Malloc)
                let paddedOutputValuesDevice = paddedOutputValues |> List.map (fun o -> worker.Malloc(o)) |> List.rev
                let paddedOutputDerivativesDevice = paddedOutputDerivatives |> List.map (fun o' -> worker.Malloc(o')) |> List.rev
                let gradsDevice = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(height w * width w))

                let inputsDevice = inputs0Device :: paddedOutputValuesDevice

                subtractVectorKernel.Launch errorSignalsLp.[N] errorSignalsDevice.[N].Ptr paddedTargetDevice.Ptr paddedOutputValuesDevice.[N].Ptr

                for j in N..(-1)..0 do
                    if j < N then
                        multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j + 1] errorSignalsDevice.[j].Ptr weightsDevice.[j + 1].Ptr errorSignalsDevice.[j + 1].Ptr (height paddedWeights.[j + 1]) (width paddedWeights.[j + 1])
                    pointwiseMultiplyVectorKernel.Launch errorSignalsLp.[j] errorSignalsDevice.[j].Ptr paddedOutputDerivativesDevice.[j].Ptr errorSignalsDevice.[j].Ptr
                    outerProductKernel.Launch simpleMatrixLp.[j] gradsDevice.[j].Ptr errorSignalsDevice.[j].Ptr inputsDevice.[j].Ptr (width paddedWeights.[j])

                let output = gradsDevice |> List.mapi (fun i e -> e.Gather() |> rebuildMatrix (height paddedWeights.[i]) (width paddedWeights.[i]) (height paddedWeights.[i]))
                disposeAll [|errorSignalsDevice; weightsDevice; paddedOutputValuesDevice; paddedOutputDerivativesDevice; gradsDevice|]
                output                
        ) }
