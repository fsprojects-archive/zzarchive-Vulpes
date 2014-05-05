namespace DeepBelief.Tests

module ImageClassificationTests =

    open Xunit
    open FsUnit.Xunit
    open Xunit.Extensions
    open DeepBelief.ImageClassification

    type ``Given a pixel input value`` ()=
        [<Theory>]
        [<InlineData(System.Single.MaxValue, 1.0f)>]
        [<InlineData(2.0f, 1.0f)>]
        [<InlineData(1.0f, 1.0f)>]
        [<InlineData(0.5f, 0.5f)>]
        [<InlineData(0.0f, 0.0f)>]
        [<InlineData(-1.0f, 0.0f)>]
        [<InlineData(System.Single.MinValue, 0.0f)>]
        member test.``The output of createImagePixel is bounded correctly.`` input output =
            let actualImagePixel = createImagePixel input in
            let expectedImagePixel = ImagePixel output in
            expectedImagePixel |> should equal <| actualImagePixel

    type ``Given a set of label values`` ()=
        [<Theory>]
        [<InlineData(100, System.Single.MaxValue, 1.0f)>]
        [<InlineData(100, 2.0f, 1.0f)>]
        [<InlineData(100, 1.0f, 1.0f)>]
        [<InlineData(100, 0.5f, 0.5f)>]
        [<InlineData(100, 0.0f, 0.0f)>]
        [<InlineData(100, -1.0f, 0.0f)>]
        [<InlineData(100, System.Single.MinValue, 0.0f)>]
        member test.``Labels are created correctly.`` count input output =
            let actualLabel = createLabel <| Array.init count (fun _ -> input) in
            let expectedLabel = Array.init count (fun _ -> output) |> Array.map (fun value -> ImagePixel value) |> ImageLabel
            expectedLabel |> should equal <| actualLabel
