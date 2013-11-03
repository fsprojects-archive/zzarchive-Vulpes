namespace DeepBelief

module Utils =

    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions

    type [<ReflectedDefinition>] Matrix = float32[,]
    type [<ReflectedDefinition>] Vector = float32[]

    let flatten M = 
        let h = Array2D.length1 M
        let w = Array2D.length2 M
        Array.init (h*w) (fun i -> M.[i / w,i % w])

    let stackRows w X =
        let h = Array.length X / w
        Array2D.init h w (fun i j -> X.[i * w + j])

    let nextMultipleOf n i =
        let r = i % n
        if r = 0 then i else i + n - r

    let padToMultiplesOf n M =
        let h = Array2D.length1 M
        let w = Array2D.length2 M
        let paddedHeight = nextMultipleOf n h
        let paddedWidth = nextMultipleOf n w
        Array2D.init paddedHeight paddedWidth 
            (fun i j -> if i < h && j < w then M.[i, j] else 0.0f)

    let topLeftSubmatrix h w (M : float32[,]) =
        Array2D.init h w (fun i j -> M.[i, j])

    let height = Array2D.length1
    let width = Array2D.length2

    let row i M =
        Array.init (width M) (fun j -> M.[i, j])

    let column j M =
        Array.init (height M) (fun i -> M.[i, j])

    let scalarProduct (v : Vector) w = Array.map2 (*) v w |> Array.sum

    let multiply A B =
        let h = height A
        let w = width B
        let rowsOfA = [|0..h - 1|] |> Array.map (fun i -> row i A)
        let columnsOfB = [|0..w - 1|] |> Array.map (fun j -> column j B)
        Array2D.init h w (fun i j -> scalarProduct rowsOfA.[i] columnsOfB.[j])

    let transposeAndMultiply A B =
        let h = width A
        let w = width B
        let rowsOfAT = [|0..h - 1|] |> Array.map (fun i -> column i A)
        let columnsOfB = [|0..w - 1|] |> Array.map (fun j -> column j B)
        Array2D.init h w (fun i j -> scalarProduct rowsOfAT.[i] columnsOfB.[j])

    let multiplyByTranspose A B =
        let h = height A
        let w = height B
        let rowsOfA = [|0..h - 1|] |> Array.map (fun i -> row i A)
        let columnsOfBT = [|0..w - 1|] |> Array.map (fun j -> row j B)
        Array2D.init h w (fun i j -> scalarProduct rowsOfA.[i] columnsOfBT.[j])

    let multiplyVectorByMatrix A v  =
        let h = height A
        let w = width A
        let rowsOfA = [|0..h - 1|] |> Array.map (fun i -> row i A)
        Array.init h (fun i -> scalarProduct rowsOfA.[i] v)

    let mapMatrix f M =
        Array2D.init (height M) (width M) (fun i j -> f M.[i, j])

    let outerProduct (v1 : Vector) v2 =
        Array2D.init (Array.length v1) (Array.length v2) (fun i j -> v1.[i] * v2.[j])

    let prepend value vector = value :: List.ofArray vector |> Array.ofList
    let prependForBias = prepend 1.0f

    let rand = new MersenneTwister()

    let sigmoid x = 1.0f / (1.0f + exp(-x))

    let toList M =
        let h = height M
        let w = width M
        [0..h - 1] |> List.map (fun i -> List.init w (fun j -> M.[i, j]))

    let toArray M =
        let h = height M
        let w = width M
        [|0..h - 1|] |> Array.map (fun i -> Array.init w (fun j -> M.[i, j]))

    let rec transposeList = function
        | (_::_)::_ as M -> List.map List.head M :: transposeList (List.map List.tail M)
        | _ -> []

    let transpose : Matrix -> Matrix = toList >> transposeList >> array2D

    let toColumns = transpose >> toArray

    let prependColumn (column : Vector) M =
        Array2D.init (height M) (width M + 1)
            (fun i j ->
                match i, j with
                | (m, 0) -> column.[m]
                | (m, n) -> M.[m, n - 1])

    let prependRow (row : Vector) M =
        Array2D.init (height M + 1) (width M)
            (fun i j ->
                match i, j with
                | (0, n) -> row.[n]
                | (m, n) -> M.[m - 1, n])

    let prependRowOfOnes M =
        M |> prependRow (Array.init (width M) (fun _ -> 1.0f))

    let prependColumnOfOnes M =
        M |> prependColumn (Array.init (height M) (fun _ -> 1.0f))

    // Taken from http://www.cs.toronto.edu/~hinton/absps/guideTR.pdf, Section 8.
    // The initial weights should have zero mean and 0.01 standard deviation.
    let gaussianDistribution = new Normal(0.0, 0.01)

    let initGaussianWeights nRows nColumns =
        Array2D.init nRows nColumns (fun _ _ -> gaussianDistribution.Sample() |> float32)

    let addVectors v1 (v2 : Vector) =
        let n = Array.length v1
        Array.init n (fun i -> v1.[i] + v2.[i])

    let subtractVectors v1 (v2 : Vector) =
        let n = Array.length v1
        Array.init n (fun i -> v1.[i] - v2.[i])

    let addMatrices (A : Matrix) (B : Matrix) =
        let h = height A
        let w = width B
        Array2D.init h w (fun i j -> A.[i, j] + B.[i, j])

    let subtractMatrices (A : Matrix) (B : Matrix) =
        let h = height A
        let w = width B
        Array2D.init h w (fun i j -> A.[i, j] - B.[i, j])

    let multiplyVectorByScalar (lambda : float32) v =
        let n = Array.length v
        Array.init n (fun i -> lambda * v.[i])

    let multiplyMatrixByScalar (lambda : float32) M =
        let h = height M
        let w = width M
        Array2D.init h w (fun i j -> lambda * M.[i, j])

    let identityMatrix n =
        Array2D.init n n (fun i j -> if i = j then 1.0f else 0.0f)

    let sumOfRows M = M |> toArray |> Array.fold (fun acc element -> addVectors acc element) (Array.init (width M) (fun _ -> 0.0f))
    let sumOfSquares v = v |> Array.map (fun element -> element * element) |> Array.sum
    let sumOfSquaresMatrix M = M |> toArray |> Array.fold (fun acc element -> acc + (sumOfSquares element)) 0.0f

    // When applying this measure to a classification problem, 
    // where the output vectors must n - 1 zeroes and a single
    // one, it has the nice property that it evaluates to one
    // for the wrong guess, and zero for an incorrect guess.  So
    // dividing it by the set size gives the pecentage error of 
    // the test run.
    let error (target : Vector) (output : Vector) =
        (Array.zip target output |> Array.map (fun (t, o) -> t - o) |> sumOfSquares) / 2.0f
    
    let batchesOf n =
        Seq.ofArray >> Seq.mapi (fun i v -> i / n, v) >>
        Seq.groupBy fst >> Seq.map snd >>
        Seq.map (Seq.map snd >> Seq.toArray) >> Seq.toArray
    
    let nextSingle (rnd : AbstractRandomNumberGenerator) =
        rnd.NextDouble() |> float32

    let permutation rnd arr =
        arr |> Array.sortBy (fun element -> nextSingle rnd)
    let permute rnd n = permutation rnd [|0..(n-1)|]
    let permuteRows rnd M = 
        permute rnd (height M) |> Array.map (fun i -> row i M)

