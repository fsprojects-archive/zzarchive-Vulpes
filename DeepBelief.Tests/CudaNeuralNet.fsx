#r "../packages/Alea.cuBase.1.2.740/lib/net40/Alea.CUDA.dll"
#r "../packages/FsUnit.xUnit.1.2.2.1/lib/net40/FsUnit.XUnit.dll"
#r "../packages/FsUnit.xUnit.1.2.2.1/lib/net40/NHamcrest.dll"
#r "../packages/xunit.1.9.2/lib/net20/xunit.dll"
#r "../packages/MathNet.Numerics.2.6.2/lib/net40/MathNet.Numerics.dll"
#r "../packages/MathNet.Numerics.FSharp.2.6.0/lib/net40/MathNet.Numerics.FSharp.dll"
#r "../packages/FSharp.Charting.0.90.6/lib/net40/FSharp.Charting.dll"

#load "..\DeepBelief\Utils.fs"
#load "..\DeepBelief\Data.fs"
#load "..\DeepBelief\DeepBeliefNet.fs"
#load "..\DeepBelief\Kernels.fs"
#load "..\DeepBelief\NeuralNet.fs"
#load "..\DeepBelief\CudaTemplates.fs"
#load "..\DeepBelief\CudaNeuralNet.fs"

#load "TestUtils.fs"
#load "CudaNeuralNetTests.fs"

open FSharp.Charting
