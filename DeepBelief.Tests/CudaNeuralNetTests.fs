namespace DeepBelief.Tests

module CudaNeuralNetTests =

    open CudaCommon
    open Alea.CUDA
    open Alea.CUDA.Utilities
    open Xunit
    open Xunit.Extensions
    open FsUnit.Xunit
    open Backpropagation.CudaTemplates
    open Backpropagation.Parameters
    open Common.Analytics
    open Common.NeuralNet
    open DeepBelief.CudaTemplates
    open DeepBelief.DeepBeliefNet
    open DeepBelief.Utils
    open DeepBelief.Kernels
    open TestUtils
    open System

    let dbnParameters = 
        {
            Layers = LayerSizes [500; 300; 100; 10]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 10
            Epochs = Epochs 2
        }

    let backPropagationParameters =
        {
            LearningRate = LearningRate 0.8f
            Momentum = Momentum 0.25f
            Epochs = Epochs 1
            BatchSize = BatchSize 10
        }

    type ``CUDA Neural Net: Sigmoid``()=
    
        let vector = [|0.1f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.8f; 0.9f|]
        let restrictedVector = [|0.0f; 0.2f; 0.3f; 0.4f; 0.5f; 0.7f; 0.0f; 0.0f|]
        let logitVector = vector |> Array.map logitFunction |> Vector
    
        let mod10Plus1 i = 1 + i % 10
        let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
        let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
        let trainingSet = [|1..50|] |> Array.map (fun i -> (sineCurve i, label i))
        let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))

        let outputsMatch result =
            arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

        let levelResultsMatch results =
            List.forall (fun result -> outputsMatch result) results

        let resultsMatch cpu gpu =
            List.zip cpu gpu |> levelResultsMatch

        let cpuRand = new Random()

        let gpuRand = new Random()

        [<Theory>]
        [<InlineData(1)>]
        [<InlineData(2)>]
        [<InlineData(32)>]
        member test.``The sigmoid program maps the logit vector to the original vector.``(i)=
            use sigmoidProgram = i |> sigmoidTemplate |> Compiler.load Worker.Default in
            sigmoidProgram.Run logitVector 0 8 |> should equal vector

