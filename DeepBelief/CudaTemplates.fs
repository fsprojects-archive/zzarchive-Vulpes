namespace DeepBelief

module CudaTemplates =

    open System
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Kernels
    open DeepBeliefNet
    open Common.Analytics
    open Common.CudaTemplates
    open Common.Kernels
    open Common.NeuralNet
    open Common.Utils
    open Utils

    type InputBatch with
        member this.PadToMultiplesOf blockSize =
            match this with InputBatch inputBatch -> inputBatch.PadToMultiplesOf blockSize |> InputBatch
        member this.WeightedLearningRate (parameters : RestrictedBoltzmannParameters) =
            match parameters.LearningRate with LearningRate learningRate -> learningRate / (this.Size |> float32)

    type Momentum with
        member this.Value = match this with Momentum momentum -> momentum

    type RandomInputs = {
        HiddenRandoms1 : DeviceMemory<float32>
        VisibleRandoms2 : DeviceMemory<float32>
        HiddenRandoms2 : DeviceMemory<float32>
    } with
        interface IDisposable
            with member this.Dispose() = 
                    this.HiddenRandoms1.Dispose()
                    this.VisibleRandoms2.Dispose()
                    this.HiddenRandoms2.Dispose()

    type RandomInputsList = RandomInputsList of RandomInputs list with
        member this.Batch i =
            match this with RandomInputsList randomInputsList -> randomInputsList.[i]
        interface IDisposable
            with member this.Dispose() =
                    match this with 
                        RandomInputsList randomInputsList ->
                        for randomInputs in randomInputsList do (randomInputs :> IDisposable).Dispose()

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

            fun (A : Matrix) (B : Matrix) ->
                let finalHeight = A.Height
                let finalWidth = B.Width

                let A = A.PadToMultiplesOf blockSize
                let B = B.PadToMultiplesOf blockSize

                let hA = A.Height
                let wA = A.Width
                let hB = B.Height
                let wB = B.Width
                let wC = wB
                let hC = A.Height

                let A = A.ToRowMajorFormat
                let B = B.ToRowMajorFormat

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyLp blockSize hA wA hB wB
                kernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                let result = C.Gather() |> Matrix.FromRowMajorFormat wC 
                result.Submatrix 0 0 finalHeight finalWidth
            ) }

    let multiplyByTransposeTemplate (blockSize:int) = cuda {
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program:Program) ->
            let worker = program.Worker
            let multiplyByTransposeKernel = program.Apply(multiplyByTransposeKernel)

            fun (A : Matrix) (B : Matrix) ->
                let finalHeight = A.Height
                let finalWidth = B.Height

                let A = A.PadToMultiplesOf blockSize
                let B = B.PadToMultiplesOf blockSize

                let hA = A.Height
                let wA = A.Width
                let hB = B.Height
                let wB = B.Width
                let wC = hB
                let hC = A.Height

                let A = A.ToRowMajorFormat
                let B = B.ToRowMajorFormat

                use A = worker.Malloc(A)
                use B = worker.Malloc(B)
                use C = worker.Malloc<float32>(wC * hC)

                let lp = createMultiplyByTransposeLp blockSize hA wA hB wB
                multiplyByTransposeKernel.Launch lp C.Ptr A.Ptr B.Ptr hA wA hB wB
                let result = C.Gather() |> Matrix.FromRowMajorFormat wC
                result.Submatrix 0 0 finalHeight finalWidth
            ) }

    let trainRbmEpochTemplate (blockSize:int) = cuda {
        let! multiplyKernel = multiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! multiplyByTransposeKernel = multiplyByTransposeStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! transposeAndMultiplyKernel = transposeAndMultiplyStrategy blockSize |> matrixMulKernel blockSize |> Compiler.DefineKernel
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel
        let! activateKernel = <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel
        let! addMatrixKernel = <@ pointwiseAdd @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! subtractMatrixKernel = <@ pointwiseSubtract @> |> pointwiseBinaryOperationKernel blockSize |> Compiler.DefineKernel
        let! scalarMultiplyMatrixKernel = scalarMultiplyMatrixKernel blockSize |> Compiler.DefineKernel

        return Entry(fun program ->
            let worker = program.Worker
            let multiplyKernel = program.Apply multiplyKernel
            let multiplyByTransposeKernel = program.Apply multiplyByTransposeKernel
            let transposeAndMultiplyKernel = program.Apply transposeAndMultiplyKernel
            let activateFirstRowKernel = program.Apply activateFirstRowKernel
            let activateFirstColumnKernel = program.Apply activateFirstColumnKernel
            let activateKernel = program.Apply activateKernel
            let addMatrixKernel = program.Apply addMatrixKernel
            let subtractMatrixKernel = program.Apply subtractMatrixKernel
            let scalarMultiplyMatrixKernel = program.Apply scalarMultiplyMatrixKernel

            fun rnd (rbm : RestrictedBoltzmannMachine) (inputs : LayerInputs) -> 
                let batches = inputs.GetRandomisedInputBatches rnd rbm.Parameters.BatchSize
                let nRows = batches.Head.Size
                let nCols = batches.Head.Dimension
                let batches = batches |> List.map (fun inputBatch -> inputBatch.PadToMultiplesOf blockSize)
                let learningRate = rbm.Parameters.LearningRate
                let weightedLearningRate = batches.Head.WeightedLearningRate rbm.Parameters
                let paddedBatchHeight = batches.Head.Size
                let paddedBatchWidth = batches.Head.Dimension
                let batches = batches |> List.map (fun (InputBatch inputBatch) -> inputBatch.ToRowMajorFormat) |> List.map (worker.Malloc)
                let nHidden = rbm.NumberOfHiddenUnits
                let nVisible = rbm.NumberOfVisibleUnits
                
                let hVisibleUnitMatrix = paddedBatchHeight
                let wVisibleUnitMatrix = paddedBatchWidth

                let wHiddenUnitMatrix = hVisibleUnitMatrix
                let hHiddenUnitMatrix = 1 + nHidden |> nextMultipleOf blockSize

                let dimVisibleUnits = hVisibleUnitMatrix * wVisibleUnitMatrix
                let dimHiddenUnits = hHiddenUnitMatrix * wHiddenUnitMatrix

                let weightsAndBiases = rbm.ToWeightsAndBiases.PadToMultiplesOf blockSize 
                let dWeightsAndBiases = rbm.ToWeightsAndBiasesChanges.PadToMultiplesOf blockSize
                let weightsAndBiasesWidth = weightsAndBiases.Width
                let weightsAndBiasesHeight = weightsAndBiases.Height
                let weightsAndBiases = weightsAndBiases.ToRowMajorFormat
                let dWeightsAndBiases = dWeightsAndBiases.ToRowMajorFormat
                let dimWeightsAndBiases = Array.length weightsAndBiases

                use weightsAndBiases = worker.Malloc weightsAndBiases
                use dWeightsAndBiases = worker.Malloc dWeightsAndBiases
                use h1 = worker.Malloc<float32>(dimHiddenUnits)
                use v2 = worker.Malloc<float32>(dimVisibleUnits)
                use h2 = worker.Malloc<float32>(dimHiddenUnits)
                use v1ActivatedForBias = worker.Malloc<float32>(dimVisibleUnits)
                use h1ActivatedForBias = worker.Malloc<float32>(dimHiddenUnits)
                use v2ActivatedForBias = worker.Malloc<float32>(dimVisibleUnits)
                use c1 = worker.Malloc<float32>(dimWeightsAndBiases)
                use c2 = worker.Malloc<float32>(dimWeightsAndBiases)

                let makeHiddenRandomRow() =
                    Array.init (nHidden + 1) (fun _ -> rnd.NextSingle) |> Vector |> fun v -> v.PadToMultipleOf blockSize
                let makeVisibleRandomRow() =
                    Array.init (nVisible + 1) (fun _ -> rnd.NextSingle) |> Vector |> fun v -> v.PadToMultipleOf blockSize

                let preloadedRandoms = [1..batches.Length] |> List.map (fun _ -> 
                    {
                        HiddenRandoms1 = Array.init nRows (fun _ -> makeHiddenRandomRow()) |> Array.concat |> worker.Malloc;
                        VisibleRandoms2 = Array.init nRows (fun _ -> makeVisibleRandomRow()) |> Array.concat |> worker.Malloc;
                        HiddenRandoms2 = Array.init nRows (fun _ -> makeHiddenRandomRow()) |> Array.concat |> worker.Malloc
                    }) 
                use preloadedRandoms = preloadedRandoms |> RandomInputsList

                let threads = dim3(blockSize, blockSize)

                let forwardMatrixLp = createMultiplyByTransposeLp blockSize weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                let backwardMatrixLp = createTransposeAndMultiplyLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                let activateHiddenLp = createSimpleMatrixOperationLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix
                let activateVisibleLp = createSimpleMatrixOperationLp blockSize hVisibleUnitMatrix wVisibleUnitMatrix
                let activateFirstRowLp = createActivateFirstRowLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix
                let activateFirstColumnLp = createActivateFirstColumnLp blockSize hVisibleUnitMatrix wVisibleUnitMatrix
                let computeCValueLp = createMultiplyLp blockSize hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix
                let simpleWeightsLp = createSimpleMatrixOperationLp blockSize hHiddenUnitMatrix wVisibleUnitMatrix

                let numRuns = 3 * batches.Length
                for i in 0..batches.Length - 1 do
                    
                    use v1 = batches.[i]

                    let randoms = preloadedRandoms.Batch i

                    // Perform the forward iteration to populate h1
                    activateFirstColumnKernel.Launch activateFirstColumnLp v1ActivatedForBias.Ptr v1.Ptr hVisibleUnitMatrix wVisibleUnitMatrix nCols
                    multiplyByTransposeKernel.Launch forwardMatrixLp h1.Ptr weightsAndBiases.Ptr v1ActivatedForBias.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    let x = h1.Gather()
                    let y = weightsAndBiases.Gather()
                    activateKernel.Launch activateHiddenLp h1.Ptr h1.Ptr randoms.HiddenRandoms1.Ptr

                    // Perform the backward iteration to populate v2
                    activateFirstRowKernel.Launch activateFirstRowLp h1ActivatedForBias.Ptr h1.Ptr wHiddenUnitMatrix nRows
                    transposeAndMultiplyKernel.Launch backwardMatrixLp v2.Ptr h1ActivatedForBias.Ptr weightsAndBiases.Ptr hHiddenUnitMatrix wHiddenUnitMatrix weightsAndBiasesHeight weightsAndBiasesWidth
                    activateKernel.Launch activateVisibleLp v2.Ptr v2.Ptr randoms.VisibleRandoms2.Ptr

                    // Perform the forward iteration to populate h2
                    activateFirstColumnKernel.Launch activateFirstColumnLp v2ActivatedForBias.Ptr v2.Ptr hVisibleUnitMatrix wVisibleUnitMatrix nCols
                    multiplyByTransposeKernel.Launch forwardMatrixLp h2.Ptr weightsAndBiases.Ptr v2ActivatedForBias.Ptr weightsAndBiasesHeight weightsAndBiasesWidth hVisibleUnitMatrix wVisibleUnitMatrix
                    activateKernel.Launch activateHiddenLp h2.Ptr h2.Ptr randoms.HiddenRandoms2.Ptr

                    // Compute c1 = h1 * v1 and c2 = h2 * v2
                    multiplyKernel.Launch computeCValueLp c1.Ptr h1.Ptr v1.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix
                    multiplyKernel.Launch computeCValueLp c2.Ptr h2.Ptr v2.Ptr hHiddenUnitMatrix wHiddenUnitMatrix hVisibleUnitMatrix wVisibleUnitMatrix

                    // dWeightsAndBiases -> momentum * dWeightsAndBiases + weightedLearningRate * (c1 - c2)
                    subtractMatrixKernel.Launch simpleWeightsLp c1.Ptr c1.Ptr c2.Ptr
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp c1.Ptr weightedLearningRate
                    scalarMultiplyMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr rbm.Parameters.Momentum.Value
                    addMatrixKernel.Launch simpleWeightsLp dWeightsAndBiases.Ptr dWeightsAndBiases.Ptr c1.Ptr

                    // weightsAndBiases -> weightsAndBiases + dWeightsAndBiases
                    addMatrixKernel.Launch simpleWeightsLp weightsAndBiases.Ptr weightsAndBiases.Ptr dWeightsAndBiases.Ptr

                let weightsAndBiases = weightsAndBiases.Gather() |> Matrix.FromRowMajorFormat weightsAndBiasesWidth
                let wbg = dWeightsAndBiases.Gather()
                let max = Array.maxBy (fun el -> Math.Abs(el |> float)) (Array.sub wbg 1 (wbg.Length - 1))
                let dWeightsAndBiases = wbg |> Matrix.FromRowMajorFormat weightsAndBiasesWidth
                let result = RestrictedBoltzmannMachine.FromWeightsAndBiases rbm.Parameters (weightsAndBiases.Submatrix 0 0 (nHidden + 1) (nVisible + 1) |> WeightsAndBiases) (dWeightsAndBiases.Submatrix 0 0 (nHidden + 1) (nVisible + 1) |> WeightChanges)
                result
        ) }

