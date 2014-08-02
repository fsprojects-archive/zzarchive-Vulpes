namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief
open DeepBeliefNet
open Backpropagation.Parameters
open Common.Analytics
open Common.NeuralNet
open CudaDeepBeliefNet
open System
open Utils
open TestUtils

type ``Deep Belief Network with four layers and 1000 samples running on GPU`` ()=
    let sin x = Math.Sin (float x) |> float32

    let rand = new Random()
    let xInputs = Array2D.init 1000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let dbnParameters = 
        {
            Layers = LayerSizes [500; 250; 100; 50]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 10
            Epochs = Epochs 1
        }

    let xInputs = Array2D.init 1000 784 (fun _ _ -> rand.NextDouble() |> float32)
    let xInput (rnd : Random) = Array.init 784 (fun _ -> rnd.NextDouble() |> float32 |> Signal) |> Input
    let xTarget = Array.init 10 (fun _ -> 0.0f |> float32 |> Signal) |> Target
    let trainingSet = List.init 1000 (fun _ -> { TrainingInput = xInput rand; TrainingTarget = xTarget }) |> TrainingSet
    let layeredDbn = DeepBeliefNetwork.Initialise dbnParameters trainingSet

    let weightsAndBiasesMatch (WeightsAndBiases lhs) (WeightsAndBiases rhs) =
        matricesMatch lhs rhs

    let weightChangesMatch (WeightChanges lhs) (WeightChanges rhs) =
        matricesMatch lhs rhs

    let restrictedBoltzmannMachinesMatch (lhs : RestrictedBoltzmannMachine) (rhs : RestrictedBoltzmannMachine) =
        if weightChangesMatch lhs.ToWeightsAndBiasesChanges rhs.ToWeightsAndBiasesChanges && weightsAndBiasesMatch lhs.ToWeightsAndBiases rhs.ToWeightsAndBiases
        then 0
        else 1

    let networksMatch (lhs : DeepBeliefNetwork) (rhs : DeepBeliefNetwork) =
        Seq.compareWith restrictedBoltzmannMachinesMatch lhs.Machines rhs.Machines

    [<Fact>] member test.
        ``The CPU and GPU outputs of a single DBN epoch match.``()=
        let cpuTrainedDbn = layeredDbn.TrainCpu (new Random(0)) trainingSet in
        let gpuTrainedDbn = layeredDbn.TrainGpu (new Random(0)) trainingSet in
        networksMatch cpuTrainedDbn gpuTrainedDbn
