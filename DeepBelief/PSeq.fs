namespace Microsoft.FSharp.Collections

open System
open System.Linq
open System.Threading

// Type abbreviation for parallel sequences.
type pseq<'T> = ParallelQuery<'T>

[<RequireQualifiedAccess>]
[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module PSeq = 
 
    // Converst a seq<'T> to a pseq<'T>.
    let inline toP (s : seq<'T>) = 
        match s with
        | :? pseq<'T> as p ->  p
        | _ -> s.AsParallel()

    // Seq.* functions
    let empty<'T> = 
        ParallelEnumerable.Empty<'T>()

    let length s = 
        ParallelEnumerable.Count(toP(s))

    let isEmpty s = 
        not (ParallelEnumerable.Any(toP(s)))

    let singleton x = 
        ParallelEnumerable.Repeat(x, 1)

    let head s = 
        ParallelEnumerable.First(toP(s))

    let truncate n s = 
        ParallelEnumerable.Take(toP(s), n)

    let fold<'T, 'State> (f : 'State -> 'T -> 'State) acc s = 
        ParallelEnumerable.Aggregate(toP(s),acc,Func<_,_,_>(f))

    let reduce f s = 
        ParallelEnumerable.Aggregate(toP(s), Func<_,_,_>(f))

    let exists p s = 
        ParallelEnumerable.Any(toP(s), Func<_,_>(p))

    let forall p s = 
        ParallelEnumerable.All(toP(s), Func<_,_>(p))

    let exists2 (f : 'T -> 'U -> bool) s1 s2 = 
        ParallelEnumerable.Any(ParallelEnumerable.Zip(toP(s1), toP(s2),Func<_,_,_>(f)), Func<_,_>(id))

    let forall2 (f : 'T -> 'U -> bool) s1 s2 = 
        ParallelEnumerable.All(ParallelEnumerable.Zip(toP(s1), toP(s2),Func<_,_,_>(f)), Func<_,_>(id))

    let filter p s = 
        ParallelEnumerable.Where(toP(s), Func<_,_>(p))

    let iter f s = 
        ParallelEnumerable.ForAll(toP(s), Action<_>(f))

    let map f s  = 
        ParallelEnumerable.Select(toP(s), new Func<_,_>(f)) 

    let pick f s = 
        let projected = ParallelEnumerable.Select(toP(s),Func<_,_>(f))
        let res = ParallelEnumerable.FirstOrDefault(projected, Func<_,_>(Option.isSome))
        match res with 
        | Some x -> x
        | None -> raise (new System.Collections.Generic.KeyNotFoundException())

    let find p s = 
        ParallelEnumerable.First(toP(s), Func<_,_>(p))

    let tryFind p s = 
        let withSomes = ParallelEnumerable.Select(toP(s), Func<_,_>(Some))
        ParallelEnumerable.FirstOrDefault(withSomes, Func<_,_>(fun x -> p(Option.get(x))))

    let findIndex p s = 
        let indexed = ParallelEnumerable.Select(toP(s), Func<_,int,_>(fun x i -> (i,x)))
        match ParallelEnumerable.First(indexed, Func<_,_>(fun (_,x) -> p(x))) with
        | (i,x) -> i

    let tryFindIndex p s = 
         let indexed = ParallelEnumerable.Select(toP(s), Func<_,int,_>(fun x i -> Some (i,x)))
         match ParallelEnumerable.FirstOrDefault(indexed, Func<_,_>(fun x -> p(snd(Option.get x)))) with
         | Some (i,x) -> Some i
         | None -> None

    let ofArray (arr : 'T array) = 
        arr.AsParallel()

    let toArray s = 
        ParallelEnumerable.ToArray(toP(s))

    let ofList (l : 'T list) = 
        l.AsParallel()

    let toList (s : seq<'T>) = 
        toP(s) |> List.ofSeq

    let ofSeq (s : seq<'T>)  = 
        s.AsParallel()

    let toSeq s = 
        toP(s).AsSequential()

    let cast<'T> (s : System.Linq.ParallelQuery) : pseq<'T> = 
        ParallelEnumerable.Cast<'T>(s)    

    let collect f s = 
        ParallelEnumerable.SelectMany(toP(s), Func<_,_>(fun x -> (f x) :> seq<'U>))

    let append s1 s2 = 
        ParallelEnumerable.Concat(toP(s1),toP(s2))

    let init n f = 
        ParallelEnumerable.Select(ParallelEnumerable.Range(0, n), Func<_,_>(f))

    let iter2 f s1 s2 = 
        ParallelEnumerable.Zip(toP(s1),toP(s2), Func<_,_,_>(fun x y -> do f x y )) |> ignore

    let nth n s = 
        ParallelEnumerable.ElementAt(toP(s), n)

    let map2 f s1 s2 = 
        ParallelEnumerable.Zip(toP(s1),toP(s2), Func<_,_,_>(fun x y -> f x y))

    let zip s1 s2 = 
        ParallelEnumerable.Zip(toP(s1),toP(s2), Func<_,_,_>(fun x y -> (x,y)))

    let mapi f s = 
        ParallelEnumerable.Select(toP(s), new Func<_,_,_>(fun i c -> f c i))

    let iteri f s = 
        let indexed = ParallelEnumerable.Select(toP(s), Func<_,_,_>(fun x i -> (x,i)))
        ParallelEnumerable.ForAll(indexed, Action<_>(fun (x,i) -> f i x))

    let takeWhile f s = 
        ParallelEnumerable.TakeWhile(toP(s), Func<_,_>(f))

    let skip n s = 
        ParallelEnumerable.Skip(toP(s), n)

    let skipWhile f s = 
        ParallelEnumerable.SkipWhile(toP(s), Func<_,_>(f))

    let groupBy (f : 'T -> 'Key) s = 
        ParallelEnumerable.GroupBy(toP(s),Func<_,_>(f), Func<_,_,_>(fun k v -> (k, v)))  

    let distinct s = 
        ParallelEnumerable.Distinct(toP(s))

    let distinctBy p s = 
        let comparer = 
            { new System.Collections.Generic.IEqualityComparer<'T * 'Key> with
                member this.Equals(((_, p1)),((_,p2))) = p1 = p2
                member this.GetHashCode((_,p1)) = p1.GetHashCode() }
        let projected = ParallelEnumerable.Select(toP(s), Func<_,_>(fun x -> (x, p x)))
        let distinct = ParallelEnumerable.Distinct(projected, comparer)
        ParallelEnumerable.Select(distinct, Func<_,_>(fun (x,px) -> x))

    let sort s  = 
        ParallelEnumerable.OrderBy(toP(s), Func<_,_>(fun x -> x)) :> pseq<'T>

    let sortBy (f : 'T -> 'Key) s = 
        ParallelEnumerable.OrderBy(toP(s), Func<_,_>(f)) :> pseq<'T>

    let countBy f s = 
        ParallelEnumerable.GroupBy(toP(s), Func<_,_>(f), Func<_,_,_>(fun k vs -> (k, Seq.length vs)))

    let concat ss = 
        ParallelEnumerable.Aggregate(toP(ss), empty, Func<_,_,_>(fun soFar y -> append soFar (toP y) : pseq<_>))

    let choose chooser s = 
        let projected = ParallelEnumerable.Select(toP(s), Func<_,_>(chooser))
        let somes = ParallelEnumerable.Where(projected, Func<_,_>(Option.isSome))
        ParallelEnumerable.Select(somes, Func<_,_>(Option.get))

    let inline average (s : seq< ^T >) : ^T 
              when ^T : (static member ( + ) : ^T * ^T -> ^T) 
              and  ^T : (static member DivideByInt : ^T * int -> ^T) 
              and  ^T : (static member Zero : ^T) = 
        match s with
        | :? seq<float> as s -> unbox(ParallelEnumerable.Average(toP(s)))
        | :? seq<float32> as s -> unbox(ParallelEnumerable.Average(toP(s)))
        | :? seq<decimal> as s -> unbox(ParallelEnumerable.Average(toP(s)))
        | _ -> failwithf "Average is uspported for element types float, float32 and decimal, but given type : %s" (typeof<(^T)>.ToString())

    let inline averageBy (f : 'T -> ^U) (s : seq< 'T >) : ^U 
            when ^U : (static member ( + ) : ^U * ^U -> ^U) 
            and  ^U : (static member DivideByInt : ^U * int -> ^U) 
            and  ^U : (static member Zero : ^U) =
        let bType = typeof<(^U)>
        if   bType = typeof<float> then unbox(ParallelEnumerable.Average(toP(s), Func<_,float>(fun x -> unbox(f x))))
        elif bType = typeof<float32> then unbox(ParallelEnumerable.Average(toP(s), Func<_,float32>(fun x -> unbox(f x))))
        elif bType = typeof<decimal> then unbox(ParallelEnumerable.Average(toP(s), Func<_,decimal>(fun x -> unbox(f x))))
        else failwithf "AverageBy is uspported for projected types float, float32 and decimal, but used at type type : %s" (bType.ToString())

    let inline sum (s : seq< ^T >) : ^T 
            when ^T : (static member ( + ) : ^T * ^T -> ^T) 
            and  ^T : (static member Zero : ^T) = 
        match s with
        | :? seq<int> as s -> unbox(ParallelEnumerable.Sum(toP(s)))
        | :? seq<int64> as s -> unbox(ParallelEnumerable.Sum(toP(s)))
        | :? seq<float> as s -> unbox(ParallelEnumerable.Sum(toP(s)))
        | :? seq<float32> as s -> unbox(ParallelEnumerable.Sum(toP(s)))
        | :? seq<decimal> as s -> unbox(ParallelEnumerable.Sum(toP(s)))
        | _ -> failwithf "Sum is uspported for element types int, int64, float, float32 and decimal, but given type : %s" (typeof<(^T)>.ToString())

    let inline sumBy (f : 'T -> ^U) (s : seq< 'T >) : ^U  
            when ^U : (static member ( + ) : ^U * ^U -> ^U) 
            and  ^U : (static member Zero : ^U) = 
        let bType = typeof<(^U)>
        if   bType = typeof<int> then unbox(ParallelEnumerable.Average(toP(s), Func<_,int>(fun x -> unbox(f x))))
        elif bType = typeof<int64> then unbox(ParallelEnumerable.Average(toP(s), Func<_,int64>(fun x -> unbox(f x))))        
        elif bType = typeof<float> then unbox(ParallelEnumerable.Average(toP(s), Func<_,float>(fun x -> unbox(f x))))
        elif bType = typeof<float32> then unbox(ParallelEnumerable.Average(toP(s), Func<_,float32>(fun x -> unbox(f x))))
        elif bType = typeof<decimal> then unbox(ParallelEnumerable.Average(toP(s), Func<_,decimal>(fun x -> unbox(f x))))
        else failwithf "Sum is uspported for element types int, int64, float, float32 and decimal, but given type : %s" (bType.ToString())

    let inline min (s : seq< ^T > ) : ^T when ^T : comparison =    
        match s with
        | :? seq<int> as s -> unbox(ParallelEnumerable.Min(toP(s)))
        | :? seq<int64> as s -> unbox(ParallelEnumerable.Min(toP(s)))
        | :? seq<float> as s -> unbox(ParallelEnumerable.Min(toP(s)))
        | :? seq<float32> as s -> unbox(ParallelEnumerable.Min(toP(s)))
        | :? seq<decimal> as s -> unbox(ParallelEnumerable.Min(toP(s)))
        | _ -> failwithf "Min is uspported for element types int, int64, float, float32 and decimal, but given type : %s" (typeof<(^T)>.ToString())

    let inline minBy (f : ^T -> ^U) (s : seq< ^T >) : ^U when ^U : comparison = 
        let bType = typeof<(^U)>
        if   bType = typeof<int> then unbox(ParallelEnumerable.Min<(^T)>(toP(s), Func<_,int>(fun x -> unbox(f x))))
        elif bType = typeof<int64> then unbox(ParallelEnumerable.Min<(^T)>(toP(s), Func<_,int64>(fun x -> unbox(f x))))        
        elif bType = typeof<float> then unbox(ParallelEnumerable.Min<(^T)>(toP(s), Func<_,float>(fun x -> unbox(f x))))
        elif bType = typeof<float32> then unbox(ParallelEnumerable.Min<(^T)>(toP(s), Func<_,float32>(fun x -> unbox(f x))))
        elif bType = typeof<decimal> then unbox(ParallelEnumerable.Min<(^T)>(toP(s), Func<_,decimal>(fun x -> unbox(f x))))
        else ParallelEnumerable.Min(toP(s), Func<_,_>(f))

    let inline max (s : seq< ^T >) : ^T =    
        match s with
        | :? seq<int> as s -> unbox(ParallelEnumerable.Max(toP(s)))
        | :? seq<int64> as s -> unbox(ParallelEnumerable.Max(toP(s)))
        | :? seq<float> as s -> unbox(ParallelEnumerable.Max(toP(s)))
        | :? seq<float32> as s -> unbox(ParallelEnumerable.Max(toP(s)))
        | :? seq<decimal> as s -> unbox(ParallelEnumerable.Max(toP(s)))
        | _ -> failwithf "Max is uspported for element types int, int64, float, float32 and decimal, but given type : %s" (typeof<(^T)>.ToString())

    let inline maxBy (f : ^T -> ^U) (s : seq< ^T >) : ^U = 
        let bType = typeof<(^U)>
        if   bType = typeof<int> then unbox(ParallelEnumerable.Max<(^T)>(toP(s), Func<_,int>(fun x -> unbox(f x))))
        elif bType = typeof<int64> then unbox(ParallelEnumerable.Max<(^T)>(toP(s), Func<_,int64>(fun x -> unbox(f x))))        
        elif bType = typeof<float> then unbox(ParallelEnumerable.Max<(^T)>(toP(s), Func<_,float>(fun x -> unbox(f x))))
        elif bType = typeof<float32> then unbox(ParallelEnumerable.Max<(^T)>(toP(s), Func<_,float32>(fun x -> unbox(f x))))
        elif bType = typeof<decimal> then unbox(ParallelEnumerable.Max<(^T)>(toP(s), Func<_,decimal>(fun x -> unbox(f x))))
        else ParallelEnumerable.Min(toP(s), Func<_,_>(f))



    // Missing Seq.* functions
    //
    //    ?	zip3
    //    ?	scan
    //    ?	windowed
    //    ?	tryPick
    //    ?	take
    //    ?	readonly
    //    ?	pairwise
    //    ?	initInfinite
    //    ?	delay
    //    ?	compareWith
    //    ?	unfold



    // Parallel-specific functionality 
    let withDegreeOfParallelism n s = 
        toP(s).WithDegreeOfParallelism(n)

    let withExecutionMode executionMode s = 
        toP(s).WithExecutionMode(executionMode)

    let withMergeOptions mergeOptions s = 
        toP(s).WithMergeOptions(mergeOptions)
    
    let withCancellation cancellationToken s = 
        toP(s).WithCancellation(cancellationToken)
