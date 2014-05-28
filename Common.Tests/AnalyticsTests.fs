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
            let signals = Signals [|Signal (0.1f, 0.0f); Signal (0.2f, 0.0f); Signal (0.3f, 0.0f)|] in
            signals.ValuesPrependedForBias |> should equal <| Vector [| 1.0f; 0.1f; 0.2f; 0.3f |] 
            

    