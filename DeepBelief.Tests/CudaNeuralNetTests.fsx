#load "../packages/FSharp.Charting.0.90.6/FSharp.Charting.fsx"

open FSharp.Charting
open System

let mod10Plus1 i = 1 + i % 10
let sineCurve i = Array.init 784 (fun j -> Math.Sin(float j * (mod10Plus1 i |> float) * 2.0 * Math.PI / 784.0) |> float32)
let label i = Array.init 10 (fun j -> if j + 1 = mod10Plus1 i then 1.0f else 0.0f)
let trainingSet = [|1..100|] |> Array.map (fun i -> (sineCurve i, label i))
let testSet = [|1..10|] |> Array.map (fun i -> (sineCurve i, label i))

trainingSet |> Array.map (fun e -> snd e) |> Array.map (fun x -> Array.sum x)

[ fst trainingSet.[0] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[1] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[2] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[3] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[4] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[5] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[6] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[7] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[8] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line;
fst trainingSet.[9] |> Array.mapi (fun i s -> (i, s)) |> Chart.Line ]
|> Chart.Combine
