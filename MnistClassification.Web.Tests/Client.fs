namespace MnistClassification.Web.Tests

open Xunit
open System
open MnistClassification.Web.Client
open FsUnit.Xunit

type ``Training Set`` ()=

    [<Fact>] member test.
        ``Training set is rendered correctly.``()=
        TrainingSet().Html |> should equal "Hello" 
