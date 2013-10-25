// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "../packages/Alea.cuBase.1.1.549/lib/net40/Alea.CUDA.dll"
#r "../packages/Alea.cuBase.1.1.549/lib/net40/Alea.Interop.dll"
#r "../packages/MathNet.Numerics.2.6.1/lib/net40/MathNet.Numerics.dll"
#r "../packages/MathNet.Numerics.FSharp.2.6.0/lib/net40/MathNet.Numerics.FSharp.dll"

#load "DeepBeliefNet.fs"

open DeepBelief
open DeepBeliefNet
open MathNet.Numerics.Random
open MathNet.Numerics.Distributions
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic

let batch = DenseMatrix.init 10 784 (fun i j -> (i - 50) * (j - 392) |> float)
let xInputs = DenseMatrix.zeroCreate 60000 784
let sizes = [100; 50]
let alpha = 1.0
let momentum = 0.0
let twoLayerDbn = dbn sizes alpha momentum xInputs

activate rand sigmoid batch |> Matrix.forall(fun y -> y * (y - 1.0) = 0.0);;
batch |> forward twoLayerDbn.[0] |> activate rand sigmoid |> backward twoLayerDbn.[0];;