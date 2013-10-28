// Learn more about F# at http://fsharp.net
// See the 'F# Tutorial' project for more help.
namespace MnistClassification

module Main =

    open TestRun
    open DeepBelief
    open DeepBeliefNet
    open MathNet.Numerics.LinearAlgebra.Double
    open MathNet.Numerics.LinearAlgebra.Generic
    open NeuralNet
    open Utils
    open MnistDataLoad

    [<EntryPoint>]
    let main argv = 
        printfn "%A" (computeResults rand props trainingSet testSet 10)
        0 // return an integer exit code
