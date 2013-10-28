namespace DeepBelief

module Utils =

    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic

    let prepend value (vec : Vector<float>) = 
        vector [ yield! value :: (vec |> Vector.toList) ]

    let prependForBias : Vector<float> -> Vector<float> = prepend 1.0

    let toRows (M : Matrix<float>) = [0..(M.RowCount - 1)] |> List.map(fun i -> M.Row i)
    let toColumns (M : Matrix<float>) = [0..(M.ColumnCount - 1)] |> List.map(fun i -> M.Column i)

    let sigmoid x = 1.0 / (1.0 + exp(-x))

    let transpose (M : Matrix<float>) = M.Transpose()

    let prependColumn column M =
        column :: toColumns M |> DenseMatrix.ofColumnVectors :> Matrix<float>

