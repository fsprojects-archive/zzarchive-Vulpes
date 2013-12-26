// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.

#r "../packages/MathNet.Numerics.2.6.2/lib/net40/MathNet.Numerics.dll"
#r "../packages/MathNet.Numerics.FSharp.2.6.0/lib/net40/MathNet.Numerics.FSharp.dll"
#r "../packages/Alea.cuBase.1.2.723/lib/net40/Alea.CUDA.dll"

#load "Utils.fs"
#load "NeuralNet.fs"

open DeepBelief
open MathNet.Numerics.Random
open MathNet.Numerics.Distributions
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
open NeuralNet

fsi.ShowDeclarationValues <- false

let sigmoid x = 1.0f / (1.0f + exp(-x))

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
    Array2D.init rows cols (fun _ _ -> f())

let targetFun = fun x -> sin (6.0f * x)

let computeResults netProps trainingset epochs = 
    let netProps' = nnetTrain netProps trainingset epochs
    let setSize = trainingset.Weights.Length

    let error = 
        trainingset 
        |> Array.fold (fun E (x, t) -> 
            let outs = feedForward netProps' x
            let En = error t (netoutput outs)
            E + En) 0.0f

    let outputs = 
        [-0.5f .. 0.01f .. 0.5f]
        |> List.fold (fun outs x -> 
            let layeroutputs = 
                feedForward netProps' (vector [x])
            let o = (netoutput layeroutputs).At 0
            (x,o) :: outs) []
        |> List.rev

    (error / (float setSize), outputs)

let experimentSetting() = 
    let rnd = new MersenneTwister()
    let randZ() = (rnd.NextDouble() |> float32) - 0.5f

    let samples = 
        [| for i in 1 .. 25 -> randZ() |] 
        |> Array.map(fun x -> 
            x, targetFun(x) + 0.15f * randZ())

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