//    type ``CUDA Neural Net: Feed Forward``()=
//
//        let alpha = 0.5f
//        let momentum = 0.9f
//        let rand = new Random()
//        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32 |> Signal) |> Input
//        let xTarget = Array.init 10 (fun _ -> 0.0f |> float32 |> Signal) |> Target
//        let trainingSet = [{ TrainingInput = xInput; TrainingTarget = xTarget }] |> TrainingSet
//        let layeredDbn = DeepBeliefNetwork.Initialise dbnParameters trainingSet
//        let network = 
//            {
//                Parameters = backPropagationParameters
//                Layers = layeredDbn.Machines |> List.map (fun rbm -> { Weight = prependColumn rbm.HiddenBiases rbm.Weights; Activation = DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid) })
//            }
//
//        let outputsMatch result =
//            arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))
//
//        let levelResultsMatch results =
//            List.forall (fun result -> outputsMatch result) results
//
//        let resultsMatch cpu gpu =
//            List.zip cpu gpu |> levelResultsMatch
//
//        let cpuRand = new Random()
//
//        let gpuRand = new Random()
//
//        [<Theory>]
//        [<InlineData(1)>]
//        [<InlineData(2)>]
//        [<InlineData(32)>]
//        let ``The feedForward GPU program matches the outputs of the feedForward CPU function.``(i)=
//            use feedForwardProgram = i |> feedForwardTemplate |> Compiler.load Worker.Default in
//            resultsMatch (feedForward network xInput |> List.rev) ((feedForwardProgram.Run network gpuInputs).[0]) |> should equal true
//
//
//    type ``CUDA Neural Net: Compute Results``()=
//
//        let rand = new Random()
//        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
//        let gpuInputs = [| (xInput |> prependForBias, None) |]
//        let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
//        let layeredDbn = initDbn dbnParameters xInputs
//        let network = 
//            {
//                Parameters = backPropagationParameters
//                Layers = layeredDbn.Machines |> List.map (fun rbm -> { Weights = prependColumn rbm.HiddenBiases rbm.Weights; Activation = DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid) })
//            }
//    
//        let mod10Plus1 i = 1 + i % 10
//        let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
//        let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
//        let trainingSet = [|1..50|] |> Array.map (fun i -> (sineCurve i, label i))
//        let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))
//
//        let outputsMatch result =
//            arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))
//
//        let levelResultsMatch results =
//            List.forall (fun result -> outputsMatch result) results
//
//        let resultsMatch cpu gpu =
//            List.zip cpu gpu |> levelResultsMatch
//
//        let cpuRand = new Random()
//
//        let gpuRand = new Random()
//
//        let parameters =
//            {
//                LearningRate = LearningRate 0.8f
//                Momentum = Momentum 0.25f
//                Epochs = Epochs 1
//                BatchSize = BatchSize 10
//            }
//
//        [<Fact>] member test.
//            ``The gpuComputeNnetResults function generates the same output as the cpuComputeNnetResults function.``()=
//                let cpuOutput = cpuComputeNnetResults network trainingSet testSet cpuRand parameters in
//                let gpuOutput = gpuComputeNnetResults network trainingSet testSet gpuRand parameters in
//                for pair in Array.zip cpuOutput gpuOutput do
//                    arraysMatch (fst pair) (snd pair) |> should equal true
//
//
//    type ``CUDA Neural Net: Error Signals``()=
//
//        let rand = new Random()
//        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
//        let gpuInputs = [| (xInput |> prependForBias, None) |]
//        let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
//        let layeredDbn = initDbn dbnParameters xInputs
//        let network = 
//            {
//                Parameters = backPropagationParameters
//                Layers = layeredDbn.Machines |> List.map (fun rbm -> { Weight = prependColumn rbm.HiddenBiases rbm.Weights; Activation = DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid) })
//            }
//    
//        let mod10Plus1 i = 1 + i % 10
//        let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
//        let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
//        let trainingSet = [|1..50|] |> Array.map (fun i -> (sineCurve i, label i))
//        let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))
//
//        [<Theory>]
//        [<InlineData(1)>]
//        [<InlineData(2)>]
//        [<InlineData(32)>]
//        member test.``The errorSignals GPU template generates the same output as the CPU errorSignals function.``(i)=
//            use feedForwardProgram = 32 |> feedForwardTemplate |> Compiler.load Worker.Default in
//            let layerOutputs = (feedForwardProgram.Run network gpuInputs).[0] |> List.rev in
//
//            use errorSignalsProgram = i |> errorSignalsTemplate |> Compiler.load Worker.Default in
//            let gpuOutput = errorSignalsProgram.Run network layerOutputs (snd trainingSet.[0]) in
//            let cpuOutput = cpuErrorSignals network layerOutputs (snd trainingSet.[0])
//            for pair in List.zip cpuOutput gpuOutput |> List.rev do
//                arraysMatch (fst pair) (snd pair) |> should equal true
//
//
//    type ``CUDA Neural Net: Gradients``()=
//
//        let rand = new Random()
//        let xInput = Array.init 784 (fun _ -> rand.NextDouble() |> float32)
//        let gpuInputs = [| (xInput |> prependForBias, None) |]
//        let xInputs = Array2D.init 1 784 (fun _ i -> xInput.[i])
//        let layeredDbn = initDbn dbnParameters xInputs
//        let network = 
//            {
//                Parameters = backPropagationParameters
//                Layers = layeredDbn.Machines |> List.map (fun rbm -> { Weight = prependColumn rbm.HiddenBiases rbm.Weights; Activation = DifferentiableFunction (FloatingPointFunction sigmoid, FloatingPointDerivative dSigmoid) })
//            }
//    
//        let mod10Plus1 i = 1 + i % 10
//        let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
//        let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
//        let trainingSet = [|1..50|] |> Array.map (fun i -> (sineCurve i, label i))
//        let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))
//
//        [<Theory>]
//        [<InlineData(1)>]
//        [<InlineData(2)>]
//        [<InlineData(32)>]
//        member test.``The gradients GPU template generates the same output as the CPU gradients function.``(i)=
//            use feedForwardProgram = 32 |> feedForwardTemplate |> Compiler.load Worker.Default in
//            let layerOutputs = (feedForwardProgram.Run network gpuInputs).[0] |> List.rev in
//
//            use gradientsProgram = i |> gradientsTemplate |> Compiler.load Worker.Default in
//            let gpuOutput = gradientsProgram.Run network layerOutputs (fst trainingSet.[0]) (snd trainingSet.[0]) in
//            let cpuOutput = cpuGradients network layerOutputs (fst trainingSet.[0]) (snd trainingSet.[0])
//            for pair in List.zip cpuOutput gpuOutput |> List.rev do
//                fst pair |> should equal <| snd pair
