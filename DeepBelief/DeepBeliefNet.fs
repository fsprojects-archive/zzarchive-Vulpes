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
                Weights = weightsAndBiases.Submatrix 1 1;
                DWeights = dWeightsAndBiases.Submatrix 1 1;
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
    
    // Taken from http://www.cs.toronto.edu/~hinton/absps/guideTR.pdf, Section 8.
    // The visible bias b_i should be log (p_i/(1 - p_i)) where p_i is the propotion
    // of training vectors in which the unit i is on.
    let initVisibleBias v = 0.0f
//        let p = proportionOfVisibleUnits v
//        Math.Max(-1.0f, Math.Log(float p) |> float32) - Math.Max(-1.0f, Math.Log(1.0 - float p) |> float32)

    type DeepBeliefNetwork with
        member this.ToBackPropagationNetwork (backPropagationParameters : BackPropagationParameters) (deepBeliefNetwork : DeepBeliefNetwork) =
            let layers = this.Machines |> List.map (fun rbm -> { Weights = WeightsAndBiases.FromRbm rbm; Activation = sigmoidActivation }) 
            { Parameters = backPropagationParameters; Layers = layers }
        static member Initialise (deepBeliefParameters : DeepBeliefParameters) xInputs =
            let toMachines (LayerSizes layers) =
                layers |> List.fold(fun acc element -> 
                    let nVisible = fst acc
                    let nHidden = element
                    let rbmParams = deepBeliefParameters.ToRbmParameters
                    (element, (RestrictedBoltzmannMachine.Initialise rbmParams nVisible nHidden) :: snd acc))
                    (width xInputs, []) |> snd |> List.rev 
            { 
                Parameters = deepBeliefParameters;
                Machines = deepBeliefParameters.Layers |> toMachines
            }

    let activate (rnd : Random) (FloatingPointFunction activation) x =
        let exceedsActivationThreshold threshold (Range value) = value > threshold
        x |> Domain |> activation |> exceedsActivationThreshold (rnd.NextDouble() |> float32) |> Convert.ToInt32 |> float32

    type InputBatch with
        member this.Activate (rnd : Random) activation =
            match this with InputBatch matrix -> matrix.Map (activate rnd sigmoidFunction) |> Matrix |> InputBatch
        static member Error (InputBatch lhs) (InputBatch rhs) =
            (lhs - rhs).SumOfSquares / (float32 lhs.Height)

    type BatchOutput with
        member this.Activate (rnd : Random) (FloatingPointFunction activation) =
            match this with BatchOutput matrix -> matrix.Map (activate rnd sigmoidFunction) |> Matrix |> BatchOutput
        static member Error (BatchOutput lhs) (BatchOutput rhs) =
            (lhs - rhs).SumOfSquares / (float32 lhs.Width)
    
    type BatchOutputAndInput = BatchOutputAndInput of BatchOutput * InputBatch with
        static member (*) (BatchOutputAndInput (outputBeforeTransformation, inputBeforeTransformation), BatchOutputAndInput (outputAfterTransformation, inputAfterTransformation)) = 
            let product (BatchOutput output) (InputBatch input) = output * input
            (product outputAfterTransformation inputAfterTransformation) - (product outputBeforeTransformation inputBeforeTransformation) |> WeightGradients

    type RestrictedBoltzmannMachine with
        member rbm.UpdateWeights rnd (batch : InputBatch) =
            let activateFirstRow (BatchOutput (Matrix v)) = v.[1..,0..] |> Matrix |> fun m -> m.PrependColumnOfOnes |> BatchOutput
            let activateFirstColumn (InputBatch (Matrix h)) = h.[0..,1..] |> Matrix |> fun m -> m.PrependRowOfOnes |> InputBatch
            let weightedLearningRate = rbm.Parameters.LearningRate / batch.Size
            let weightsAndBiases = rbm.ToWeightsAndBiases

            let v1 = batch
            let h1 = (weightsAndBiases.Forward v1).Activate rnd sigmoidFunction |> activateFirstRow
            let v2 = (weightsAndBiases.Backward h1).Activate rnd sigmoidFunction |> activateFirstColumn
            let h2 = (weightsAndBiases.Forward v2).Activate rnd sigmoidFunction |> activateFirstRow

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
    
    let rbmEpoch rnd (rbm : RestrictedBoltzmannMachine) xInputs =
        let xRand = permuteRows rnd xInputs
        let batchSize = rbm.Parameters.BatchSize
        let samples = xRand |> batchesOf batchSize |> Array.map array2D
        samples |> Array.fold(fun acc batch ->
            let result = updateWeights rnd (snd acc) batch
            let resultErrors = fst result
            let cumulativeErrors = fst acc
            ((fst cumulativeErrors + fst resultErrors, snd cumulativeErrors + snd resultErrors), snd result)) ((0.0f, 0.0f), rbm)

    let cpuRbmUp rbm activation xInputs =
        forward rbm xInputs |> mapMatrix activation |> transpose

    let cpuRbmTrain rnd (rbm : RestrictedBoltzmannMachine) (xInputs : Matrix) =
        let initialisedRbm =
            {
                Parameters = rbm.Parameters;
                Weights = rbm.Weights;
                DWeights = rbm.DWeights;
                VisibleBiases = xInputs.[0..,1..] |> toColumns |> Array.map initVisibleBias;
                DVisibleBiases = rbm.DVisibleBiases
                HiddenBiases = rbm.HiddenBiases
                DHiddenBiases = rbm.DHiddenBiases
            }
        let epochs = value rbm.Parameters.Epochs
        [1..epochs] |> List.fold(fun acc i ->
            snd (rbmEpoch rnd acc xInputs)) initialisedRbm

    let cpuDbnTrain rnd (dbn : DeepBeliefNetwork) xInputs =
        let prependedInputs = xInputs |> prependColumnOfOnes
        let start = cpuRbmTrain rnd (List.head dbn.Machines) prependedInputs
        { 
            Parameters = dbn.Parameters;
            Machines = dbn.Machines.Tail |> List.fold(fun acc element -> 
                let currentTuple = List.head acc
                let x = cpuRbmUp (fst currentTuple |> toWeightsAndBiases) sigmoidFunction (snd currentTuple)
                let nextRbm = cpuRbmTrain rnd element x
                (nextRbm, x) :: acc) [(start, prependedInputs)]
                |> List.map fst |> List.rev 
        }
