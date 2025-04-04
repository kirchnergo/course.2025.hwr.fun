open System
#r "../src/4/02_PBT/lib/Expecto.dll"
open Expecto
let expectoConfig = {defaultConfig with colour = Expecto.Logging.ColourLevel.Colour0}
#load "../src/4/02_PBT/A1_Add_Implementations.fsx"
open A1_Add_Implementations

module Test1 =
    open Implementation1
    let tests = testList "implementation 1" [
        test "add 1 3 = 4" {
            let actual = add 1 3
            let expected = 4
            Expect.equal expected actual "" }
        test "add 2 2 = 4" {
            let actual = add 2 2
            let expected = 4
            Expect.equal expected actual "" } ];;
runTests expectoConfig Test1.tests

module Implementation1 =
    let add x y =
        4

open System
#r "../src/4/02_PBT/lib/FsCheck.dll"

let add1 x y = x + y
let add2 x y = x - y
let commutativeProperty f x y =
   let result1 = f x y
   let result2 = f y x
   result1 = result2;;
FsCheck.Check.Quick (commutativeProperty add1)
FsCheck.Check.Quick (commutativeProperty add2)

type Temp = F of int | C of float;;
let fGen =
    FsCheck.Gen.choose(32,212)
    |> FsCheck.Gen.map (fun i -> F i);;
let cGen =
    FsCheck.Gen.choose(0,100)
    |> FsCheck.Gen.map (fun i -> C (float i));;
let tempGen =
    FsCheck.Gen.oneof [fGen; cGen]

let test = tempGen |> FsCheck.Gen.sample 0 20
test

open FsCheck
let smallerThan81Property x = x < 81
FsCheck.Check.Quick smallerThan81Property

let test1 = FsCheck.Arb.shrink 100 |> Seq.toList
let test2 = FsCheck.Arb.shrink 88 |> Seq.toList
test2
