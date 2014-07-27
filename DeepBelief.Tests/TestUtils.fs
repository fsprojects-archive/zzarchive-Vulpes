namespace DeepBelief.Tests

module TestUtils =

    open DeepBelief
    open System
    open Utils
    open Common.Analytics

    let allElementsOfArray predicate (v : 'a[]) =
        v |> Array.fold (fun acc element -> acc && predicate element) true

    let allElementsOfVector predicate (Vector v) =
        allElementsOfArray predicate v

    let allElementsOfMatrix predicate (M : Matrix) =
        M.ToRowMajorFormat |> allElementsOfArray predicate

    let nonZeroEntries (M : Matrix) =
        M.ToRowMajorFormat |> Array.filter (fun x -> x <> 0.0f)

    let liesWithinTolerance diffs =
        let maxDiff = Array.max diffs
        maxDiff < 1e-6

    let arraysMatch (cpu : float32[]) (gpu : float32[]) =
        Array.zip cpu gpu |> Array.map (fun el -> Math.Abs ((fst el |> float) - (snd el |> float))) |> liesWithinTolerance

    let vectorsMatch (Vector cpu) (Vector gpu) =
        arraysMatch cpu gpu

    let outputsMatch result =
        arraysMatch (fst (fst result)) (fst (snd result)) && arraysMatch (snd (fst result)) (snd (snd result))

    let levelResultsMatch results =
        List.forall (fun result -> outputsMatch result) results

    let resultsMatch cpu gpu =
        List.zip cpu gpu |> levelResultsMatch
