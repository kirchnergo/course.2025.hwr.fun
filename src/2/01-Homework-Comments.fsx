module Tools = 

    open System

    let randomString n = 
        let r = Random()
        let chars = Array.concat([[|'a' .. 'z'|];[|'A' .. 'Z'|];[|'0' .. '9'|]])
        let sz = Array.length chars in
        String(Array.init n (fun _ -> chars.[r.Next sz]))

randomString 20

// Two-Fer
module TwoFer = 

    let twoFer (input: string option): string = 
        match input with
            | None -> "One for you, one for me."
            | _ -> "One for " + input.Value + ", one for me." 

    let twoFer1 (input: string option): string =
        let name =
            match input with
            | None -> "you"
            | Some input -> input
        sprintf "One for %s, one for me." name

    let twoFer2 (input: string option): string =
        match input with
        | None -> "One for you, one for me."
        | Some s -> sprintf "One for %s, one for me." s

twoFer (Some (randomString 10))
randomString 10 |> Some |> twoFer
randomString 10 |> Some |> twoFer1
randomString 10 |> Some |> twoFer2

// Leap
module Leap =

    let leapYear (year: int): bool =
        let currYear: int = year
        let mutable result: bool = false
        let mutable div4: int = 0
        let mutable div100: int = 0
        let mutable div400: int = 0
        div4 <- currYear % 4
        result <- if div4 = 0 then true else false
        if (result) then
            div100 <- currYear % 100
            result <- if div100 = 0 then false else true
            if (not result) then
                div400 <- currYear % 400
                result <- if div400 = 0 then true else false
        result

    let leapYear1 (year: int): bool = 
        if year % 4 = 0 then
            if year % 100 = 0 then
                if year % 400 = 0 then
                    true
                else
                    false
            else
                true
        else
            false


    let leapYear2 (year: int): bool = 
        match (year % 4, year % 100, year % 400) with
        | (0, 0, 0) -> true
        | (0, 0, _) -> false
        | (0, _, _) -> true
        | _ -> false

    let leapYear3 (year: int): bool =
        match year with
        | _ when year % 400 = 0 -> true
        | _ when year % 100 = 0 -> false
        | _ when year % 4 = 0 -> true
        | _ -> false

// Isogram
module Isogram01 = 

    let isIsogram (str: string) = 
        let list = 
            str.ToCharArray()
            |> Array.filter System.Char.IsLetter   
            |> Array.map System.Char.ToLower
            |> Array.toList

        let distinctList = 
            list 
            |> List.distinct 
        //list.Equals(distinctList)
        list = distinctList

    #time "on"
    isIsogram "blah"
    isIsogram "test"
    isIsogram (Tools.randomString (int 100_000))
    #time "off"

module Isogram02 = 

    open System
    let isIsogram (str: string) =
        let word = str.ToUpper().ToCharArray() |> Seq.filter Char.IsLetter

        // if (Seq.length word = (Seq.distinct word |> Seq.length))
        // then true
        // else false

        //Seq.length word = (Seq.distinct word |> Seq.length)

        Seq.length word = Seq.length (Seq.distinct word)

module Isogram03 = 

    let isIsogram str = 
        let valid = [ 'A' .. 'Z' ]
        let upper = Seq.map System.Char.ToUpper str
        let upperChar = Seq.filter (fun x -> List.contains x valid) upper
        let unique = Seq.distinct upperChar
        (Seq.length upperChar = Seq.length unique)


// Sum Of Multiples
module SumOfMultiples01 = 

    let sum (numbers: int list) (upperBound: int): int =
        [ 0 .. upperBound - 1 ]
        |> List.filter (fun x -> List.exists (fun y -> y <> 0 && x % y = 0) numbers) 
        |> List.sum

sum [3;5;7;13] 100

module SumOfMultiples02 = 

    let sum (numbers: int list) (upperBound: int): int = 
        if (upperBound > 0) then
            let allMultiplesList = List.map (fun x -> (if(x > 0) then seq{0 .. x .. (upperBound - 1)} else seq{0}) |> Seq.toList) numbers
            let allMultiples = List.concat allMultiplesList |> Seq.ofList
            let uniqueMultiples = Seq.distinct allMultiples
            printfn "%A" (uniqueMultiples |> Seq.toList)
            Seq.sum uniqueMultiples
        else 0

sum [3;5;7;13] 100

module SumOfMultiples03 = 

    let sum (numbers: int list) (upperBound: int): int = 
        if upperBound > 0 && List.exists (fun x -> x > 0) numbers then
            let multiples =
                [ for particular in numbers do
                    if particular > 0 then
                        for i in 1 .. (upperBound - 1)/particular do 
                            yield particular * i ]
            Seq.sum (Seq.distinct multiples)
        else
            0

sum [3;5;7;13] 100

// Allgemein

// - Publish on exercism.io!! 
// - Vermeide "mutable"!!
// - nur wichtiges verdient einen Namen
// - Vertraue der "Pipe"!! (Einkaufswagen?) 

