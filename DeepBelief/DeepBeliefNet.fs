namespace DeepBelief

module DeepBeliefNet =

    open System
    open Utils
    open Common.Analytics
    open Common.NeuralNet
    open Backpropagation.Parameters
    open MathNet.Numerics.Distributions

    type DeepBeliefParameters = {
        Layers : LayerSizes
        LearningRate : LearningRate
        Momentum : Momentum
        BatchSize : BatchSize
        Epochs : Epochs
    }

    type RestrictedBoltzmannParameters = {
        LearningRate : LearningRate
        Momentum : Momentum
        BatchSize : BatchSize
        Epochs : Epochs
    }

    type RestrictedBoltzmannMachine = {
        Parameters : RestrictedBoltzmannParameters
        Weights : Matrix
        DWeights : Matrix
        VisibleBiases : Vector
        DVisibleBiases : Vector
        HiddenBiases : Vector
        DHiddenBiases : Vector
    }

    type DeepBeliefNetwork = {
        Parameters : DeepBeliefParameters
        Machines : RestrictedBoltzmannMachine list
    }

    type WeightsAndBiases with
        static member FromRbm (rbm : RestrictedBoltzmannMachine) =
            rbm.Weights.PrependColumn rbm.HiddenBiases |> WeightsAndBiases

    type DeepBeliefParameters with
        member this.ToRbmParameters =
            {
                RestrictedBoltzmannParameters.LearningRate = this.LearningRate;
                Momentum = this.Momentum;
                BatchSize = this.BatchSize;
                Epochs = this.Epochs
            }

    type RestrictedBoltzmannMachine with
        member this.ToWeightsAndBiases = 
            (this.Weights.PrependColumn this.HiddenBiases).PrependRow (this.VisibleBiases.Prepend 0.0f) |> WeightsAndBiases
        member this.ToWeightsAndBiasesChanges =
            (this.DWeights.PrependColumn this.DHiddenBiases).PrependRow (this.DVisibleBiases.Prepend 0.0f) |> WeightChanges
        member this.NumberOfHiddenUnits =
            this.HiddenBiases.Length
        member this.NumberOfVisibleUnits =
            this.VisibleBiases.Length
        static member FromWeightsAndBiases (parameters : RestrictedBoltzmannParameters) (WeightsAndBiases weightsAndBiases) (WeightChanges dWeightsAndBiases) =
            {
                Parameters = parameters;
                Weights = weightsAndBiases.Submatrix 1 1 (weightsAndBiases.Height - 1) (weightsAndBiases.Width - 1);
                DWeights = dWeightsAndBiases.Submatrix 1 1 (dWeightsAndBiases.Height - 1) (dWeightsAndBiases.Width - 1);
                HiddenBiases = (weightsAndBiases.Column 0).Subvector 1;
                DHiddenBiases = (dWeightsAndBiases.Column 0).Subvector 1;
                VisibleBiases = (weightsAndBiases.Row 0).Subvector 1;
                DVisibleBiases = (dWeightsAndBiases.Row 0).Subvector 1;
            }
        static member Initialise (parameters : RestrictedBoltzmannParameters) nVisible nHidden =
            let gaussianDistribution = new Normal(0.0, 0.01)
            let initGaussianWeights nRows nColumns =
                Array2D.init nRows nColumns (fun _ _ -> gaussianDistribution.Sample() |> float32) |> Matrix
            { 
                Parameters = parameters;
                Weights = initGaussianWeights nHidden nVisible;
                DWeights = Array2D.zeroCreate nHidden nVisible |> Matrix;
                VisibleBiases = Array.zeroCreate nVisible |> Vector;
                DVisibleBiases = Array.zeroCreate nVisible |> Vector;
                HiddenBiases = Array.zeroCreate nHidden |> Vector;
                DHiddenBiases = Array.zeroCreate nHidden |> Vector
            }            

    type DeepBeliefNetwork with
        member this.ToBackPropagationNetwork (backPropagationParameters : BackPropagationParameters) =
            let layers = this.Machines |> List.map (fun rbm -> { Weights = WeightsAndBiases.FromRbm rbm; Activation = sigmoidActivation }) 
            { Parameters = backPropagationParameters; Layers = layers }
        static member Initialise (deepBeliefParameters : DeepBeliefParameters) (TrainingSet trainingSet) =
            let toMachines (LayerSizes layers) =
                layers |> List.fold(fun acc element -> 
                    let nVisible = fst acc
                    let nHidden = element
                    let rbmParams = deepBeliefParameters.ToRbmParameters
                    (element, (RestrictedBoltzmannMachine.Initialise rbmParams nVisible nHidden) :: snd acc))
                    (trainingSet.Head.TrainingInput.Size, []) |> snd |> List.rev 
            { 
                Parameters = deepBeliefParameters;
                Machines = deepBeliefParameters.Layers |> toMachines
            }

    type Random with
        member this.NextSingle = this.NextDouble() |> float32

    let activate (rnd : Random) (FloatingPointFunction activation) x =
        let exceedsActivationThreshold threshold (Range value) = value > threshold
        x |> Domain |> activation |> exceedsActivationThreshold rnd.NextSingle |> Convert.ToInt32 |> float32

    type InputBatch with
        member this.Activate (rnd : Random) activation =
            match this with InputBatch matrix -> matrix.Map (activate rnd sigmoidFunction) |> InputBatch
        static member FromTrainingExamples (examples : Input list) =
            let h = examples.Length
            let w = examples.Head.Size
            let inputValue j (Input input) = match input.[j] with Signal signal -> signal
            Array2D.init h w (fun i j -> examples.[i] |> inputValue j) |> Matrix |> fun matrix -> matrix.PrependColumnOfOnes |> InputBatch
        static member Error (InputBatch lhs) (InputBatch rhs) =
            (lhs - rhs).SumOfSquares / (float32 lhs.Height)

    type BatchOutput with
        member this.Activate (rnd : Random) (FloatingPointFunction activation) =
            match this with BatchOutput matrix -> matrix.Map (activate rnd sigmoidFunction) |> BatchOutput
        static member Error (BatchOutput lhs) (BatchOutput rhs) =
            (lhs - rhs).SumOfSquares / (float32 lhs.Width)
    
    type BatchOutputAndInput = BatchOutputAndInput of BatchOutput * InputBatch with
        static member (*) (BatchOutputAndInput (outputBeforeTransformation, inputBeforeTransformation), BatchOutputAndInput (outputAfterTransformation, inputAfterTransformation)) = 
            let product (BatchOutput output) (InputBatch input) = output * input
            (product outputAfterTransformation inputAfterTransformation) - (product outputBeforeTransformation inputBeforeTransformation) |> WeightGradients

    type LayerInputs with
        member this.GetRandomisedInputBatches (rnd : Random) (BatchSize batchSize) =
            match this with 
                LayerInputs inputs ->
                    let batches = inputs |> List.sortBy (fun element -> rnd.NextSingle) |> batchesOf batchSize
                    batches |> List.map InputBatch.FromTrainingExamples

    type RestrictedBoltzmannMachine with
        member rbm.UpdateWeights rnd (batch : InputBatch) =
            let weightedLearningRate = rbm.Parameters.LearningRate / batch.Size
            let weightsAndBiases = rbm.ToWeightsAndBiases

            let v1 = batch
            let h1 = (weightsAndBiases.Forward v1.ActivateFirstColumn).Activate rnd sigmoidFunction
            let v2 = (weightsAndBiases.Backward h1.ActivateFirstRow).Activate rnd sigmoidFunction
            let h2 = (weightsAndBiases.Forward v2.ActivateFirstColumn).Activate rnd sigmoidFunction

            let visibleError = InputBatch.Error v1 v2
            let hiddenError = BatchOutput.Error h1 h2

            let outputAndInputBeforeTransformation = BatchOutputAndInput (h1, v1)
            let outputAndInputAfterTransformation = BatchOutputAndInput (h2, v2)
            let changes = rbm.ToWeightsAndBiasesChanges.NextChanges weightedLearningRate rbm.Parameters.Momentum (outputAndInputBeforeTransformation * outputAndInputAfterTransformation) 
            let weightsAndBiases = weightsAndBiases.Update changes
            ( 
                (visibleError, hiddenError),
                RestrictedBoltzmannMachine.FromWeightsAndBiases rbm.Parameters weightsAndBiases changes
            )
        member rbm.RunEpochCpu (rnd : Random) (inputs : LayerInputs) =
            let batches = inputs.GetRandomisedInputBatches rnd rbm.Parameters.BatchSize
            let results = batches |> List.fold(fun acc batch ->
                let result = acc |> snd |> fun (machine : RestrictedBoltzmannMachine) -> machine.UpdateWeights rnd batch
                let resultErrors = fst result
                let cumulativeErrors = fst acc
                ((fst cumulativeErrors + fst resultErrors, snd cumulativeErrors + snd resultErrors), snd result)) ((0.0f, 0.0f), rbm)
            snd results
        member rbm.NextLayerUpCpu rnd (inputs : LayerInputs) =
            let toLayerInput (BatchOutput output) =
                let width = output.Width
                let output = output.Submatrix 1 0 (output.Height - 1) output.Width
                [0..width - 1] |> List.map (fun j -> output.Column j |> Input.FromVector) |> LayerInputs
            let batch = (inputs.GetRandomisedInputBatches rnd (BatchSize 1)).Head
            (rbm.ToWeightsAndBiases.Forward batch.ActivateFirstColumn).Activate rnd sigmoidFunction |> toLayerInput
        member rbm.TrainLayerCpu rnd (inputs : LayerInputs) =
            let runEpochs (Epochs epochs) =
                [1..epochs] |> List.fold(fun (acc : RestrictedBoltzmannMachine) i -> acc.RunEpochCpu rnd inputs) rbm
            runEpochs rbm.Parameters.Epochs

    type TrainingSet with 
        member this.ToFirstLayerInput = 
            match this with TrainingSet examples -> examples |> List.map (fun example -> example.TrainingInput) |> LayerInputs

    type DeepBeliefNetwork with
        member dbn.TrainCpu rnd (TrainingSet trainingSet) =
            let firstLayerInputs = trainingSet |> List.map (fun example -> example.TrainingInput) |> LayerInputs
            let start = dbn.Machines.Head.TrainLayerCpu rnd firstLayerInputs
            {
                Parameters = dbn.Parameters;
                Machines = dbn.Machines.Tail |> List.fold(fun acc element -> 
                    let currentTuple = List.head acc
                    let rbm : RestrictedBoltzmannMachine = fst currentTuple
                    let layerInputs = snd currentTuple
                    let nextLayerUp = rbm.NextLayerUpCpu rnd layerInputs
                    let nextRbm = element.TrainLayerCpu rnd nextLayerUp
                    (nextRbm, nextLayerUp) :: acc) [(start, firstLayerInputs)]
                    |> List.map fst |> List.rev 
            }
