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
    let dbnParameters = 
        {
            Layers = LayerSizes [500; 250; 100; 50]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 100
            Epochs = Epochs 1
        }

    let xInput n = Array.init 784 (fun x -> 
        let nx = 2.0 * Math.PI * float x * float (n % 10) / 784.0
        Math.Sin nx |> float32 |> Signal) |> Input
    let xTarget n = Array.init 10 (fun i -> (if i = n then 1.0f else 0.0f) |> float32 |> Signal) |> Target
    let trainingSet = List.init 1000 (fun n -> { TrainingInput = xInput n; TrainingTarget = xTarget n }) |> TrainingSet
    let layeredDbn = DeepBeliefNetwork.Initialise dbnParameters trainingSet

    let weightsAndBiasesMatch (WeightsAndBiases lhs) (WeightsAndBiases rhs) =
        matricesMatch lhs rhs

    let weightChangesMatch (WeightChanges lhs) (WeightChanges rhs) =
        matricesMatch lhs rhs

    let restrictedBoltzmannMachinesMatch (lhs : RestrictedBoltzmannMachine) (rhs : RestrictedBoltzmannMachine) =
        if weightChangesMatch lhs.ToWeightsAndBiasesChanges rhs.ToWeightsAndBiasesChanges && weightsAndBiasesMatch lhs.ToWeightsAndBiases rhs.ToWeightsAndBiases
        then 0
        else 1

    let compareNetworks (lhs : DeepBeliefNetwork) (rhs : DeepBeliefNetwork) =
        Seq.compareWith restrictedBoltzmannMachinesMatch lhs.Machines rhs.Machines

    [<Fact>] member test.
        ``The CPU and GPU outputs of a single DBN epoch match.``()=
        let cpuTrainedDbn = layeredDbn.TrainCpu (new Random(0)) trainingSet in
        let gpuTrainedDbn = layeredDbn.TrainGpu (new Random(0)) trainingSet in
        compareNetworks cpuTrainedDbn gpuTrainedDbn |> should equal 0
