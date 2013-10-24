namespace DeepBelief

module DeepBeliefNet =

    open Alea.CUDA
    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions
    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic

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
            HiddenBiases = DenseVector.zeroCreate nVisible;
            DHiddenBiases = DenseVector.zeroCreate nVisible
        }

    let dbn sizes alpha momentum (xInputs : Matrix<float>) =
        sizes |> List.fold(fun acc element -> 
            let nVisible = fst acc
            let nHidden = element
            (element, (initRbm nVisible nHidden alpha momentum) :: snd acc))
            (xInputs.ColumnCount, [])
            |> snd
            |> List.rev

    let rbmTrain (rbm : RestrictedBoltzmannMachine) (xInputs : DevicePtr<float32>) (xCols : int) =
        rbm

