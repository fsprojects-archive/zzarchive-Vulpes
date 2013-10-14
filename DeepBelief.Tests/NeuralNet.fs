namespace DeepBelief.Tests

open Xunit
open FsUnit.Xunit
open DeepBelief.NeuralNet
open MathNet.Numerics.LinearAlgebra.Double
open MathNet.Numerics.LinearAlgebra.Generic
open System

type ``Given a Restricted Boltzmann Machine`` ()=
   let nh = 1000
   let nv = 789
   let rbm = new Rbm(nh, nv)
   let bound = 4.0 * Math.Sqrt (6.0 / (float)(nh + nv))

   [<Fact>] member test.
    ``The width of W should be correct.``()=
        rbm.W.ColumnCount |> should equal 1000

   [<Fact>] member test.
    ``The height of W should be correct.``()=
        rbm.W.RowCount |> should equal 789

   [<Fact>] member test.
    ``The entries of W should have the correct bounds.``()=
        rbm.W |> Matrix.map Math.Abs 
        |> Matrix.forall (fun x -> x <= bound) 
        |> should equal true
