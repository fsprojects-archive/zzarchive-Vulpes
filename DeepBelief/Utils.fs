// The MIT License (MIT)
// 
// Copyright (c) 2014 SpiegelSoft Ltd
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
namespace DeepBelief

module Utils =

    open Microsoft.FSharp.Quotations
    open MathNet.Numerics.Random
    open MathNet.Numerics.Distributions
    open System.Threading.Tasks
    open Alea.CUDA.Utilities

    type [<ReflectedDefinition>] Matrix = float32[,]
    type [<ReflectedDefinition>] Vector = float32[]

    let LCG_A = 1664525u
    let LCG_C = 1013904223u

    let generateStartState (seed:uint32) =
        let state = Array.zeroCreate 8
        state.[0] <- seed
        for i = 1 to 7 do state.[i] <- LCG_A * state.[i - 1] + LCG_C
        state

    /// Transforms an uint32 random number to a float value 
    /// on the interval [0,1] by dividing by 2^32-1
    let [<ReflectedDefinition>] toFloat32 (x:uint32) = float32(x) * 2.3283064E-10f

    /// Transforms an uint32 random number to a float value 
    /// on the interval [0,1] by dividing by 2^32-1
    let [<ReflectedDefinition>] toFloat64 (x:uint32) = float(x) * 2.328306437080797e-10   

    /// compute the derivative of a function, midpoint rule
    let [<ReflectedDefinition>] derivative eps f = 
        fun x -> ((f (x + eps/2.0f) - f (x - eps/2.0f)) / eps)

    /// Density of normal with mean mu and standard deviation sigma. 
    let inline normpdf (mu:'T) (sigma:'T) : Expr<'T -> 'T> =
        <@ fun x -> exp(-(x - mu)*(x - mu)/(2G*sigma*sigma)) / (sigma*__sqrt2pi()) @>

    let flattenMatrix M = 
        let h = Array2D.length1 M
        let w = Array2D.length2 M
        Array.init (h*w) (fun i -> M.[i / w,i % w])

    let flattenSamples samples =
        samples |> Array.map flattenMatrix
        |> Array.fold (fun acc element -> Array.concat [acc; element]) [| |]

    let rebuildMatrix wFull h w (X : Vector) =
        Array2D.init h w (fun i j -> X.[i * wFull + j])

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

    let padToMultipleOf n x =
        let size = Array.length x
        let paddedSize = nextMultipleOf n size
        Array.init paddedSize 
            (fun i -> if i < size then x.[i] else 0.0f)

    let topLeftSubmatrix h w (M : float32[,]) =
        Array2D.init h w (fun i j -> M.[i, j])

    let subvector size (x : float32[]) =
        Array.init size (fun i -> x.[i])

    let height = Array2D.length1
    let width = Array2D.length2

    let row i M =
        Array.init (width M) (fun j -> M.[i, j])

    let column j M =
        Array.init (height M) (fun i -> M.[i, j])

    let scalarProduct (v : Vector) w = Array.map2 (*) v w |> Array.sum

    let multiply A B =
        let rowsA, colsA = height A, width A
        let rowsB, colsB = height B, width B
        let result = Array2D.create rowsA colsB 0.0f
        Parallel.For(0, rowsA, (fun i->
            for j = 0 to colsB - 1 do
               for k = 0 to colsA - 1 do
                  result.[i,j] <- result.[i,j] + A.[i,k] * B.[k,j]))  
        |> ignore
        result
    
    let transposeAndMultiply A B =
        let rowsA, colsA = width A, height A
        let rowsB, colsB = height B, width B
        let result = Array2D.create rowsA colsB 0.0f
        Parallel.For(0, rowsA, (fun i->
            for j = 0 to colsB - 1 do
               for k = 0 to colsA - 1 do
                  result.[i,j] <- result.[i,j] + A.[k,i] * B.[k,j]))  
        |> ignore
        result

    let multiplyByTranspose A B =
        let rowsA, colsA = height A, width A
        let rowsB, colsB = width B, height B
        let result = Array2D.create rowsA colsB 0.0f
        Parallel.For(0, rowsA, (fun i->
            for j = 0 to colsB - 1 do
               for k = 0 to colsA - 1 do
                  result.[i,j] <- result.[i,j] + A.[i,k] * B.[j,k]))  
        |> ignore
        result

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

    let sigmoidFunction x = 1.0f / (1.0f + exp(-x))
    let logitFunction x = log x - log (1.0f - x)
    let sigmoidDerivative x = 
        let f = sigmoidFunction x
        f * (1.0f - f)

    let toList M =
        let h = height M
        let w = width M
        [0..h - 1] |> List.map (fun i -> List.init w (fun j -> M.[i, j]))

    let toArray M =
        let h = height M
        let w = width M
        [|0..h - 1|] |> Array.map (fun i -> Array.init w (fun j -> M.[i, j]))

    let transpose M =
        let h = width M
        let w = height M
        Array2D.init h w (fun i j -> M.[j, i])

    let toColumns (M : Matrix) = 
        let h = height M
        let w = width M
        [|0..w - 1|] |> Array.map (fun j -> Array.init h (fun i -> M.[i, j])) 

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

    let prependRowOfZeroes M =
        M |> prependRow (Array.init (width M) (fun _ -> 0.0f))

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

    let proportionOfVisibleUnits v =
        v |> Array.filter (fun u -> u > 0.5f) |> fun arr -> float32 arr.Length / float32 v.Length
