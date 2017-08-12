[![Issue Stats](http://issuestats.com/github/fsprojects/Vulpes/badge/issue)](http://issuestats.com/github/fsprojects/Vulpes)
[![Issue Stats](http://issuestats.com/github/fsprojects/Vulpes/badge/pr)](http://issuestats.com/github/fsprojects/Vulpes)

# Vulpes

Vulpes is an implementation of [deep belief](http://www.cs.toronto.edu/~hinton/absps/fastnc.pdf)  and [deep learning](http://www.cs.nyu.edu/~yann/research/deep/), written in F# and using [Alea.cuBase](https://www.quantalea.net/products/introduction/) to connect to your PC's GPU device.

## Building

At present, Vulpes has been built only on Visual Studio.

To run Vulpes on the [MNIST](http://yann.lecun.com/exdb/mnist/) dataset of handwritten images, set the startup project to [MnistClassification.fsproj](https://github.com/SpiegelSoft/Vulpes/blob/master/MnistClassification/MnistClassification.fsproj).

For the MNIST dataset, Vulpes performs pretraining using a deep belief net, followed by fine tuning using a simple backpropagation algorithm.

The pretraining and fine tuning parameters are defined in [Program.fs](https://github.com/SpiegelSoft/Vulpes/blob/master/MnistClassification/Program.fs):

```F#
// Pretraining parameters
let dbnParameters = 
    {
        Layers = LayerSizes [500; 300; 150; 60; 10]
        LearningRate = LearningRate 0.9f
        Momentum = Momentum 0.2f
        BatchSize = BatchSize 30
        Epochs = Epochs 10
    }
```
```F#
// Fine tuning parameters
let backPropagationParameters =
    {
        LearningRate = LearningRate 0.8f
        Momentum = Momentum 0.25f
        Epochs = Epochs 10
    }
```

The pretraining is launched by the line

```F#
let trainedMnistDbn = trainMnistDbn rand dbnParameters
```

The output of the pretraining is then translated into a set of backpropagation inputs, which are used to launch the fine tuning in the next line:

```F#
let backPropagationNetwork = toBackPropagationNetwork backPropagationParameters trainedMnistDbn
let backPropagationResults = gpuComputeNnetResults backPropagationNetwork mnistTrainingSet mnistTestSet rand backPropagationParameters
```

## Contributions

There are several avenues for further development of Vulpes.  To contribute as a developer, I would encourage you to join the [mailing list](https://groups.google.com/forum/#!forum/vulpes-developers), where we can discuss the issues and milestones.

There is a [list of milestones](https://github.com/SpiegelSoft/Vulpes/issues/milestones?with_issues=no) and an [issues database](https://github.com/SpiegelSoft/Vulpes/issues) in this repository.

## Maintainer(s)

- [@SpiegelSoft](https://github.com/SpiegelSoft)

The default maintainer account for projects under "fsprojects" is [@fsprojectsgit](https://github.com/fsprojectsgit) - F# Community Project Incubation Space (repo management)

