namespace DeepBelief.Tests

open System
open Xunit
open FsUnit.Xunit
open Common.Analytics
open DeepBelief.CudaTemplates
open DeepBelief.Utils
open TestUtils

type ``Numerical Utilities``() =

    let rand = new Random()

    let M = array2D   [ [1.0f; 2.0f; 3.0f]; 
                        [4.0f; 5.0f; 6.0f] ] |> Matrix

    let Mt = array2D  [ [1.0f; 4.0f];
                        [2.0f; 5.0f];
                        [3.0f; 6.0f] ] |> Matrix

    let MTimesMt = array2D    [ [14.0f; 32.0f];
                                [32.0f; 77.0f] ] |> Matrix
                                                             
    let X = [| 1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f |] |> Vector
    let Xconcat = [| 1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f; 1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f |] |> Vector

    let v = [|1.0f; 2.0f; 3.0f|] |> Vector
    let w = [|4.0f; 5.0f; 6.0f|] |> Vector
    
    let vPaddedTo5 = [|1.0f; 2.0f; 3.0f; 0.0f; 0.0f|] |> Vector

    let vb = [| 3.0f; 2.0f; 1.0f |] |> Vector
    let hb = [| 2.0f; 1.0f |] |> Vector

    let MWithVisibleBiases = 
        array2D   [ [3.0f; 2.0f; 1.0f];
                    [1.0f; 2.0f; 3.0f];
                    [4.0f; 5.0f; 6.0f] ] |> Matrix

    let MWithHiddenBiases = 
        array2D   [ [2.0f; 1.0f; 2.0f; 3.0f];
                    [1.0f; 4.0f; 5.0f; 6.0f] ] |> Matrix

    let MPaddedTo5 =
        array2D   [ [1.0f; 2.0f; 3.0f; 0.0f; 0.0f];  
                    [4.0f; 5.0f; 6.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f] ] |> Matrix   
    
    let MPaddedTo3 =
        array2D   [ [1.0f; 2.0f; 3.0f;];  
                    [4.0f; 5.0f; 6.0f;];  
                    [0.0f; 0.0f; 0.0f;]; ] |> Matrix

    [<Fact>] member test.
        ``The height of M is 2.``() =
            M.Height |> should equal 2

    [<Fact>] member test.
        ``The width of M is 3.``() =
            M.Width |> should equal 3

    [<Fact>] member test.
        ``Row 1 of M is the vector [4, 5, 6].``() =
            M.Row 1 |> should equal <| Vector [|4.0f; 5.0f; 6.0f|]

    [<Fact>] member test.
        ``Column 2 of M is the vector [3, 6].``() =
            M.Column 2 |> should equal <| Vector [|3.0f; 6.0f|]

    [<Fact>] member test.
        ``M flattens to the 1 to 6 array.``() =
            M.ToRowMajorFormat |> Vector |> should equal X

    [<Fact>] member test.
        ``The 1 to 6 array stacks up to M.``() =
            rebuildMatrix 3 2 3 X |> should equal M

    [<Fact>] member test.
        ``The prependRow function maps M and vb to MWithVisibleBiases.``()=
            M.PrependRow vb |> should equal MWithVisibleBiases

    [<Fact>] member test.
        ``The prependColumn function maps M and hb to MWithHiddenBiases.``()=
            M.PrependColumn hb |> should equal MWithHiddenBiases
//
//    [<Fact>] member test.
//        ``M is padded out to multiples of 5 correctly.``() =
//            M.PadToMultiplesOf 5 |> should equal MPaddedTo5
//
//    [<Fact>] member test.
//        ``M is padded out to multiples of 1 correctly.``() =
//            padToMultiplesOf 1 M |> should equal M
//
//    [<Fact>] member test.
//        ``M is padded out to multiples of 3 correctly.``() =
//            padToMultiplesOf 3 M |> should equal MPaddedTo3
//
//    [<Fact>] member test.
//        ``The padded versions of M reduce back to M.``() =
//            (topLeftSubmatrix 2 3 MPaddedTo3, topLeftSubmatrix 2 3 MPaddedTo5) |> should equal (M, M)
//
//    [<Fact>] member test.
//        ``The batchesOf function splits 1 to 10 up correctly.``()=
//        [|1..10|] |> batchesOf 3 |> should equal [|[|1;2;3|];[|4;5;6|];[|7;8;9|];[|10|]|]

//    [<Fact>] member test.
//        ``The transpose function transforms M to Mt and then transforms Mt back to M.``()=
//            (transpose M, transpose Mt) |> should equal (Mt, M)
//
//    [<Fact>] member test.
//        ``The multiply function multiplies M by Mt.``()=
//            multiply M Mt |> should equal MTimesMt
//
//    [<Fact>] member test.
//        ``The transposeAndMultiply function transposes Mt to M and multiplies it by Mt.``()=
//            transposeAndMultiply Mt Mt |> should equal MTimesMt
//
//    [<Fact>] member test.
//        ``The multiplyByTranspose function transposes M to Mt and multiplies it by Mt.``()=
//            multiplyByTranspose M M |> should equal MTimesMt
//            
//    [<Fact>] member test.
//        ``The proportionOfVisible units function gives 0.2 for the vector [0,1,0,0,0,0,0,1,0,0]``()=
//            [| 0.0f; 1.0f ;0.0f ;0.0f ;0.0f ;0.0f ;0.0f ;1.0f ;0.0f ;0.0f |] 
//            |> proportionOfVisibleUnits |> should equal 0.2f
//
//    [<Fact>] member test.
//        ``The toColumns function breaks Mt up into columns v and w.``()=
//            toColumns Mt |> should equal [|v; w|]
//    
//    [<Fact>] member test.
//        ``The error function gives zero for the vectors [1,0,0] and [1,0,0].``()=
//            error [|1.0f; 0.0f; 0.0f|] [|1.0f; 0.0f; 0.0f|] |> should equal 0.0f    
//   
//    [<Fact>] member test.
//        ``The error function gives one for the vectors [1,0,0] and [0,0,1].``()=
//            error [|1.0f; 0.0f; 0.0f|] [|0.0f; 0.0f; 1.0f|] |> should equal 1.0f
//
//    [<Fact>] member test.
//        ``The scaled learning rate divides the learning rate by the specified integer.``()=
//            let learningRate = LearningRate 10.0f in
//            learningRate / 10 |> should equal <| ScaledLearningRate 1.0f