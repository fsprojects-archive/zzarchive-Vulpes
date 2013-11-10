namespace DeepBelief

module DeepBeliefNet =

    open MathNet.Numerics
    open MathNet.Numerics.Random
    open System
    open Utils

    type RestrictedBoltzmannMachine = {
        Alpha : float32
        Momentum : float32
        Weights : Matrix
        DWeights : Matrix
        VisibleBiases : Vector
        DVisibleBiases : Vector
        HiddenBiases : Vector
        DHiddenBiases : Vector
    }
    
    let sizeOfRbm (rbm : RestrictedBoltzmannMachine) =
        2 * (1 + Array.length rbm.HiddenBiases + Array.length rbm.VisibleBiases + (width rbm.Weights) * (height rbm.Weights))

    let rbmValue i alpha momentum hiddenBiases dHiddenBiases visibleBiases dVisibleBiases weights dWeights =
        let nh = Array.length hiddenBiases
        let nv = Array.length visibleBiases
        let nw = Array.length weights
        let nDh = Array.length dHiddenBiases
        let nDv = Array.length dVisibleBiases
        let nDw = Array.length dWeights
        let ph = 2 + nh
        let pDh = ph + nDh
        let pv = pDh + nv
        let pDv = pv + nDv
        let pw = pDv + nw
        let pDw = pw + nDw
        match i with
        | 0 -> alpha
        | 1 -> momentum
        | j when j < ph -> hiddenBiases.[j - 2]
        | j when j < pDh -> dHiddenBiases.[j - ph]
        | j when j < pv -> visibleBiases.[j - pDh]
        | j when j < pDv -> dVisibleBiases.[j - pv]
        | j when j < pw -> weights.[j - pDv]
        | j when j < pDw -> dWeights.[j - pw]
        | _ -> 0.0f

    let flattenRbm rbm =
        let alpha = rbm.Alpha
        let momentum = rbm.Momentum
        let hiddenBiases = rbm.HiddenBiases
        let dHiddenBiases = rbm.DHiddenBiases
        let visibleBiases = rbm.VisibleBiases
        let dVisibleBiases = rbm.DVisibleBiases
        let weights = flattenMatrix rbm.Weights
        let dWeights = flattenMatrix rbm.DWeights
        Array.init (sizeOfRbm rbm) 
            (fun i -> rbmValue i alpha momentum hiddenBiases dHiddenBiases visibleBiases dVisibleBiases weights dWeights)

    let rebuildRbm nVisible nHidden (X : Vector) =
        let nWeights = nHidden * nVisible
        let endHiddenBiases = 2 + nHidden - 1
        let endDHiddenBiases = endHiddenBiases + nHidden
        let endVisibleBiases = endDHiddenBiases + nVisible
        let endDVisibleBiases = endVisibleBiases + nVisible
        let endWeights = endDVisibleBiases + nWeights
        let endDWeights = endWeights + nWeights
        {
            Alpha = X.[0];
            Momentum = X.[1];
            HiddenBiases = X.[2..endHiddenBiases];
            DHiddenBiases = X.[(endHiddenBiases + 1)..endDHiddenBiases];
            VisibleBiases = X.[(endDHiddenBiases + 1)..endVisibleBiases];
            DVisibleBiases = X.[(endVisibleBiases + 1)..endDVisibleBiases];
            Weights = rebuildMatrix nVisible X.[(endDVisibleBiases + 1)..endWeights];
            DWeights = rebuildMatrix nVisible X.[(endWeights + 1)..endDWeights]
        }

    // Taken from http://www.cs.toronto.edu/~hinton/absps/guideTR.pdf, Section 8.
    // The visible bias b_i should be log (p_i/(1 - p_i)) where p_i is the propotion
    // of training vectors in which the unit i is on.
    let initVisibleUnit v =
        let p = proportionOfVisibleUnits v
        Math.Max(-100.0f, Math.Log(float p) |> float32) - Math.Max(-100.0f, Math.Log(1.0 - float p) |> float32)

    let initRbm nVisible nHidden alpha momentum =
        { 
            Alpha = alpha; 
            Momentum = momentum; 
            Weights = initGaussianWeights nHidden nVisible;
            DWeights = Array2D.zeroCreate nHidden nVisible;
            VisibleBiases = Array.zeroCreate nVisible;
            DVisibleBiases = Array.zeroCreate nVisible;
            HiddenBiases = Array.zeroCreate nHidden;
            DHiddenBiases = Array.zeroCreate nHidden
        }

    let dbn sizes alpha momentum xInputs =
        sizes |> List.fold(fun acc element -> 
            let nVisible = fst acc
            let nHidden = element
            (element, (initRbm nVisible nHidden alpha momentum) :: snd acc))
            (width xInputs, [])
            |> snd |> List.rev

    let forward rbm v = 
        let prependedWeights = rbm.Weights |> prependColumn rbm.HiddenBiases
        let prependedInputs = prependColumnOfOnes v
        multiplyByTranspose prependedWeights prependedInputs

    let backward rbm h = 
        let prependedWeights = rbm.Weights |> prependRow rbm.VisibleBiases
        let prependedInputs = prependRowOfOnes h
        transposeAndMultiply prependedInputs prependedWeights

    let activate (rnd : AbstractRandomNumberGenerator) activation xInputs =
        xInputs |> mapMatrix (fun x -> activation x > float32 (rnd.NextDouble()) |> Convert.ToInt32 |> float32)

    let updateWeights rnd rbm batch =
        let batchSize = float32 (height batch)
        let weightedAlpha = rbm.Alpha / batchSize

        let v1 = batch
        let h1 = v1 |> forward rbm  |> activate rnd sigmoid
        let v2 = h1 |> backward rbm |> activate rnd sigmoid
        let h2 = v2 |> forward rbm  |> activate rnd sigmoid

        let c1 = multiply h1 v1
        let c2 = multiply h2 v2

        let changeOfVisibleUnits = subtractMatrices v1 v2
        let changeOfHiddenUnits = (subtractMatrices h1 h2) |> transpose

        let visibleError = (changeOfVisibleUnits |> sumOfSquaresMatrix) / batchSize
        let hiddenError = (changeOfHiddenUnits |> sumOfSquaresMatrix) / batchSize

        let DWeights = addMatrices (multiplyMatrixByScalar rbm.Momentum rbm.DWeights) (multiplyMatrixByScalar weightedAlpha (subtractMatrices c1 c2))
        let DVisibleBiases = addVectors (multiplyVectorByScalar rbm.Momentum rbm.DVisibleBiases) (multiplyVectorByScalar weightedAlpha (sumOfRows changeOfVisibleUnits))
        let DHiddenBiases = addVectors (multiplyVectorByScalar rbm.Momentum rbm.DHiddenBiases) (multiplyVectorByScalar weightedAlpha (sumOfRows changeOfHiddenUnits))
        ( 
            visibleError,
            {
                Alpha = rbm.Alpha;
                Momentum = rbm.Momentum;
                Weights = addMatrices rbm.Weights DWeights;
                DWeights = DWeights;
                VisibleBiases = addVectors rbm.VisibleBiases DVisibleBiases;
                DVisibleBiases = DVisibleBiases;
                HiddenBiases = addVectors rbm.HiddenBiases DHiddenBiases;
                DHiddenBiases = DHiddenBiases
            }
        )
    
    let rbmEpoch rnd batchSize rbm xInputs =
        let nRows = height xInputs
        let nCols = width xInputs
        let xRand = permuteRows rnd xInputs
        let samples = xRand |> batchesOf batchSize |> Array.map array2D
        samples |> Array.fold(fun acc batch ->
            let result = updateWeights rnd (snd acc) batch
            (fst acc + fst result, snd result)) (0.0f, rbm)

    let rbmUp rbm activation xInputs =
        forward rbm xInputs |> mapMatrix activation |> transpose

    let rbmTrain rnd batchSize epochs rbm xInputs =
        let initialisedRbm =
            {
                Alpha = rbm.Alpha;
                Momentum = rbm.Momentum;
                Weights = rbm.Weights;
                DWeights = rbm.DWeights;
                VisibleBiases = xInputs |> toColumns |> Array.map initVisibleUnit;
                DVisibleBiases = rbm.DVisibleBiases
                HiddenBiases = rbm.HiddenBiases
                DHiddenBiases = rbm.DHiddenBiases
            }
        [1..epochs] |> List.fold(fun acc i ->
            snd (rbmEpoch rnd batchSize acc xInputs)) initialisedRbm

    let dbnTrain rnd batchSize epochs rbms xInputs =
        let start = rbmTrain rnd batchSize epochs (List.head rbms) xInputs
        rbms.Tail |> List.fold(fun acc element -> 
            let currentTuple = List.head acc
            let x = rbmUp (fst currentTuple) sigmoid (snd currentTuple)
            let nextRbm = rbmTrain rnd batchSize epochs element x
            (nextRbm, x) :: acc) [(start, xInputs)]
            |> List.map fst |> List.rev
