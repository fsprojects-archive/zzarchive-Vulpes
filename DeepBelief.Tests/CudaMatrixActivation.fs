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
namespace DeepBelief.Tests

open Alea.CUDA
open Alea.CUDA.Utilities
open Xunit
open Xunit.Extensions
open FsUnit.Xunit
open DeepBelief.CudaTemplates
open DeepBelief.Utils
open DeepBelief.Kernels

type ``CUDA Matrix Activation``()=
    
    let A2By2 = array2D [ [0.1f; 0.2f];
                          [0.3f; 0.4f] ]
                          |> mapMatrix logitFunction
    let rnd2By2 = array2D [ [0.05f; 0.25f];
                            [0.42f; 0.38f] ]
    let res2By2 = array2D [ [1.0f; 0.0f];
                            [0.0f; 1.0f] ]

    let A2By4 = array2D [ [0.1f; 0.2f; 0.3f; 0.4f];
                          [0.5f; 0.6f; 0.7f; 0.8f] ]
                          |> mapMatrix logitFunction
    let rnd2By4 = array2D [ [0.05f; 0.67f; 0.12f; 0.75f];
                            [0.95f; 0.37f; 0.65f; 0.12f] ]
    let res2By4 = array2D [ [1.0f; 0.0f; 1.0f; 0.0f];
                            [0.0f; 1.0f; 1.0f; 1.0f] ]

    let A4By2 = array2D [ [0.1f; 0.5f];
                          [0.2f; 0.6f];
                          [0.3f; 0.7f];
                          [0.4f; 0.8f] ]
                          |> mapMatrix logitFunction
    let rnd4By2 = array2D [ [0.05f; 0.95f];
                            [0.67f; 0.37f];
                            [0.12f; 0.65f];
                            [0.75f; 0.12f] ]
    let res4By2 = array2D [ [1.0f; 0.0f];
                            [0.0f; 1.0f];
                            [1.0f; 1.0f];
                            [0.0f; 1.0f] ]

    let rnd2By4With3FirstRowActivations = array2D [ [1.00f; 1.00f; 1.00f; 0.00f];
                                                    [0.95f; 0.37f; 0.65f; 0.12f] ]
    let rnd4By2With3FirstRowActivations = array2D [ [1.00f; 1.00f];
                                                    [0.67f; 0.37f];
                                                    [0.12f; 0.65f];
                                                    [0.75f; 0.12f] ]

    let rnd2By4With3FirstColumnActivations = array2D [ [1.00f; 0.67f; 0.12f; 0.75f];
                                                       [1.00f; 0.37f; 0.65f; 0.12f] ]
    let rnd4By2With3FirstColumnActivations = array2D [ [1.00f; 0.95f];
                                                       [1.00f; 0.37f];
                                                       [1.00f; 0.65f];
                                                       [0.00f; 0.12f] ]

    let activateFirstRowTemplate (blockSize : int) (nActivations : int) = cuda {
        let! activateFirstRowKernel = activateFirstRowKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let activateFirstRowKernel = program.Apply activateFirstRowKernel

            fun (A : Matrix) ->
                let hA = height A
                let wA = width A
                let paddedA = padToMultiplesOf blockSize A
                let hPaddedA = height paddedA
                let wPaddedA = width paddedA
                let flattenedA = flattenMatrix paddedA

                use flattenedA = worker.Malloc flattenedA
                let lp = createActivateFirstRowLp blockSize hPaddedA wPaddedA
                activateFirstRowKernel.Launch lp flattenedA.Ptr wPaddedA nActivations

                flattenedA.Gather() |> rebuildMatrix wPaddedA hA wA
        )
    }

    let activateFirstColumnTemplate (blockSize : int) (nActivations : int) = cuda {
        let! activateFirstColumnKernel = activateFirstColumnKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let activateFirstColumnKernel = program.Apply activateFirstColumnKernel

            fun (A : Matrix) ->
                let hA = height A
                let wA = width A
                let paddedA = padToMultiplesOf blockSize A
                let hPaddedA = height paddedA
                let wPaddedA = width paddedA
                let flattenedA = flattenMatrix paddedA

                use flattenedA = worker.Malloc flattenedA
                let lp = createActivateFirstColumnLp blockSize hPaddedA wPaddedA
                activateFirstColumnKernel.Launch lp flattenedA.Ptr hPaddedA wPaddedA nActivations

                let result = flattenedA.Gather() |> rebuildMatrix wPaddedA hA wA
                result
        )
    }

    let activateTemplate (blockSize : int) = cuda {
        let! activateKernel =  <@ sigmoid @> |> activateKernel blockSize |> Compiler.DefineKernel

        return Entry(fun (program : Program) ->
            let worker = program.Worker
            let activateKernel = program.Apply activateKernel

            fun (A : Matrix) (rnd : Matrix) ->
                CudaCommon.binaryMatrixOperation blockSize A rnd activateKernel worker
        )
    }

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The activate template activates the 2 by 2 matrix correctly.``(i)=
        use activateProgram = i |> activateTemplate |> Compiler.load Worker.Default in
        activateProgram.Run A2By2 rnd2By2 |> should equal res2By2

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The activate template activates the 2 by 4 matrix correctly.``(i)=
        use activateProgram = i |> activateTemplate |> Compiler.load Worker.Default in
        activateProgram.Run A2By4 rnd2By4 |> should equal res2By4

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(32)>]
    member test.``The activate template activates the 4 by 2 matrix correctly.``(i)=
        use activateProgram = i |> activateTemplate |> Compiler.load Worker.Default in
        activateProgram.Run A4By2 rnd4By2 |> should equal res4By2

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(3)>]
    [<InlineData(32)>]
    member test.``The activateFirstRow template activates the top row of a 2 by 4 matrix correctly.``(i)=
        let activateFirstRowProgram = activateFirstRowTemplate i 3 |> Compiler.load Worker.Default in
        activateFirstRowProgram.Run rnd2By4 |> should equal rnd2By4With3FirstRowActivations

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(3)>]
    [<InlineData(32)>]
    member test.``The activateFirstRow template activates the top row of a 4 by 2 matrix correctly.``(i)=
        let activateFirstRowProgram = activateFirstRowTemplate i 3 |> Compiler.load Worker.Default in
        activateFirstRowProgram.Run rnd4By2 |> should equal rnd4By2With3FirstRowActivations

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(3)>]
    [<InlineData(32)>]
    member test.``The activateFirstColumn template activates the left column of a 2 by 4 matrix correctly.``(i)=
        let activateFirstColumnProgram = activateFirstColumnTemplate i 3 |> Compiler.load Worker.Default in
        activateFirstColumnProgram.Run rnd2By4 |> should equal rnd2By4With3FirstColumnActivations

    [<Theory>]
    [<InlineData(1)>]
    [<InlineData(2)>]
    [<InlineData(3)>]
    [<InlineData(32)>]
    member test.``The activateFirstColumn template activates the left column of a 4 by 2 matrix correctly.``(i)=
        let activateFirstColumnProgram = activateFirstColumnTemplate i 3 |> Compiler.load Worker.Default in
        activateFirstColumnProgram.Run rnd4By2 |> should equal rnd4By2With3FirstColumnActivations


