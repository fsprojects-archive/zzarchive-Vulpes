namespace Common.Tests

module AnalyticsTests = 

    open Xunit
    open Xunit.Extensions
    open FsUnit.Xunit
    open Common.Analytics
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

    type ``Signals can be converted into values prepended for bias``()=
        
        [<Fact>]
        member test.``The values prepended for bias match the expected input pattern.``()=
            let signals = Signals [|Signal (0.1f, None); Signal (0.2f, None); Signal (0.3f, None)|] in
            signals.ValuesPrependedForBias |> should equal <| Vector [| 1.0f; 0.1f; 0.2f; 0.3f |]

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
            differentiableFunction.GenerateSignals (Vector vector) |> should equal <| Signals (vector |> Array.map (fun x -> Signal (power x exponent, exponent * power x (exponent - 1.0f) |> Some)))

    