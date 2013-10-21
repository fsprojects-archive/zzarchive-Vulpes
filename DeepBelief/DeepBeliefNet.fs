namespace DeepBelief

module DeepBeliefNet =

    let dbnTrain trainLayer stepUp (dbn : List<'a>) xInputs =
        let start = trainLayer (dbn.Head, xInputs)
        dbn.Tail |> List.fold(fun (acc : List<'b * 'c>) element -> 
            let currentTuple = acc.Head
            let x = stepUp (fst currentTuple) (snd currentTuple)
            let nextDbn = trainLayer (element, x)
            (nextDbn, x) :: acc) [(start, xInputs)]
            |> List.rev

