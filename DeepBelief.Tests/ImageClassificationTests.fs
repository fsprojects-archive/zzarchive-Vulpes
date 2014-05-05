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
