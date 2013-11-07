namespace DeepBelief.Tests

module TestUtils =

    open DeepBelief
    open Utils

    let allElementsOfVector predicate (v : Vector) =
        v |> Array.fold (fun acc element -> acc && predicate element) true

    let allElementsOfMatrix predicate (M : Matrix) =
        M |> toArray |> Array.fold (fun acc element -> acc && allElementsOfVector predicate element) true

    let nonZeroEntries M =
        M |> flattenMatrix |> Array.filter (fun x -> x <> 0.0f)