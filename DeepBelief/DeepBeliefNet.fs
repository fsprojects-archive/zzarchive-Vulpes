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

    type DeepBeliefNetwork with
        member this.ToBackPropagationNetwork (backPropagationParameters : BackPropagationParameters) (deepBeliefNetwork : DeepBeliefNetwork) =
            let layers = this.Machines |> List.map (fun rbm -> { Weights = WeightsAndBiases.FromRbm rbm; Activation = sigmoidActivation }) 
            { Parameters = backPropagationParameters; Layers = layers }

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
            (this.Weights.PrependColumn this.HiddenBiases).PrependRow (this.VisibleBiases.Prepend 0.0f)
            |> Matrix |> WeightsAndBiases
        member this.ToWeightsAndBiasesGradients =
            (this.DWeights.PrependColumn this.DHiddenBiases).PrependRow (this.DVisibleBiases.Prepend 0.0f)
            |> Matrix |> WeightGradients
        member this.NumberOfHiddenUnits =
            this.HiddenBiases.Length
        member this.NumberOfVisibleUnits =
            this.VisibleBiases.Length
        static member FromWeightsAndBiases (parameters : RestrictedBoltzmannParameters) (WeightsAndBiases weightsAndBiases) (WeightGradients dWeightsAndBiases) =
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
       

    let initDbn (deepBeliefParameters : DeepBeliefParameters) xInputs =
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

    let activateFirstRow (v:Matrix) = v.[1..,0..] |> prependRowOfOnes
    let activateFirstColumn (h:Matrix) = h.[0..,1..] |> prependColumnOfOnes

    let forward weightsAndBiases v = multiplyByTranspose weightsAndBiases v
    let backward weightsAndBiases h = transposeAndMultiply h weightsAndBiases

    let activate (rnd : Random) activation xInputs =
        xInputs |> mapMatrix (fun x -> activation x > float32 (rnd.NextDouble()) |> Convert.ToInt32 |> float32)

    let updateWeights rnd (rbm : RestrictedBoltzmannMachine) batch =
        let batchSize = height batch
        let weightedLearningRate = rbm.Parameters.LearningRate / batchSize
        let weightsAndBiases = toWeightsAndBiases rbm
        let dWeightsAndBiases = toDWeightsAndBiases rbm

        let v1 = batch
        let h1 = v1 |> forward weightsAndBiases  |> activate rnd sigmoidFunction |> activateFirstRow
        let v2 = h1 |> backward weightsAndBiases |> activate rnd sigmoidFunction |> activateFirstColumn
        let h2 = v2 |> forward weightsAndBiases  |> activate rnd sigmoidFunction |> activateFirstRow

        let visibleError = (subtractMatrices v1 v2 |> sumOfSquaresMatrix) / batchSize
        let hiddenError = (subtractMatrices h1 h2 |> sumOfSquaresMatrix) / batchSize

        let c1 = multiply h1 v1
        let c2 = multiply h2 v2

        let momentum = value rbm.Parameters.Momentum
        let dWeightsAndBiases = addMatrices (multiplyMatrixByScalar momentum dWeightsAndBiases) (multiplyMatrixByScalar weightedLearningRate (subtractMatrices c1 c2))
        let weightsAndBiases = addMatrices weightsAndBiases dWeightsAndBiases
        ( 
            (visibleError, hiddenError),
            toRbm rbm.Parameters weightsAndBiases dWeightsAndBiases
        )
    
    let rbmEpoch rnd (rbm : RestrictedBoltzmannMachine) xInputs =
        let xRand = permuteRows rnd xInputs
        let batchSize = value rbm.Parameters.BatchSize
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
