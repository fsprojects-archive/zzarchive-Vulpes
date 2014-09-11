namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief
open DeepBeliefNet
open Backpropagation.Parameters
open Common.Analytics
open Common.NeuralNet
open Common.Utils
open CudaDeepBeliefNet
open System
open Utils
open TestUtils

type RbmTestInputs() =
    let xInputs n = Array.init 784 (fun x -> 
        let nx = 2.0 * Math.PI * float x * float (n % 10) / 784.0
        Math.Sin nx |> float32 |> Signal) |> Input
    let dbnParameters = 
        {
            Layers = LayerSizes [500; 250; 100; 50]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 100
            Epochs = Epochs 1
        }

    let xTarget n = Array.init 10 (fun i -> (if i = n then 1.0f else 0.0f) |> float32 |> Signal) |> Target
    let weightsAndBiasesMatch (WeightsAndBiases lhs) (WeightsAndBiases rhs) = matricesMatch lhs rhs
    let weightChangesMatch (WeightChanges lhs) (WeightChanges rhs) = matricesMatch lhs rhs

    member this.TrainingSet = List.init 1000 (fun n -> { TrainingInput = xInputs (n % 10); TrainingTarget = xTarget n }) |> TrainingSet
    member this.LayeredDbn = DeepBeliefNetwork.Initialise dbnParameters this.TrainingSet
    member this.CompareRbms (lhs : RestrictedBoltzmannMachine) (rhs : RestrictedBoltzmannMachine) =
        if weightChangesMatch lhs.ToWeightsAndBiasesChanges rhs.ToWeightsAndBiasesChanges && weightsAndBiasesMatch lhs.ToWeightsAndBiases rhs.ToWeightsAndBiases then 0 else 1
    member this.CompareDbns (lhs : DeepBeliefNetwork) (rhs : DeepBeliefNetwork) = Seq.compareWith this.CompareRbms lhs.Machines rhs.Machines

type ``Deep Belief Network with four layers and 1000 samples running on GPU`` ()=
    let testInputs = new RbmTestInputs()

    [<Fact>] member test.
        ``The CPU and GPU outputs of a single DBN epoch match.``()=
        let cpuTrainedDbn = testInputs.LayeredDbn.TrainCpu (new RandomSingle(0)) testInputs.TrainingSet in
        let gpuTrainedDbn = testInputs.LayeredDbn.TrainGpu (new RandomSingle(0)) testInputs.TrainingSet in
        testInputs.CompareDbns cpuTrainedDbn gpuTrainedDbn |> should equal 0

type ``Restricted Boltzmann Machine with 784 visible units and 500 hidden units``()=
    let testInputs = new RbmTestInputs()

    [<Fact>] member test.
        ``The CPU and GPU outputs of a single RBM epoch match.``()=
        let firstLayerInput = testInputs.TrainingSet.ToFirstLayerInput in
        let rnd0 = new RandomSingle(0) in
        let rnd1 = new RandomSingle(0) in
        let cpuTrainedRbm = testInputs.LayeredDbn.Machines.Head.TrainLayerCpu (rnd0) firstLayerInput in
        let gpuTrainedRbm = testInputs.LayeredDbn.Machines.Head.TrainLayerCpu (rnd1) firstLayerInput in
        testInputs.CompareRbms cpuTrainedRbm gpuTrainedRbm |> should equal 0
