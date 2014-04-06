# Vulpes

Vulpes is an implementation of [deep belief] (http://www.cs.toronto.edu/~hinton/absps/fastnc.pdf)  and [deep learning] (http://www.cs.nyu.edu/~yann/research/deep/), written in F# and using [Alea.cuBase] (https://www.quantalea.net/products/introduction/) to connect to your PC's GPU device.

## Building

At present, Vulpes has been built only on Visual Studio.

To run Vulpes on the [MNIST] (http://yann.lecun.com/exdb/mnist/) dataset of handwritten images, set the startup project to [MnistClassification.fsproj] (https://github.com/SpiegelSoft/Vulpes/blob/master/MnistClassification/MnistClassification.fsproj).

For the MNIST dataset, Vulpes performs pretraining using a deep belief net, followed by fine tuning using a simple backpropagation algorithm.

The pretraining and fine tuning parameters are defined in [Program.fs] (https://github.com/SpiegelSoft/Vulpes/blob/master/MnistClassification/Program.fs):

```F#
    let dbnParameters = 
        {
            Layers = LayerSizes [500; 300; 150; 60; 10]
            LearningRate = LearningRate 0.9f
            Momentum = Momentum 0.2f
            BatchSize = BatchSize 30
            Epochs = Epochs 10
        }

    let backPropagationParameters =
        {
            LearningRate = LearningRate 0.8f
            Momentum = Momentum 0.25f
            Epochs = Epochs 10
        }
```


## Contributions

There are several avenues for further development of Vulpes.  To contribute as a developer, I would encounrage you to join the [mailing list] (https://groups.google.com/forum/#!forum/vulpes-developers), where we can discuss the issues and milestones.

There is a [list of milestones] (https://github.com/SpiegelSoft/Vulpes/issues/milestones?with_issues=no) and an [issues database] (https://github.com/SpiegelSoft/Vulpes/issues) in this repository.


