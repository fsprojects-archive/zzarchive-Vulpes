namespace Common.Tests

module AnalyticsTests = 

    open Xunit
    open Xunit.Extensions
    open FsUnit.Xunit
    open Common.Analytics
    open Common.NeuralNet
    open System

    type ``Floating point derivatives evaluate correctly``()=

        let verifyDerivative value Df =
            let f = FloatingPointFunction (fun (Domain x) -> Math.Exp (2.0 * float x) |> float32 |> Range) in
            let differentiableFunction = DifferentiableFunction (f, Df) in
            let actual = differentiableFunction.EvaluateDerivative (Domain value) in
            let expected = Gradient (2.0 * Math.Exp(2.0f * value |> float) |> float32)
            expected |> should equal actual
        
        [<Theory>]
        [<InlineData(-2.0f)>]
        [<InlineData(-1.0f)>]
        [<InlineData(0.0f)>]
        [<InlineData(1.0f)>]
        [<InlineData(2.0f)>]
        member test.``The derivative of e^2x can be evaluated when expressed in function value form.``(value)=
            let Df = FunctionValueForm (fun (Range y) -> 2.0f * y |> Gradient) in 
            verifyDerivative value Df
        
        [<Theory>]
        [<InlineData(-2.0f)>]
        [<InlineData(-1.0f)>]
        [<InlineData(0.0f)>]
        [<InlineData(1.0f)>]
        [<InlineData(2.0f)>]
        member test.``The derivative of e^2x can be evaluated when expressed in argument form.``(value)=
            let Df = ArgumentForm (fun (Domain x) -> 2.0f * (Math.Exp (2.0 * float x) |> float32) |> Gradient) in 
            verifyDerivative value Df
        
        [<Theory>]
        [<InlineData(-2.0f)>]
        [<InlineData(-1.0f)>]
        [<InlineData(0.0f)>]
        [<InlineData(1.0f)>]
        [<InlineData(2.0f)>]
        member test.``The derivative of e^2x can be evaluated when expressed in argument and function value form.``(value)=
            let Df = ArgumentAndFunctionValueForm (fun (Domain x) -> fun (Range y) -> 2.0f * y |> Gradient) in 
            verifyDerivative value Df

    type ``Vectors can be prepended for bias``()=
        
        [<Fact>]
        member test.``The values prepended for bias match the expected input pattern.``()=
            let signals = Vector [| 0.1f; 0.2f; 0.3f|] in
            signals.PrependForBias |> should equal <| Vector [| 1.0f; 0.1f; 0.2f; 0.3f |]

    type ``A differentiable function can generate signals from a vector``()=

        [<Theory>]
        [<InlineData(-2.0f)>]
        [<InlineData(-1.0f)>]
        [<InlineData(1.0f)>]
        [<InlineData(2.0f)>]
        member test.``The values of a 10-dimensional vector are converted into the correct signals``(exponent)=
            let vector = [|1.0f..10.0f|] in
            let power x n = float32 (Math.Pow(float x, float n))
            let differentiableFunction = DifferentiableFunction (FloatingPointFunction (fun (Domain x) -> Range (power x exponent)), ArgumentForm (fun (Domain x) -> Gradient (exponent * power x (exponent - 1.0f)))) in
            differentiableFunction.GenerateSignals (Vector vector) |> should equal <| (vector |> Array.map (fun x -> power x exponent |> Signal))

    type ``Matrix arithmetic``()=

        [<Fact>]
        member test.``Matrix addition``()=
            let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ] |> Matrix in
            let B = array2D [ [7.0f; 8.0f; 9.0f]; [10.0f; 11.0f; 12.0f] ] |> Matrix in
            let C = array2D [ [8.0f; 10.0f; 12.0f]; [14.0f; 16.0f; 18.0f] ] |> Matrix in
            A + B |> should equal <| C

        [<Fact>]
        member test.``Matrix multplication``()=
            let A = array2D [ [1.0f; 2.0f; 3.0f]; [4.0f; 5.0f; 6.0f] ] |> Matrix in
            let x = [|7.0f; 8.0f; 9.0f|] |> Vector in
            A * x |> should equal <| Vector [|50.0f; 122.0f|]

        [<Fact>]
        member test.``Matrix transpose multplication``()=
            let A = array2D [ [1.0f; 4.0f; ]; [2.0f; 5.0f;]; [3.0f; 6.0f] ] |> Matrix in
            let x = [|7.0f; 8.0f; 9.0f|] |> Vector in
            A ^* x |> should equal <| Vector [|50.0f; 122.0f|]

    type ``Vector arithmetic``()=

        [<Fact>]
        member test.``Vector subtraction``()=
            let x = [|4.0f; 5.0f; 6.0f|] |> Vector in
            let y = [|1.0f; 2.0f; 3.0f|] |> Vector in
            x - y |> should equal <| Vector [|3.0f; 3.0f; 3.0f|]
