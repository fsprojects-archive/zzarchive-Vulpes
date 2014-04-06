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

module CudaTemplates =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Kernels
    open DeepBeliefNet
    open NeuralNet
    open Utils

    let coerceLp blockSize =
        let threads = dim3(blockSize)
        let grid = dim3(1)
        LaunchParam(grid, threads)

    let createMultiplyVectorByMatrixLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(hA / threads.x)
        LaunchParam(grid, threads)

    let createMultiplyVectorByTransposeOfMatrixLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(wA / threads.x)
        LaunchParam(grid, threads)

    let createMultiplyLp blockSize hA wA hB wB =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(wB / threads.x, hA / threads.y)
        LaunchParam(grid, threads)

    let createMultiplyByTransposeLp blockSize hA wA hB wB =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(hB / threads.x, hA / threads.y)
        LaunchParam(grid, threads)

    let createTransposeAndMultiplyLp blockSize hA wA hB wB =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(wB / threads.x, wA / threads.y)
        LaunchParam(grid, threads)

    let createSimpleVectorOperationLp blockSize size =
        let threads = dim3(blockSize)
        let grid = dim3(size / threads.x)
        LaunchParam(grid, threads)

    let createOuterProductLp blockSize hA wA =
        let threads = dim3(blockSize, blockSize)
        let grid = dim3(hA / threads.x, wA / threads.y)
        LaunchParam(grid, threads)

    let createSimpleMatrixOperationLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3((hA * wA) / threads.x)
        LaunchParam(grid, threads)

    let createOffsetMatrixOperationLp blockSize hA wA =
        let threads = dim3(blockSize)
        let grid = dim3(((hA - 1) * wA) / threads.x)
        LaunchParam(grid, threads)

    let createActivateFirstRowLp blockSize hM wM =
        let threads = dim3(blockSize)
        let grid = dim3(wM / threads.x)
        LaunchParam(grid, threads)

    let createActivateFirstColumnLp blockSize hM wM =
        let threads = dim3(blockSize)
        let grid = dim3(hM / threads.x)
        LaunchParam(grid, threads)
        
    let multiplyTemplate (blockSize:int) = cuda {
        let! kernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let kernel = program.Apply(kernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                let finalHeight = Utils.height A
                let finalWidth = Utils.width B

                let A = Utils.padToMultiplesOf blockSize A
                let B = Utils.padToMultiplesOf blockSize B

                let hA = Utils.height A
                let wA = Utils.width A
                let hB = Utils.height B
                let wB = Utils.width B
                let wC = wB
                let hC = Utils.height A

                let A = Utils.flattenMatrix A
                let B = Utils.flattenMatrix B

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyLp blockSize hA wA hB wB
                kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                let result = C.Gather()
                result |> Utils.rebuildMatrix wC finalHeight finalWidth
            ) }

    let multiplyByTransposeTemplate (blockSize:int) = cuda {
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let multiplyByTransposeKernel = program.Apply(multiplyByTransposeKernel)

            fun (A : Utils.Matrix) (B : Utils.Matrix) ->
                let finalHeight = Utils.height A
                let finalWidth = Utils.height B

                let A = Utils.padToMultiplesOf blockSize A
                let B = Utils.padToMultiplesOf blockSize B

                let hA = Utils.height A
                let wA = Utils.width A
                let hB = Utils.height B
                let wB = Utils.width B
                let wC = hB
                let hC = Utils.height A

                let A = Utils.flattenMatrix A
                let B = Utils.flattenMatrix B

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyByTransposeLp blockSize hA wA hB wB
                multiplyByTransposeKernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                let result = C.Gather()
                result |> Utils.rebuildMatrix wC finalHeight finalWidth
            ) }

    let trainRbmEpochTemplate (blockSize:int) = cuda {
        let! multiplyKernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! rngKernel = <@ Utils.toFloat32 @> |> xorShiftKernel |> Compiler.DefineKernel
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel
        let! activateKernel = <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel
        let! addMatrixKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! subtractMatrixKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! scalarMultiplyMatrixKernel = scalarMultiplyMatrixKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let rngKernel = program.Apply rngKernel
            let multiplyKernel = program.Apply multiplyKernel
            let multiplyByTransposeKernel = program.Apply multiplyByTransposeKernel
            let transposeAndMultiplyKernel = program.Apply transposeAndMultiplyKernel
            let activateFirstRowKernel = program.Apply activateFirstRowKernel
            let activateFirstColumnKernel = program.Apply activateFirstColumnKernel
            let activateKernel = program.Apply activateKernel
            let addMatrixKernel = program.Apply addMatrixKernel
            let subtractMatrixKernel = program.Apply subtractMatrixKernel
            let scalarMultiplyMatrixKernel = program.Apply scalarMultiplyMatrixKernel

            // Copy pre-calculated bit-matrices, needed for jump-ahead
            // calculations, to the device memory.
            let jumpAheadMatrices = worker.Malloc(Data.jumpAheadMatrices)

            fun rand (rbm : RestrictedBoltzmannMachine) xInputs -> 
                let nRows = Utils.height xInputs
                let nCols = Utils.width xInputs
                let xRand = Utils.permuteRows rand xInputs
                let batchSize = value rbm.Parameters.BatchSize
                let samples = 
                    xRand |> Utils.batchesOf batchSize 
                    |> Array.map (array2D >> Utils.prependColumnOfOnes >> Utils.padToMultiplesOf blockSize)
                
                let paddedSampleHeight = Utils.height samples.[0]
                let paddedSampleWidth = Utils.width samples.[0]

                let samples = samples |> Array.map (Utils.flattenMatrix >> worker.Malloc)

                let nHidden = DeepBeliefNet.numberOfHiddenUnits rbm
                let nVisible = DeepBeliefNet.numberOfVisibleUnits rbm
                
                let hVisibleUnitMatrix = paddedSampleHeight
                let wVisibleUnitMatrix = paddedSampleWidth

                let wHiddenUnitMatrix = hVisibleUnitMatrix
                let hHiddenUnitMatrix = 1 + nHidden |> Utils.nextMultipleOf blockSize

                let dimVisibleUnits = hVisibleUnitMatrix * wVisibleUnitMatrix
                let dimHiddenUnits = hHiddenUnitMatrix * wHiddenUnitMatrix

                let weightsAndBiases = DeepBeliefNet.toWeightsAndBiases rbm |> Utils.padToMultiplesOf blockSize 
                let dWeightsAndBiases = DeepBeliefNet.toDWeightsAndBiases rbm |> Utils.padToMultiplesOf blockSize
                let weightsAndBiasesWidth = Utils.width weightsAndBiases
                let weightsAndBiasesHeight = Utils.height weightsAndBiases
                let weightsAndBiases = weightsAndBiases|> Utils.flattenMatrix
                let dWeightsAndBiases = dWeightsAndBiases |> Utils.flattenMatrix
                let dimWeightsAndBiases = Array.length weightsAndBiases

                use weightsAndBiases = worker.Malloc weightsAndBiases
                use dWeightsAndBiases = worker.Malloc dWeightsAndBiases
                use h1 = worker.Malloc<float32>(dimHiddenUnits)
                use v2 = worker.Malloc<float32>(dimVisibleUnits)
                use h2 = worker.Malloc<float32>(dimHiddenUnits)
                use c1 = worker.Malloc<float32>(dimWeightsAndBiases)
                use c2 = worker.Malloc<float32>(dimWeightsAndBiases)

                use hiddenRandoms = worker.Malloc<float32>(dimHiddenUnits)
                use visibleRandoms = worker.Malloc<float32>(dimVisibleUnits)

                let threads = dim3(blockSize, blockSize)

                let forwardMatrixLp = createMultiplyByTransposeLp blockSize weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                let backwardMatrixLp = createTransposeAndMultiplyLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                let activateHiddenLp = createSimpleMatrixOperationLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix
                let activateVisibleLp = createSimpleMatrixOperationLp blockSize hVisibleUnitMatrix wVisibleUnitMatrix
                let activateFirstRowLp = createActivateFirstRowLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix
                let activateFirstColumnLp = createActivateFirstColumnLp blockSize hVisibleUnitMatrix wVisibleUnitMatrix
                let computeCValueLp = createMultiplyLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix
                let simpleWeightsLp = createSimpleMatrixOperationLp blockSize hHiddenUnitMatrix wVisibleUnitMatrix

                let rngNumStreams = 1024
                let rngBlockSize = dim3(32, 8)
                let rngNumThreadsPerBlock = rngBlockSize.Size
                let rngGridSize = dim3(rngNumStreams / rngNumThreadsPerBlock)
                let rngSharedMemorySize = XorShift7.Size * rngNumThreadsPerBlock
                let rngLp = LaunchParam(rngGridSize, rngBlockSize, rngSharedMemorySize)

                let learningRate = value rbm.Parameters.LearningRate
                let momentum = value rbm.Parameters.Momentum
                let weightedLearningRate = learningRate / (float32 samples.Length)
                use state0 = Utils.generateStartState 42u |> worker.Malloc

                let numRuns = 3 * samples.Length
                for i in 0..samples.Length - 1 do
                    
                    use v1 = samples.[i]

                    // Perform the forward iteration to populate h1
                    multiplyByTransposeKernel.Launch forwardMatrixLp h1.Ptr weightsAndBiases.Ptr v1.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns i state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h1.Ptr h1.Ptr hiddenRandoms.Ptr
                    activateFirstRowKernel.Launch activateFirstRowLp h1.Ptr wHiddenUnitMatrix nRows

                    // Perform the backward iteration to populate v2
                    transposeAndMultiplyKernel.Launch backwardMatrixLp v2.Ptr h1.Ptr weightsAndBiases.Ptr hHiddenUnitMatrix wHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                    rngKernel.Launch rngLp numRuns (i + samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimVisibleUnits / rngNumStreams) visibleRandoms.Ptr
                    activateKernel.Launch activateVisibleLp v2.Ptr v2.Ptr visibleRandoms.Ptr
                    activateFirstColumnKernel.Launch activateFirstColumnLp v2.Ptr hVisibleUnitMatrix wVisibleUnitMatrix nCols

                    // Perform the forward iteration to populate h2
                    multiplyByTransposeKernel.Launch forwardMatrixLp h2.Ptr weightsAndBiases.Ptr v2.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    rngKernel.Launch rngLp numRuns (i + 2 * samples.Length) state0.Ptr jumpAheadMatrices.Ptr (dimHiddenUnits / rngNumStreams) hiddenRandoms.Ptr
                    activateKernel.Launch activateHiddenLp h2.Ptr h2.Ptr hiddenRandoms.Ptr
                    activateFirstRowKernel.Launch activateFirstRowLp h2.Ptr wHiddenUnitMatrix nRows

                    // Compute c1 = h1 * v1 and c2 = h2 * v2
                    multiplyKernel.Launch computeCValueLp c1.Ptr h1.Ptr v1.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix
                    multiplyKernel.Launch computeCValueLp c2.Ptr h2.Ptr v2.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix

                    // dWeightsAndBiases -> momentum * dWeightsAndBiases + weightedLearningRate * (c1 - c2)
                    subtractMatrixKernel.Launch simpleWeightsLp c1.Ptr c1.Ptr c2.Ptr
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp c1.Ptr weightedLearningRate
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr momentum
                    addMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr dWeightsAndBiases.Ptr c1.Ptr

                    // weightsAndBiases -> weightsAndBiases + dWeightsAndBiases
                    addMatrixKernel.Launch simpleWeightsLp weightsAndBiases.Ptr weightsAndBiases.Ptr dWeightsAndBiases.Ptr

                let weightsAndBiases = weightsAndBiases.Gather() |> Utils.rebuildMatrix wVisibleUnitMatrix (nHidden + 1) (nVisible + 1)
                let wbg = dWeightsAndBiases.Gather()
                let max = Array.maxBy (fun el -> Math.Abs(el |> float)) (Array.sub wbg 1 (wbg.Length - 1))
                let dWeightsAndBiases = wbg |> Utils.rebuildMatrix wVisibleUnitMatrix (nHidden + 1) (nVisible + 1)
                let result = DeepBeliefNet.toRbm rbm.Parameters weightsAndBiases dWeightsAndBiases
                result
        ) }

    let runTrainNeuralNetEpochTemplate (parameters : BackPropagationParameters) (blockSize : int) = cuda {
        let! multiplyVectorByMatrixAndTransformKernel = multiplyVectorByMatrixAndTransformKernel blockSize <@ sigmoid @> |> Compiler.DefineKernel
        let! multiplyVectorByMatrixAndTransformTwiceKernel = multiplyVectorByMatrixAndTransformTwiceKernel blockSize <@ sigmoid @> <@ dSigmoid @> |> Compiler.DefineKernel
        let! multiplyVectorByTransposeOfMatrixKernel = multiplyVectorByTransposeOfMatrixKernel blockSize |> Compiler.DefineKernel
        let! coerceKernel = coerceKernel blockSize |> Compiler.DefineKernel
        let! addVectorKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! subtractVectorKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! pointwiseMultiplyVectorKernel = <@ pointwiseMultiply @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! outerProductKernel = outerProductKernel blockSize |> Compiler.DefineKernel
        let! scalarMultiplyMatrixKernel = scalarMultiplyMatrixKernel blockSize |> Compiler.DefineKernel
        let! addMatrixKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyVectorByMatrixAndTransformKernel = program.Apply multiplyVectorByMatrixAndTransformKernel
            let multiplyVectorByMatrixAndTransformTwiceKernel = program.Apply multiplyVectorByMatrixAndTransformTwiceKernel
            let multiplyVectorByTransposeOfMatrixKernel = program.Apply multiplyVectorByTransposeOfMatrixKernel
            let coerceKernel = program.Apply coerceKernel
            let addVectorKernel = program.Apply addVectorKernel
            let subtractVectorKernel = program.Apply subtractVectorKernel
            let pointwiseMultiplyVectorKernel = program.Apply pointwiseMultiplyVectorKernel
            let outerProductKernel = program.Apply outerProductKernel
            let scalarMultiplyMatrixKernel = program.Apply scalarMultiplyMatrixKernel
            let addMatrixKernel = program.Apply addMatrixKernel

            fun (netProps : NnetProperties) (rand : Random) trainingSet testSet -> 
                let paddedWeights = netProps.Weights |> List.map (Utils.prependRowOfZeroes >> Utils.padToMultiplesOf blockSize)
                
                let forwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByMatrixLp blockSize (Utils.height w) (Utils.width w))
                let backwardLp = paddedWeights |> List.map (fun w -> createMultiplyVectorByTransposeOfMatrixLp blockSize (Utils.height w) (Utils.width w))
                let outputLp = paddedWeights |> List.map (fun w -> createSimpleVectorOperationLp blockSize (Utils.height w))
                let simpleMatrixLp = paddedWeights |> List.map (fun w -> createSimpleMatrixOperationLp blockSize (Utils.height w) (Utils.width w))
                let offsetMatrixLp = paddedWeights |> List.map (fun w -> createOffsetMatrixOperationLp blockSize (Utils.height w) (Utils.width w))
                let outerProductLp = paddedWeights |> List.map (fun w -> createOuterProductLp blockSize (Utils.height w) (Utils.width w))

                use inputs0 = worker.Malloc<float32>(Utils.width paddedWeights.[0])

                // The contents of these lists will need to be disposed at the end of the run.
                let outputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let weights = paddedWeights |> List.map (Utils.flattenMatrix >> worker.Malloc)
                let prevDWeights = paddedWeights |> List.map (fun w -> Array2D.zeroCreate (Utils.height w) (Utils.width w) |> Utils.flattenMatrix |> worker.Malloc)
                let grads = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w * Utils.width w))
                let dOutputs = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                let errorSignals = paddedWeights |> List.map (fun w -> worker.Malloc<float32>(Utils.height w))
                
                let inputs = inputs0 :: outputs
                let N = weights.Length - 1
                let epochs = value parameters.Epochs
                let learningRate = value parameters.LearningRate
                let momentum = value parameters.Momentum
                for i in 0..(Array.length trainingSet * epochs) - 1 do
                    let index = rand.Next (Array.length trainingSet)
                    inputs0.Scatter(fst trainingSet.[index] |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch (coerceLp 1) lastOutput.Ptr 0 0 1.0f
                        multiplyVectorByMatrixAndTransformTwiceKernel.Launch forwardLp.[j] dOutputs.[j].Ptr outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (Utils.height paddedWeights.[j]) (Utils.width paddedWeights.[j])
                        coerceKernel.Launch (coerceLp 1) dOutputs.[j].Ptr 0 0 0.0f

                        let minIndex = 1 + Utils.height netProps.Weights.[j]
                        let maxIndex = Utils.height paddedWeights.[j]

                        coerceKernel.Launch (coerceLp maxIndex) outputs.[j].Ptr minIndex maxIndex 0.0f
                        coerceKernel.Launch (coerceLp maxIndex) dOutputs.[j].Ptr minIndex maxIndex 0.0f

                    coerceKernel.Launch (coerceLp 1) outputs.[N].Ptr 0 0 1.0f

                    errorSignals.[N].Scatter (snd trainingSet.[index] |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)
                    subtractVectorKernel.Launch outputLp.[N] errorSignals.[N].Ptr errorSignals.[N].Ptr outputs.[N].Ptr

                    for j in N..(-1)..0 do
                        if j < N then 
                            multiplyVectorByTransposeOfMatrixKernel.Launch backwardLp.[j + 1] errorSignals.[j].Ptr weights.[j + 1].Ptr errorSignals.[j + 1].Ptr (Utils.height paddedWeights.[j + 1]) (Utils.width paddedWeights.[j + 1])

                        pointwiseMultiplyVectorKernel.Launch outputLp.[j] errorSignals.[j].Ptr dOutputs.[j].Ptr errorSignals.[j].Ptr
                        outerProductKernel.Launch outerProductLp.[j] grads.[j].Ptr errorSignals.[j].Ptr inputs.[j].Ptr (Utils.width paddedWeights.[j])

                    for j in N..(-1)..0 do
                        let wW = Utils.width paddedWeights.[j]
                        scalarMultiplyMatrixKernel.Launch simpleMatrixLp.[j] grads.[j].Ptr learningRate
                        scalarMultiplyMatrixKernel.Launch simpleMatrixLp.[j] prevDWeights.[j].Ptr momentum
                        addMatrixKernel.Launch offsetMatrixLp.[j] (prevDWeights.[j].Ptr + wW) (prevDWeights.[j].Ptr + wW) (grads.[j].Ptr + wW)
                        addMatrixKernel.Launch offsetMatrixLp.[j] (weights.[j].Ptr + wW) (weights.[j].Ptr + wW) (prevDWeights.[j].Ptr + wW)

                let mutable testOutputs = [||]
                for i in 0..Array.length testSet - 1 do
                    inputs0.Scatter(fst testSet.[i] |> Utils.prependForBias |> Utils.padToMultipleOf blockSize)

                    for j in 0..N do
                        let lastOutput = if j = 0 then inputs0 else outputs.[j - 1]
                        coerceKernel.Launch (coerceLp 1) lastOutput.Ptr 0 0 1.0f
                        multiplyVectorByMatrixAndTransformKernel.Launch forwardLp.[j] outputs.[j].Ptr weights.[j].Ptr lastOutput.Ptr (Utils.height paddedWeights.[j]) (Utils.width paddedWeights.[j])

                        let minIndex = 1 + Utils.height netProps.Weights.[j]
                        let maxIndex = Utils.height paddedWeights.[j]
                        coerceKernel.Launch (coerceLp maxIndex) outputs.[j].Ptr minIndex maxIndex 0.0f

                    let finalOutput = outputs.[N].Gather()
                    testOutputs <- Array.append testOutputs [|(Array.sub finalOutput 1 (Array.length (snd testSet.[i])))|]

                Utils.disposeAll [|outputs; weights; prevDWeights; grads; dOutputs; errorSignals|]
                testOutputs
        ) }
