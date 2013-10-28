// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "../packages/MathNet.Numerics.2.6.1/lib/net40/MathNet.Numerics.dll"
#r "../packages/MathNet.Numerics.FSharp.2.6.0/lib/net40/MathNet.Numerics.FSharp.dll"

#load "NeuralNet.fs"

open DeepBelief
open MathNet.Numerics.Random
open MathNet.Numerics.Distributions
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
open NeuralNet

fsi.ShowDeclarationValues <- false

let sigmoid x = 1.0 / (1.0 + exp(-x))

/// returns the output vector from a given list of layer outputs
let netoutput (layeroutputs : ('a * 'a) list) = 
    fst (layeroutputs.Head)

/// computes the output error from a
/// given target and an actual output vector
let error (target : Vector<_>) (output : Vector<_>) =
    ((target - output) 
        |> Vector.map (fun x -> x * x) 
        |> Vector.toArray 
        |> Array.sum) / 2.0

let initWeights rows cols f =
    DenseMatrix.OfArray(Array2D.init rows cols (fun _ _ -> f())) 
        :> Matrix<float>

let targetFun = fun x -> sin (6.0 * x)

let computeResults netProps trainingset epochs = 
    let netProps' = nnetTrain netProps trainingset epochs
    let setSize = trainingset.Length

    let error = 
        trainingset 
        |> Array.fold (fun E (x, t) -> 
            let outs = feedforward netProps' x
            let En = error t (netoutput outs)
            E + En) 0.0

    let outputs = 
        [-0.5 .. 0.01 .. 0.5]
        |> List.fold (fun outs x -> 
            let layeroutputs = 
                feedforward netProps' (vector [x])
            let o = (netoutput layeroutputs).At 0
            (x,o) :: outs) []
        |> List.rev

    (error / (float setSize), outputs)

let experimentSetting() = 
    let rnd = new MersenneTwister()
    let randZ() = rnd.NextDouble() - 0.5

    let samples = 
        [| for i in 1 .. 25 -> randZ() |] 
        |> Array.map(fun x -> 
            x, targetFun(x) + 0.15 * randZ())

    let trainingSet = 
        samples 
        |> Array.map (fun (x,y) -> vector [x], vector [y])
    
    let Wih = initWeights 15 2 randZ
    let Who = initWeights 1 16 randZ
    let netProps = { Weights = [Wih; Who]; 
                     Activations = [sigmoid; tanh]}
    (samples, trainingSet, netProps)

let testRun experiment listOfEpochs =
    let samples, ts, netProps = experiment()
    let data = 
        listOfEpochs
        |> List.fold (fun acc N ->
            let error, outs = computeResults netProps ts N
            printfn "mean error after %d epochs: %f" N error
            outs :: acc) []
        |> List.rev
    
    (samples, data)

let samples, data = testRun experimentSetting [75; 500; 2000; 5000; 10000];