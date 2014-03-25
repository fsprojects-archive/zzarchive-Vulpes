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
namespace DeepBelief.Tests

open System
open Xunit
open FsUnit.Xunit
open DeepBelief.Utils
open TestUtils

type ``Numerical Utilities``() =

    let rand = new Random()

    let M = array2D   [ [1.0f; 2.0f; 3.0f]; 
                        [4.0f; 5.0f; 6.0f] ]

    let Mt = array2D  [ [1.0f; 4.0f];
                        [2.0f; 5.0f];
                        [3.0f; 6.0f] ]

    let MTimesMt = array2D    [ [14.0f; 32.0f];
                                [32.0f; 77.0f] ]
                                                             
    let X = [| 1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f |]
    let Xconcat = [| 1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f; 1.0f; 2.0f; 3.0f; 4.0f; 5.0f; 6.0f |]

    let v = [|1.0f; 2.0f; 3.0f|]
    let w = [|4.0f; 5.0f; 6.0f|]
    
    let vPaddedTo5 = [|1.0f; 2.0f; 3.0f; 0.0f; 0.0f|]

    let vb = [| 3.0f; 2.0f; 1.0f |]
    let hb = [| 2.0f; 1.0f |]

    let MWithVisibleBiases = 
        array2D   [ [3.0f; 2.0f; 1.0f];
                    [1.0f; 2.0f; 3.0f];
                    [4.0f; 5.0f; 6.0f] ]

    let MWithHiddenBiases = 
        array2D   [ [2.0f; 1.0f; 2.0f; 3.0f];
                    [1.0f; 4.0f; 5.0f; 6.0f] ]

    let MPaddedTo5 =
        array2D   [ [1.0f; 2.0f; 3.0f; 0.0f; 0.0f];  
                    [4.0f; 5.0f; 6.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f];  
                    [0.0f; 0.0f; 0.0f; 0.0f; 0.0f] ]   
    
    let MPaddedTo3 =
        array2D   [ [1.0f; 2.0f; 3.0f;];  
                    [4.0f; 5.0f; 6.0f;];  
                    [0.0f; 0.0f; 0.0f;]; ]   

    [<Fact>] member test.
        ``The height of M is 2.``() =
            height M |> should equal 2

    [<Fact>] member test.
        ``The width of M is 3.``() =
            width M |> should equal 3

    [<Fact>] member test.
        ``Row 1 of M is the vector [4, 5, 6].``() =
            row 1 M |> should equal [|4.0f; 5.0f; 6.0f|]

    [<Fact>] member test.
        ``Column 2 of M is the vector [3, 6].``() =
            column 2 M |> should equal [|3.0f; 6.0f|]

    [<Fact>] member test.
        ``The scalar product of v and w is 32.``() =
            scalarProduct v w |> should equal 32.0f

    [<Fact>] member test.
        ``M flattens to the 1 to 6 array.``() =
            flattenMatrix M |> should equal X

    [<Fact>] member test.
        ``An array of M instances flattens to a concatenated version of the 1 to 6 array.``() =
            flattenSamples [|M; M|] |> should equal Xconcat

    [<Fact>] member test.
        ``The 1 to 6 array stacks up to M.``() =
            rebuildMatrix 3 2 3 X |> should equal M

    [<Fact>] member test.
        ``6 padded out to a multiple of 1 is 6.``() =
            nextMultipleOf 1 6 |> should equal 6

    [<Fact>] member test.
        ``6 padded out to a multiple of 3 is 6.``() =
            nextMultipleOf 3 6 |> should equal 6

    [<Fact>] member test.
        ``7 padded out to a multiple of 3 is 9.``() =
            nextMultipleOf 3 7 |> should equal 9
            
    [<Fact>] member test.
        ``Permute 10 gives an array of length 10 containing each of the digits 0 to 9.``()=
        permute rand 10 |> Array.sort |> should equal [|0..9|] 

    [<Fact>] member test.
        ``The sumOfRows function maps the identity matrix to a vector of ones.``()=
            identityMatrix 10 |> sumOfRows |> allElementsOfVector (fun i -> i = 1.0f) |> should equal true

    [<Fact>] member test.
        ``The prependRow function maps M and vb to MWithVisibleBiases.``()=
            prependRow vb M |> should equal MWithVisibleBiases

    [<Fact>] member test.
        ``The prependColumn function maps M and hb to MWithHiddenBiases.``()=
            prependColumn hb M |> should equal MWithHiddenBiases

    [<Fact>] member test.
        ``v is padded out to multiples of 5 correctly.``() =
            padToMultipleOf 5 v |> should equal vPaddedTo5

    [<Fact>] member test.
        ``v is padded out to multiples of 1 correctly.``() =
            padToMultipleOf 1 v |> should equal v

    [<Fact>] member test.
        ``M is padded out to multiples of 5 correctly.``() =
            padToMultiplesOf 5 M |> should equal MPaddedTo5

    [<Fact>] member test.
        ``M is padded out to multiples of 1 correctly.``() =
            padToMultiplesOf 1 M |> should equal M

    [<Fact>] member test.
        ``M is padded out to multiples of 3 correctly.``() =
            padToMultiplesOf 3 M |> should equal MPaddedTo3

    [<Fact>] member test.
        ``The padded versions of M reduce back to M.``() =
            (topLeftSubmatrix 2 3 MPaddedTo3, topLeftSubmatrix 2 3 MPaddedTo5) |> should equal (M, M)

    [<Fact>] member test.
        ``The batchesOf function splits 1 to 10 up correctly.``()=
        [|1..10|] |> batchesOf 3 |> should equal [|[|1;2;3|];[|4;5;6|];[|7;8;9|];[|10|]|]

    [<Fact>] member test.
        ``The transpose function transforms M to Mt and then transforms Mt back to M.``()=
            (transpose M, transpose Mt) |> should equal (Mt, M)

    [<Fact>] member test.
        ``The multiply function multiplies M by Mt.``()=
            multiply M Mt |> should equal MTimesMt

    [<Fact>] member test.
        ``The transposeAndMultiply function transposes Mt to M and multiplies it by Mt.``()=
            transposeAndMultiply Mt Mt |> should equal MTimesMt

    [<Fact>] member test.
        ``The multiplyByTranspose function transposes M to Mt and multiplies it by Mt.``()=
            multiplyByTranspose M M |> should equal MTimesMt
            
    [<Fact>] member test.
        ``The permuteRows method preserves the dimensions of the batch matrix.``()=
        permuteRows rand Mt |> (fun x -> (x.Length, x.[0].Length)) |> should equal (3, 2)

    [<Fact>] member test.
        ``The proportionOfVisible units function gives 0.2 for the vector [0,1,0,0,0,0,0,1,0,0]``()=
            [| 0.0f; 1.0f ;0.0f ;0.0f ;0.0f ;0.0f ;0.0f ;1.0f ;0.0f ;0.0f |] 
            |> proportionOfVisibleUnits |> should equal 0.2f

    [<Fact>] member test.
        ``The toColumns function breaks Mt up into columns v and w.``()=
            toColumns Mt |> should equal [|v; w|]
    
    [<Fact>] member test.
        ``The error function gives zero for the vectors [1,0,0] and [1,0,0].``()=
            error [|1.0f; 0.0f; 0.0f|] [|1.0f; 0.0f; 0.0f|] |> should equal 0.0f    
   
    [<Fact>] member test.
        ``The error function gives one for the vectors [1,0,0] and [0,0,1].``()=
            error [|1.0f; 0.0f; 0.0f|] [|0.0f; 0.0f; 1.0f|] |> should equal 1.0f