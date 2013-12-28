namespace DeepBelief

module DeepBeliefNet =

    open MathNet.Numerics.Random
    open System
    open Utils

    type RestrictedBoltzmannMachine = {
        Weights : Matrix
        DWeights : Matrix
        VisibleBiases : Vector
        DVisibleBiases : Vector
        HiddenBiases : Vector
        DHiddenBiases : Vector
    }
    
    let toWeightsAndBiases rbm =
        let prependedVisibleBiases = rbm.VisibleBiases |> prepend 0.0f
        rbm.Weights |> prependColumn rbm.HiddenBiases |> prependRow prependedVisibleBiases
     
    let toDWeightsAndBiases rbm =
        let prependedDVisibleBiases = rbm.DVisibleBiases |> prepend 0.0f
        rbm.DWeights |> prependColumn rbm.DHiddenBiases |> prependRow prependedDVisibleBiases

    let toRbm (weightsAndBiases : Matrix) (dWeightsAndBiases : Matrix) =
        let nVisible = (width weightsAndBiases) - 1
        let nHidden = (height weightsAndBiases) - 1
        {
            Weights = weightsAndBiases.[1..nHidden, 1..nVisible];
            DWeights = dWeightsAndBiases.[1..nHidden, 1..nVisible];
            HiddenBiases = weightsAndBiases.[1..nHidden, 0..0] |> column 0;
            DHiddenBiases = dWeightsAndBiases.[1..nHidden, 0..0] |> column 0;
            VisibleBiases = weightsAndBiases.[0..0, 1..nVisible] |> row 0;
            DVisibleBiases = dWeightsAndBiases.[0..0, 1..nVisible] |> row 0;
        }

    let numberOfHiddenUnits rbm = Array.length rbm.HiddenBiases
    let numberOfVisibleUnits rbm = Array.length rbm.VisibleBiases

    // Taken from http://www.cs.toronto.edu/~hinton/absps/guideTR.pdf, Section 8.
    // The visible bias b_i should be log (p_i/(1 - p_i)) where p_i is the propotion
    // of training vectors in which the unit i is on.
    let initVisibleUnit v =
        let p = proportionOfVisibleUnits v
        Math.Max(-100.0f, Math.Log(float p) |> float32) - Math.Max(-100.0f, Math.Log(1.0 - float p) |> float32)

    let initRbm nVisible nHidden =
        { 
            Weights = initGaussianWeights nHidden nVisible;
            DWeights = Array2D.zeroCreate nHidden nVisible;
            VisibleBiases = Array.zeroCreate nVisible;
            DVisibleBiases = Array.zeroCreate nVisible;
            HiddenBiases = Array.zeroCreate nHidden;
            DHiddenBiases = Array.zeroCreate nHidden
        }

    let dbn sizes xInputs =
        sizes |> List.fold(fun acc element -> 
            let nVisible = fst acc
            let nHidden = element
            (element, (initRbm nVisible nHidden) :: snd acc))
            (width xInputs, [])
            |> snd |> List.rev

    let activateFirstRow (v:Matrix) = v.[1..,0..] |> prependRowOfOnes
    let activateFirstColumn (h:Matrix) = h.[0..,1..] |> prependColumnOfOnes

    let forward weightsAndBiases v = multiplyByTranspose weightsAndBiases v
    let backward weightsAndBiases h = transposeAndMultiply h weightsAndBiases

    let activate (rnd : AbstractRandomNumberGenerator) activation xInputs =
        xInputs |> mapMatrix (fun x -> activation x > float32 (rnd.NextDouble()) |> Convert.ToInt32 |> float32)

    let updateWeights rnd alpha momentum rbm batch =
        let batchSize = float32 (height batch)
        let weightedAlpha = alpha / batchSize
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

        let dWeightsAndBiases = addMatrices (multiplyMatrixByScalar momentum dWeightsAndBiases) (multiplyMatrixByScalar weightedAlpha (subtractMatrices c1 c2))
        let weightsAndBiases = addMatrices weightsAndBiases dWeightsAndBiases
        ( 
            (visibleError, hiddenError),
            toRbm weightsAndBiases dWeightsAndBiases
        )
    
    let rbmEpoch rnd alpha momentum batchSize rbm xInputs =
        let xRand = permuteRows rnd xInputs
        let samples = xRand |> batchesOf batchSize |> Array.map array2D
        samples |> Array.fold(fun acc batch ->
            let result = updateWeights rnd alpha momentum (snd acc) batch
            let resultErrors = fst result
            let cumulativeErrors = fst acc
            ((fst cumulativeErrors + fst resultErrors, snd cumulativeErrors + snd resultErrors), snd result)) ((0.0f, 0.0f), rbm)

    let cpuRbmUp rbm activation xInputs =
        forward rbm xInputs |> mapMatrix activation |> transpose

    let cpuRbmTrain rnd alpha momentum batchSize epochs rbm (xInputs : Matrix) =
        let initialisedRbm =
            {
                Weights = rbm.Weights;
                DWeights = rbm.DWeights;
                VisibleBiases = xInputs.[0..,1..] |> toColumns |> Array.map initVisibleUnit;
                DVisibleBiases = rbm.DVisibleBiases
                HiddenBiases = rbm.HiddenBiases
                DHiddenBiases = rbm.DHiddenBiases
            }
        [1..epochs] |> List.fold(fun acc i ->
            snd (rbmEpoch rnd alpha momentum batchSize acc xInputs)) initialisedRbm

    let cpuDbnTrain rnd alpha momentum batchSize epochs rbms xInputs =
        let prependedInputs = xInputs |> prependColumnOfOnes
        let start = cpuRbmTrain rnd alpha momentum batchSize epochs (List.head rbms) prependedInputs
        rbms.Tail |> List.fold(fun acc element -> 
            let currentTuple = List.head acc
            let x = cpuRbmUp (fst currentTuple |> toWeightsAndBiases) sigmoidFunction (snd currentTuple)
            let nextRbm = cpuRbmTrain rnd alpha momentum batchSize epochs element x
            (nextRbm, x) :: acc) [(start, prependedInputs)]
            |> List.map fst |> List.rev
