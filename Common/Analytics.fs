namespace Common

module Analytics =
    
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
        static member (.*) (Vector lhs, Vector rhs) =
            Array.map2 (*) lhs rhs |> Vector
        static member (-) (Vector lhs, Vector rhs) =
            Array.map2 (-) lhs rhs |> Vector

    type [<ReflectedDefinition>] Matrix = Matrix of float32[,] with
        static member (+) (Matrix lhs, Matrix rhs) =
            let h = height lhs
            let w = width rhs
            Array2D.init h w (fun i j -> lhs.[i, j] + rhs.[i, j])
            |> Matrix
        static member (-) (Matrix lhs, Matrix rhs) =
            let h = height lhs
            let w = width rhs
            Array2D.init h w (fun i j -> lhs.[i, j] - rhs.[i, j])
        static member (!*) (Vector v1, Vector v2) =
            Array2D.init (Array.length v1) (Array.length v2) (fun i j -> v1.[i] * v2.[j])
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
        static member (*) (Matrix A, Matrix B) =
            let row i (M : float32[,]) = Array.init (width M) (fun j -> M.[i, j])
            let column j (M : float32[,]) = Array.init (height M) (fun i -> M.[i, j])
            let h = height A
            let w = width B
            let rowsOfA = [|0..h - 1|] |> Array.map (fun i -> row i A)
            let columnsOfB = [|0..w - 1|] |> Array.map (fun j -> column j A)
            Array2D.init h w (fun i j -> Array.map2 (*) rowsOfA.[i] columnsOfB.[j] |> Array.sum)
            |> Matrix
        static member (^*) (Matrix A, Vector v) =
            let column j (M : float32[,]) = Array.init (height M) (fun i -> M.[i, j])
            let h = height A
            let w = width A
            let columnsOfA = [|0..w - 1|] |> Array.map (fun j -> column j A)
            Array.init w (fun j -> Array.map2 (*) columnsOfA.[j] v |> Array.sum) 
            |> Vector

    type Error = Error of float32
    
    type ErrorSignal = ErrorSignal of float32

    type ErrorSignals = ErrorSignals of ErrorSignal[]

    type Errors = Errors of Error[]

    type Signal = Signal of (float32 * float32 option) with
        static member (-) (target : float32, Signal signal) =
            target - fst signal

    type Signals = Signals of Signal[] with
        member signals.ValuesPrependedForBias = 
            match signals with (Signals signalsArray) -> 1.0f :: List.ofArray (Array.map (fun (Signal (x, d)) -> x) signalsArray) |> Array.ofList |> Vector
        static member (-) (Vector target, Signals signals) =
            signals |> Array.map (fun (Signal signal) -> fst signal) |> Array.map2 (-) target |> Array.map Error |> Errors

    type Errors with
        member this.ToErrorSignals (Signals signalsArray) =
            0

    type WeightsAndBiases = WeightsAndBiases of Matrix with
        static member (*) (WeightsAndBiases weightsAndBiases, Signals signalsArray) =
            let prependedSignals = 1.0f :: List.ofArray (Array.map (fun (Signal (x, d)) -> x) signalsArray) |> Array.ofList |> Vector
            weightsAndBiases * prependedSignals

    type DifferentiableFunction with
        member this.GenerateSignals(Vector vector) =
            let valueAndDerivative x =
                let y = this.Evaluate x;
                let derivative = this.EvaluateDerivative2 x y
                (y, derivative)
            let signal (Range y, Gradient d) =
                Signal (y, Some d)
            vector |> Array.map (fun x -> Domain x |> valueAndDerivative |> signal) |> Signals

    let prepend value (Vector vector) = value :: List.ofArray vector |> Array.ofList |> Vector

    let activate (Vector value, Vector derivative) (activation : DifferentiableFunction) =
        value |> Array.map (fun x -> activation.Evaluate (Domain x))