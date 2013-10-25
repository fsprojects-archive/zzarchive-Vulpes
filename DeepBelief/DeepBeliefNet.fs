namespace DeepBelief

module DeepBeliefNet =

    open Alea.CUDA
    open MathNet.Numerics
    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions
    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic
    open System

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

    let sigmoid x = 1.0 / (1.0 + exp(-x))

    let rand = new MersenneTwister()

    let dbnTrain trainLayer stepUp (rbms : List<'a>) xInputs =
        let start = trainLayer (rbms.Head, xInputs)
        rbms.Tail |> List.fold(fun (acc : List<'b * 'c>) element -> 
            let currentTuple = acc.Head
            let x = stepUp (fst currentTuple) (snd currentTuple)
            let nextRbm = trainLayer (element, x)
            (nextRbm, x) :: acc) [(start, xInputs)]
            |> List.rev

    let initRbm nVisible nHidden alpha momentum =
        { 
            Alpha = alpha; 
            Momentum = momentum; 
            Weights = DenseMatrix.zeroCreate nHidden nVisible;
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
            |> snd
            |> List.rev

    let rbmTrain rbm (xInputs : Matrix<float>) batchSize epochs =
        let numBatches = xInputs.RowCount / batchSize
        rbm

    let transpose (M : Matrix<float>) = M.Transpose()

    let addHiddenBiases rbm =
        Matrix.mapCols (fun _ col -> col + rbm.HiddenBiases)

    let addVisibleBiases rbm =
        Matrix.mapRows (fun _ row -> row + rbm.VisibleBiases)

    let forward rbm v =
        rbm.Weights * (transpose v) |> addHiddenBiases rbm |> transpose

    let backward rbm h =
        h * rbm.Weights |> addVisibleBiases rbm

    let sumOfRows M =
        M |> Matrix.sumRowsBy (fun _ row -> row)

    let sumOfSquares M =
        M |> Matrix.fold (fun acc element -> acc + element * element) 0.0

    let activate (rnd : AbstractRandomNumberGenerator) activation xInputs =
        xInputs |> Matrix.map(fun x -> activation x > rnd.NextDouble() |> Convert.ToInt32 |> float)

    let rec permutation (rnd : AbstractRandomNumberGenerator) list =
        let indexedList = list |> List.mapi (fun i element -> (i, element))
        match list with
        | [] -> []
        | _ ->
            let n = list.Length
            let index = rnd.Next n
            list.[index] :: permutation rnd (indexedList 
                |> List.filter (fun t -> fst t <> index)
                |> List.map (fun t -> snd t))

    let permute rnd n = permutation rnd [0..(n-1)]

    let permuteRows rnd (M : Matrix<float>) = 
        let rows = permute rnd M.RowCount |> List.map (fun i -> M.Row i)
        DenseMatrix.ofRowVectors rows

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

        let DWeights = rbm.Momentum * rbm.DWeights + weightedAlpha * (c1 - c2)
        let DVisibleBiases = rbm.Momentum * rbm.DVisibleBiases + weightedAlpha * (sumOfRows changeOfVisibleUnits)
        let DHiddenBiases = rbm.Momentum * rbm.DHiddenBiases + weightedAlpha * (sumOfRows changeOfHiddenUnits)
        ( 
            (changeOfVisibleUnits |> sumOfSquares) / batchSize,
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

    let epoch rnd xInputs =
        let xRand = permuteRows rnd xInputs
        xRand