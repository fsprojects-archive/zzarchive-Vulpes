namespace DeepBelief.Tests

module TestUtils =

    open DeepBelief
    open System
    open Utils

    let allElementsOfVector predicate (v : Vector) =
        v |> Array.fold (fun acc element -> acc && predicate element) true

    let allElementsOfMatrix predicate (M : Matrix) =
        M |> toArray |> Array.fold (fun acc element -> acc && allElementsOfVector predicate element) true

    let nonZeroEntries M =
        M |> flattenMatrix |> Array.filter (fun x -> x <> 0.0f)

    let liesWithinTolerance diffs =
        let maxDiff = Array.max diffs
        maxDiff < 1e-6

    let arraysMatch (cpu : float32[]) (gpu : float32[]) =
        Array.zip cpu gpu |> Array.map (fun el -> Math.Abs ((fst el |> float) - (snd el |> float))) |> liesWithinTolerance

    let outputsMatch result =
        arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

    let levelResultsMatch results =
        List.forall (fun result -> outputsMatch result) results

    let resultsMatch cpu gpu =
        List.zip cpu gpu |> levelResultsMatch
