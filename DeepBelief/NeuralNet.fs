// The MIT License (MIT)
// 
// Copyright (c) 2014 SpiegelSoft Ltd
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace DeepBelief

module NeuralNet =

    open System
    open Utils

    /// precision for calculating the derivatives
    let prc = 1e-6f
    
    type FloatingPointFunction = FloatingPointFunction of (float32 -> float32) with
        interface IWrappedType<float32 -> float32> with
            member this.Value = let (FloatingPointFunction f) = this in f

    type FloatingPointDerivative = FloatingPointDerivative of (float32 -> float32 -> float32) with
        interface IWrappedType<float32 -> float32 -> float32> with
            member this.Value = let (FloatingPointDerivative f) = this in f

    type DifferentiableFunction = DifferentiableFunction of (FloatingPointFunction * FloatingPointDerivative) with
        interface IWrappedType<FloatingPointFunction * FloatingPointDerivative> with
            member this.Value = let (DifferentiableFunction f) = this in f

    type Layer = {
        Weight : Matrix
        Activation : DifferentiableFunction
    }

    type NnetProperties = {
        Weights : Matrix list
        Activations : DifferentiableFunction list
    }

    type NnetInput = NnetInput of Matrix

    type NnetOutput = NnetOutput of Matrix

    type SupervisedLearning = SupervisedLearning of (NnetInput -> NnetOutput)

    let toNnetProperties weights =
        {
            Weights = weights;
            Activations = weights |> List.map (fun _ -> DifferentiableFunction (FloatingPointFunction Kernels.sigmoid, FloatingPointDerivative Kernels.dSigmoid))
        }

    /// returns list of (out, out') vectors per layer
    // Taken from Reto Matter's blog, http://retomatter.blogspot.ch/2013/01/functional-feed-forward-neural-networks.html
    let feedForward (netProps : NnetProperties) input = 
        List.fold 
            (fun (os : (Vector * Vector) list) (W, f) -> 
                let prevLayerOutput = 
                    match os.IsEmpty with
                    | true -> input
                    | _    -> fst (os.Head)
                let prevOut = prependForBias prevLayerOutput
                let layerInput = prevOut |> multiplyVectorByMatrix W
                (layerInput |> Array.map (fst f), 
                 layerInput |> Array.map (fun x -> (snd f) (x |> fst f) x)) :: os) 
          [] (List.zip netProps.Weights (netProps.Activations |> List.map (fun a -> value a |> fun f -> (value <| fst f, value <| snd f))))

    /// matlab like pointwise multiply
    let (.*) (v1 : Vector) (v2 : Vector) = 
        let n = Array.length v1
        Array.init n (fun i -> v1.[i] * v2.[i])

    /// computes the error signals per layer
    /// starting at output layer towards first hidden layer
    let cpuErrorSignals (Ws : Matrix list) layeroutputs (target : Vector) = 
        let trp = fun W -> Some(transpose W)

        // need weights and layer outputs in reverse order, 
        // e.g starting from output layer
        let weightsAndOutputs = 
            let transposed = Ws |> List.tail |> List.map trp |> List.rev
            List.zip (None :: transposed) layeroutputs

        List.fold (fun prevDs ((W : Matrix option), (o, o')) -> 
            match W with
            | None    -> (o' .* (subtractVectors target o)) :: prevDs 
            | Some(W) -> (o' .* ((multiplyVectorByMatrix W prevDs.Head)).[1..]) :: prevDs) 
          [] weightsAndOutputs

    /// computes a list of gradients matrices
    let gradients (Ws : Matrix list) layeroutputs input target = 
        let actualOuts = layeroutputs |> List.unzip |> fst |> List.tail |> List.rev
        let signals = cpuErrorSignals Ws layeroutputs target
        (input :: actualOuts, signals) 
            ||> List.zip 
            |> List.map (fun (zs, ds) -> outerProduct ds (prependForBias zs))

    /// updates the weights matrices with the given deltas 
    /// of timesteps (t) and (t-1)
    /// returns the new weights matrices
    let updateWeights Ws (Gs : Matrix list) (prevDs : Matrix list) eta alpha = 
        (List.zip3 Ws Gs prevDs) 
            |> List.map (fun (W, G, prevD) ->
                let dW = addMatrices (multiplyMatrixByScalar eta G) (multiplyMatrixByScalar alpha prevD)
                addMatrices W dW, dW)

    /// for each weight matrix builds another matrix with same dimension
    /// initialized with 0.0
    let initDeltaWeights (Ws : Matrix list) = 
        Ws |> List.map (fun W -> initGaussianWeights (height W) (width W))

    let step netProps prevDs input target eta alpha = 
        let layeroutputs = feedForward netProps input
        let Gs = gradients netProps.Weights layeroutputs input target
        (updateWeights netProps.Weights Gs prevDs eta alpha)

    let nnetTrain (rnd : Random) props samples eta alpha epochs = 
        let count = samples |> Array.length
        let Ws, fs = props.Weights, props.Activations
        let rec loop Ws Ds i =
            match i < (epochs * count) with
            | true -> 
                let input, target = samples.[rnd.Next(count)]
                let netProps = { Weights = Ws; Activations = fs }
                let ws, ds = List.unzip (step netProps Ds input target eta alpha)
                loop ws ds (i + 1)
            | _    -> Ws
        let Ws' = loop Ws (initDeltaWeights Ws) 0
        { props with Weights = Ws' }

    let netoutput (layeroutputs : ('a * 'a) list) = fst (layeroutputs.Head)

    let cpuComputeNnetResults netProps trainingSet testSet eta alpha rnd epochs = 
        let netProps' = nnetTrain rnd netProps trainingSet eta alpha epochs
        let cpuOutput = testSet |> Array.map (fun (x, t) -> netoutput (feedForward netProps' x))
        cpuOutput
