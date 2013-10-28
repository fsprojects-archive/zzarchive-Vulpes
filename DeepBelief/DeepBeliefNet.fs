namespace DeepBelief

module DeepBeliefNet =

    open MathNet.Numerics
    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions
    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic
    open System
    open Utils

    type RestrictedBoltzmannMachine = {
        Alpha : float
        Momentum : float
        Weights : Matrix<float>
        DWeights : Matrix<float>
        VisibleBiases : Vector<float>
        DVisibleBiases : Vector<float>
        HiddenBiases : Vector<float>
        DHiddenBiases : Vector<float>
    }

    let rand = new MersenneTwister()

    let proportionOfVisibleUnits (v : Vector<float>) =
        v.ToArray() |> Array.filter (fun u -> u > 0.5) |> fun arr -> float arr.Length / float v.Count
        
    // Taken from http://www.cs.toronto.edu/~hinton/absps/guideTR.pdf, Section 8.
    // The visible bias b_i should be log (p_i/(1 - p_i)) where p_i is the propotion
    // of training vectors in which the unit i is on.
    let initVisibleUnit v =
        let p = proportionOfVisibleUnits v
        Math.Max(-100.0, Math.Log(p)) - Math.Max(-100.0, Math.Log(1.0 - p))
        
    // Taken from http://www.cs.toronto.edu/~hinton/absps/guideTR.pdf, Section 8.
    // The initial weights should have zero mean and 0.01 standard deviation.
    let gaussianDistribution = new Normal(0.0, 0.01)
    let initRbm nVisible nHidden alpha momentum =
        { 
            Alpha = alpha; 
            Momentum = momentum; 
            Weights = DenseMatrix.randomCreate nHidden nVisible gaussianDistribution;
            DWeights = DenseMatrix.zeroCreate nHidden nVisible;
            VisibleBiases = DenseVector.zeroCreate nVisible;
            DVisibleBiases = DenseVector.zeroCreate nVisible;
            HiddenBiases = DenseVector.zeroCreate nHidden;
            DHiddenBiases = DenseVector.zeroCreate nHidden
        }

    let dbn sizes alpha momentum (xInputs : Matrix<float>) =
        sizes |> List.fold(fun acc element -> 
            let nVisible = fst acc
            let nHidden = element
            (element, (initRbm nVisible nHidden alpha momentum) :: snd acc))
            (xInputs.ColumnCount, [])
            |> snd |> List.rev

    let addHiddenBiases rbm = Matrix.mapCols (fun _ col -> col + rbm.HiddenBiases)
    let addVisibleBiases rbm = Matrix.mapRows (fun _ row -> row + rbm.VisibleBiases)

    let forward rbm v = rbm.Weights * (transpose v) |> addHiddenBiases rbm |> transpose
    let backward rbm h = h * rbm.Weights |> addVisibleBiases rbm

    let sumOfRows M = M |> Matrix.sumRowsBy (fun _ row -> row)
    let sumOfSquares M = M |> Matrix.fold (fun acc element -> acc + element * element) 0.0

    let activate (rnd : AbstractRandomNumberGenerator) activation xInputs =
        xInputs |> Matrix.map(fun x -> activation x > rnd.NextDouble() |> Convert.ToInt32 |> float)

    let permutation (rnd : AbstractRandomNumberGenerator) list =
        list |> List.sortBy (fun element -> rnd.NextDouble())
    let permute rnd n = permutation rnd [0..(n-1)]
    let permuteRows rnd (M : Matrix<float>) = 
        permute rnd M.RowCount |> List.map (fun i -> M.Row i) |> DenseMatrix.ofRowVectors

    let updateWeights rnd rbm (batch : Matrix<float>) =
        let batchSize = float batch.RowCount
        let weightedAlpha = rbm.Alpha / batchSize

        let v1 = batch
        let h1 = v1 |> forward rbm  |> activate rnd sigmoid
        let v2 = h1 |> backward rbm |> activate rnd sigmoid
        let h2 = v2 |> forward rbm  |> activate rnd sigmoid

        let c1 = (transpose h1) * v1
        let c2 = (transpose h2) * v2

        let changeOfVisibleUnits = v1 - v2
        let changeOfHiddenUnits = h1 - h2
        let visibleError = (changeOfVisibleUnits |> sumOfSquares) / batchSize
        let hiddenError = (changeOfHiddenUnits |> sumOfSquares) / batchSize

        let DWeights = rbm.Momentum * rbm.DWeights + weightedAlpha * (c1 - c2)
        let DVisibleBiases = rbm.Momentum * rbm.DVisibleBiases + weightedAlpha * (sumOfRows changeOfVisibleUnits)
        let DHiddenBiases = rbm.Momentum * rbm.DHiddenBiases + weightedAlpha * (sumOfRows changeOfHiddenUnits)
        ( 
            visibleError,
            {
                Alpha = rbm.Alpha;
                Momentum = rbm.Momentum;
                Weights = rbm.Weights + DWeights;
                DWeights = DWeights;
                VisibleBiases = rbm.VisibleBiases + DVisibleBiases;
                DVisibleBiases = DVisibleBiases;
                HiddenBiases = rbm.HiddenBiases + DHiddenBiases;
                DHiddenBiases = DHiddenBiases
            }
        )

    let batchesOf n =
        Seq.ofList >> Seq.mapi (fun i v -> i / n, v) >>
        Seq.groupBy fst >> Seq.map snd >>
        Seq.map (Seq.map snd >> Seq.toList) >> Seq.toList
    
    let epoch rnd batchSize rbm xInputs =
        let xRand = permuteRows rnd xInputs
        let samples = xRand |> toRows |> batchesOf batchSize |> List.map (fun rows -> DenseMatrix.ofRowVectors rows)
        samples |> List.fold(fun acc batch ->
            let result = updateWeights rnd (snd acc) batch
            (fst acc + fst result, snd result)) (0.0, rbm)

    let rbmUp rbm activation xInputs =
        forward rbm xInputs |> Matrix.map activation

    let rbmTrain rnd batchSize epochs rbm xInputs =
        let initialisedRbm =
            {
                Alpha = rbm.Alpha;
                Momentum = rbm.Momentum;
                Weights = rbm.Weights;
                DWeights = rbm.DWeights;
                VisibleBiases = xInputs |> toColumns |> List.map initVisibleUnit |> vector;
                DVisibleBiases = rbm.DVisibleBiases
                HiddenBiases = rbm.HiddenBiases
                DHiddenBiases = rbm.DHiddenBiases
            }
        [1..epochs] |> List.fold(fun acc i ->
            snd (epoch rnd batchSize acc xInputs)) initialisedRbm

    let dbnTrain rnd batchSize epochs rbms xInputs =
        let start = rbmTrain rnd batchSize epochs (List.head rbms) xInputs
        rbms.Tail |> List.fold(fun acc element -> 
            let currentTuple = List.head acc
            let x = rbmUp (fst currentTuple) sigmoid (snd currentTuple)
            let nextRbm = rbmTrain rnd batchSize epochs element x
            (nextRbm, x) :: acc) [(start, xInputs)]
            |> List.map fst |> List.rev
