namespace Common

module Analytics =
    open NeuralNet
    open System.Threading.Tasks

    type Domain = Domain of float32

    type Range = Range of float32

    type Gradient = Gradient of float32

    type FloatingPointFunction = FloatingPointFunction of (Domain -> Range)

    type FloatingPointDerivative = 
        | ArgumentAndFunctionValueForm of (Domain -> Range -> Gradient)
        | FunctionValueForm of (Range -> Gradient)
        | ArgumentForm of (Domain -> Gradient)

    type DifferentiableFunction = DifferentiableFunction of (FloatingPointFunction * FloatingPointDerivative) with
        member func.Evaluate x = 
            match func with
            | DifferentiableFunction (FloatingPointFunction f, Df) -> f x
        member func.EvaluateDerivative x =
            match func with 
            | DifferentiableFunction (FloatingPointFunction f, ArgumentAndFunctionValueForm Df) -> Df x (f x)
            | DifferentiableFunction (FloatingPointFunction f, FunctionValueForm Df) -> Df (f x)
            | DifferentiableFunction (FloatingPointFunction f, ArgumentForm Df) -> Df x
        member func.EvaluateDerivative2 x fx =
            match func with
            | DifferentiableFunction (FloatingPointFunction f, ArgumentAndFunctionValueForm Df) -> Df x fx
            | DifferentiableFunction (FloatingPointFunction f, FunctionValueForm Df) -> Df fx
            | DifferentiableFunction (FloatingPointFunction f, ArgumentForm Df) -> Df x

    let height = Array2D.length1

    let width = Array2D.length2

    type [<ReflectedDefinition>] Vector = Vector of float32[] with
        static member (-) (Vector lhs, Vector rhs) =
            Array.map2 (-) lhs rhs |> Vector
        member vector.Prepend value =
            match vector with Vector array -> value :: List.ofArray array |> Array.ofList |> Vector
        member vector.PrependForBias =
            vector.Prepend 1.0f
        member vector.Length =
            match vector with Vector array -> Array.length array
        member vector.Subvector i =
            match vector with Vector array -> array.[i..] |> Vector
        member vector.SumOfSquares =
            match vector with Vector array -> array|> Array.map (fun element -> element * element) |> Array.sum
        static member Error (Vector target) (Vector output) =
            // When applying this measure to a classification problem, 
            // where the output vectors must n - 1 zeroes and a single
            // one, it has the nice property that it evaluates to one
            // for the wrong guess, and zero for an incorrect guess.  So
            // dividing it by the set size gives the pecentage error of 
            // the test run.
            (Array.zip target output |> Array.map (fun (t, o) -> t - o) |> Vector |> fun vector -> vector.SumOfSquares) / 2.0f


    type [<ReflectedDefinition>] Matrix = Matrix of float32[,] with
        static member (+) (Matrix lhs, Matrix rhs) =
            let h = height lhs
            let w = width rhs
            Array2D.init h w (fun i j -> lhs.[i, j] + rhs.[i, j])
            |> Matrix
        member this.Map f = 
            match this with Matrix matrix -> Array2D.init (height matrix) (width matrix) (fun i j -> f matrix.[i, j])
        member this.Height = match this with Matrix matrix -> height matrix
        member this.Width = match this with Matrix matrix -> width matrix
        member this.Value i j = match this with Matrix matrix -> matrix.[i, j]
        member this.Submatrix i j =
            match this with Matrix matrix -> Matrix matrix.[i.., j..]
        member this.SumOfSquares =
            match this with 
                Matrix matrix -> 
                    [|0..(height matrix)|] |> Array.map (fun i -> Array.init (width matrix) (fun j -> matrix.[i, j])) 
                    |> Array.fold (fun acc element -> acc + (Vector element).SumOfSquares) 0.0f
        member this.Row i =
            match this with Matrix matrix -> Array.init (width matrix) (fun j -> matrix.[i, j]) |> Vector
        member this.Column j =
            match this with Matrix matrix -> Array.init (height matrix) (fun i -> matrix.[i, j]) |> Vector
        static member (-) (Matrix lhs, Matrix rhs) =
            let h = height lhs
            let w = width rhs
            Array2D.init h w (fun i j -> lhs.[i, j] - rhs.[i, j])
            |> Matrix
        static member (.*) (Vector v1, Vector v2) = 
            Array.init (Array.length v1) (fun i -> v1.[i] * v2.[i])
        static member (*) (Matrix A, Vector v) =
            let row i (M : float32[,]) = Array.init (width M) (fun j -> M.[i, j])
            let h = height A
            let w = width A
            let rowsOfA = [|0..h - 1|] |> Array.map (fun i -> row i A)
            Array.init h (fun i -> Array.map2 (*) rowsOfA.[i] v |> Array.sum) 
            |> Vector
        static member (*) (lambda : float32, Matrix M) =
            let h = height M
            let w = width M
            Array2D.init h w (fun i j -> lambda * M.[i, j]) 
            |> Matrix
        static member (*) (Matrix A, Matrix B) =
            let rowsA, colsA = height A, width A
            let rowsB, colsB = height B, width B
            let result = Array2D.create rowsA colsB 0.0f
            Parallel.For(0, rowsA, (fun i->
                for j = 0 to colsB - 1 do
                   for k = 0 to colsA - 1 do
                      result.[i,j] <- result.[i,j] + A.[i,k] * B.[k,j]))  
            |> ignore
            result |> Matrix
        static member (^*) (Matrix A, Vector v) =
            let column j (M : float32[,]) = Array.init (height M) (fun i -> M.[i, j])
            let h = height A
            let w = width A
            let columnsOfA = [|0..w - 1|] |> Array.map (fun j -> column j A)
            Array.init w (fun j -> Array.map2 (*) columnsOfA.[j] v |> Array.sum) 
            |> Vector
        member this.MultiplyByTranspose (Matrix B) =
            match this with 
                Matrix A ->
                    let rowsA, colsA = height A, width A
                    let rowsB, colsB = width B, height B
                    let result = Array2D.create rowsA colsB 0.0f
                    Parallel.For(0, rowsA, (fun i->
                        for j = 0 to colsB - 1 do
                           for k = 0 to colsA - 1 do
                              result.[i,j] <- result.[i,j] + A.[i,k] * B.[j,k]))  
                    |> ignore
                    result |> Matrix
        member this.TransposeAndMultiply (Matrix B) =
            match this with 
                Matrix A ->
                    let rowsA, colsA = width A, height A
                    let rowsB, colsB = height B, width B
                    let result = Array2D.create rowsA colsB 0.0f
                    Parallel.For(0, rowsA, (fun i->
                        for j = 0 to colsB - 1 do
                           for k = 0 to colsA - 1 do
                              result.[i,j] <- result.[i,j] + A.[k,i] * B.[k,j]))  
                    |> ignore
                    result |> Matrix

    type Vector with
        static member (*) (Vector v1, Vector v2) =
            Array2D.init (Array.length v1) (Array.length v2) (fun i j -> v1.[i] * v2.[j])
            |> Matrix

    type Matrix with
        member this.PrependColumn (Vector column) =
            Array2D.init this.Height (this.Width + 1)
                (fun i j ->
                    match i, j with
                    | (m, 0) -> column.[m]
                    | (m, n) -> this.Value m (n - 1))
            |> Matrix
        member this.PrependRow (Vector row) =
            Array2D.init (this.Height + 1) this.Width
                (fun i j ->
                    match i, j with
                    | (0, n) -> row.[n]
                    | (m, n) -> this.Value (m - 1) n)
            |> Matrix
        member this.PrependColumnOfOnes =
            Array.init this.Height (fun i -> 1.0f) |> Vector |> this.PrependColumn 
        member this.PrependRowOfOnes =
            Array.init this.Width (fun j -> 1.0f) |> Vector |> this.PrependRow

    type Error = Error of float32
    
    type ErrorSignal = ErrorSignal of float32

    type ErrorSignals = ErrorSignals of ErrorSignal[]

    type Errors = Errors of Error[]

    type VisibleUnit = VisibleUnit of float32

    type VisibleUnits = VisibleUnits of VisibleUnit[]

    type TrainingExample with
        member trainingExample.VisibleUnits = trainingExample.Input |> fun (Input input) -> input |> Array.map (fun (Signal signal) -> VisibleUnit signal) |> VisibleUnits

    type HiddenUnit = HiddenUnit of (float32 * float32) with
        static member (-) (Signal signal, HiddenUnit hiddenUnit) = signal - fst hiddenUnit

    type HiddenUnits = HiddenUnits of HiddenUnit[] with
        static member (-) (Target target, HiddenUnits hiddenUnits) =
            hiddenUnits |> Array.map2 (-) target |> Array.map Error |> Errors

    type Errors with
        static member (.*) (HiddenUnits lhs, Errors rhs) =
            let derivative (HiddenUnit (x, x')) = x'
            let error (Error e) = e
            Array.map2 (*) (lhs |> Array.map derivative) (rhs |> Array.map error) |> Array.map ErrorSignal |> ErrorSignals
        member this.ToErrorSignals hiddenUnits = hiddenUnits .* this 

    type WeightGradients = WeightGradients of Matrix

    type WeightChanges = WeightChanges of Matrix with
        member changes.NextChanges (ScaledLearningRate learningRate) (Momentum momentum) (WeightGradients weightGradients) =
            match changes with WeightChanges weightChanges -> momentum * weightChanges + learningRate * weightGradients |> WeightChanges

    type InputBatch = InputBatch of Matrix with
        member this.Size =
            match this with InputBatch matrix -> matrix.Height
        member this.ActivateFirstColumn =
            match this with (InputBatch (Matrix h)) -> h.[0..,1..] |> Matrix |> fun m -> m.PrependRowOfOnes |> InputBatch

    type Input with
        static member FromVector (Vector vector) = vector |> Array.map (fun value -> Signal value) |> Input

    type BatchOutput = BatchOutput of Matrix with
        member this.ActivateFirstRow =
            match this with (BatchOutput (Matrix v)) -> v.[1..,0..] |> Matrix |> fun m -> m.PrependColumnOfOnes |> BatchOutput

    type WeightsAndBiases = WeightsAndBiases of Matrix with
        static member (*) (WeightsAndBiases weightsAndBiases, HiddenUnits hiddenUnitsArray) =
            let prependedSignals = 1.0f :: List.ofArray (Array.map (fun (HiddenUnit (x, d)) -> x) hiddenUnitsArray) |> Array.ofList |> Vector
            weightsAndBiases * prependedSignals
        static member (*) (WeightsAndBiases weightsAndBiases, VisibleUnits visibleUnitsArray) =
            let prependedSignals = 1.0f :: List.ofArray (Array.map (fun (VisibleUnit x) -> x) visibleUnitsArray) |> Array.ofList |> Vector
            weightsAndBiases * prependedSignals
        static member (*) (WeightsAndBiases weightsAndBiases, signals : Signal[]) =
            let prependedSignals = 1.0f :: List.ofArray (Array.map (fun (Signal s) -> s) signals) |> Array.ofList |> Vector
            weightsAndBiases * prependedSignals
        static member (*) (WeightsAndBiases weightsAndBiases, ErrorSignals errorSignals) =
            let product = weightsAndBiases ^* (errorSignals |> Array.map (fun (ErrorSignal errorSignal) -> errorSignal) |> Vector)
            product |> fun (Vector vector) -> vector.[1..] |> Array.map Error |> Errors
        member this.Update (WeightChanges weightChanges) =
            match this with WeightsAndBiases weightsAndBiases -> weightsAndBiases + weightChanges |> WeightsAndBiases
        member this.Forward (InputBatch batch) =
            match this with WeightsAndBiases weightsAndBiases -> weightsAndBiases.MultiplyByTranspose batch |> BatchOutput
        member this.Backward (BatchOutput output) =
            match this with WeightsAndBiases weightsAndBiases -> weightsAndBiases.TransposeAndMultiply output |> InputBatch

    type DifferentiableFunction with
        member this.GenerateHiddenUnits(Vector vector) =
            let valueAndDerivative x =
                let y = this.Evaluate x;
                let derivative = this.EvaluateDerivative2 x y
                (y, derivative)
            let hiddenUnit (Range y, Gradient d) = HiddenUnit (y, d)
            vector |> Array.map (fun x -> Domain x |> valueAndDerivative |> hiddenUnit) |> HiddenUnits
        member this.GenerateSignals(Vector vector) =
            let signal (Range y) = Signal y
            vector |> Array.map (fun x -> Domain x |> this.Evaluate |> signal)
