let twoFer (input: string option): string = 
    input 
    |> Option.defaultValue "you"
    |> sprintf "One for %s, one for me."

let test1 = [twoFer None; twoFer (Some "Alice"); twoFer (Some "Bob")]

let (|IsDivisibleBy|_|) divisor n =
    if n % divisor = 0 then Some () else None

let leapYear year =
    match year with
    | IsDivisibleBy 400 -> true
    | IsDivisibleBy 100 -> false
    | IsDivisibleBy   4 -> true
    | _ -> false

let test1 = [leapYear 1900; leapYear 1996]
let test2 = [leapYear 2000; leapYear 2019; leapYear 2020]

let isIsogram (str: string) =
    let letters =
        str.ToLowerInvariant()
        |> Seq.filter System.Char.IsLetter
        |> Seq.toList
    letters
    |> Seq.distinct
    |> Seq.length
    |> (=) letters.Length
let test1 = [isIsogram ""; isIsogram "isogram"]
let test2 = [isIsogram "eleven"; isIsogram "subdermatoglyphic"]

let multiplesOf max n =
    if n = 0 then [0] else [n .. n .. (max - 1)]
let sum (numbers: int list) (upperBound: int): int =
    numbers
    |> List.collect (multiplesOf upperBound)
    |> List.distinct
    |> List.sum
#time "on"
let test = [sum [3; 5] 1000; sum [2; 3; 5; 7; 11] 10000]
#time "off"

let rec length' list =
    match list with
    | [] -> 0
    | _::xs -> 1 + length' xs
let length list =
    let rec _length list acc =
        match list with
        | [] -> acc
        | _::xs -> _length xs (acc + 1)
    _length list 0

let test1 = [length' []; length' [1; 2; 3; 4]]
let test2 = [length []; length [1; 2; 3; 4]]

let reverse list =
    let rec _reverse list acc =
        match list with
        | [] -> acc
        | x::xs -> _reverse xs (x::acc)
    _reverse list []

let test1 = reverse [1; 3; 5; 7]
let test2 = reverse [[1; 2]; [3]; []; [4..8]]

let map f list = 
    let rec _map f list acc =
        match list with
        | [] -> acc |> reverse
        | x::xs -> _map f xs ((f x)::acc)
    _map f list []   

let test = map (fun x -> x + 1) [1; 3; 5; 7]

// filter : f:('a -> bool) -> list:'a list -> 'a list
let filter f list =
    ...
    match list with
    | [] -> ...
    | x::xs -> ...

let test = filter (fun x -> x % 2 = 1) [1..1000]

let rec filter f list = 
    match list with
    | [] -> []
    | x::xs ->
        match f x with
        | true -> x :: filter f xs
        | false -> filter f xs
let test = filter (fun x -> x % 2 = 1) [1..10_000]

let filter f list = 
    let rec _filter f list acc = 
        match list with
        | [] -> acc |> reverse
        | x::xs ->
            match f x with
            | true -> _filter f xs (x::acc)
            | false -> _filter f xs acc
    _filter f list []
let test = filter (fun x -> x % 2 = 1) [1..10_000]

#time "on"
let answer = 5I **(int (4I ** (int (3I ** 2))));;
let sans = answer.ToString()
let l = sans.Length
let prefix = sans.Substring(0,20)
let suffix = sans.Substring(l-20)
#time "off"
printfn "Length = %d, digits %s ... %s" l prefix suffix

let rec foldl folder state list = 
    match list with
    | [] -> state
    | x::xs -> foldl folder (folder state x) xs

let test1 = foldl (+) 0 [1..1_000]
let test2 = foldl (*) 1I [1I..42I]

let flip f b a = f a b 
let rec foldr folder state list = 
    foldl (flip folder) state (reverse list)

let test = foldr (+) 5 [1; 2; 3; 4]

let append xs ys = foldr (fun x acc -> x :: acc) ys xs

let test = append [1..5] [6..10]

let concat xs = foldr append [] xs
let rec concat' xs = 
    match xs with
    | [] -> []
    | []::ys -> concat' ys
    | (x::xs)::ys -> x:: (concat' (xs::ys))
let concat'' xs =
    let rec _concat xs acc = 
        match xs with
        | [] -> acc |> reverse
        | []::ys -> _concat ys acc
        | (x::xs)::ys -> _concat (xs::ys) (x::acc)
    _concat xs []

let test1 = concat [[1; 2]; [3]; []; [4; 5; 6]]
let test2 = concat' [[1; 2]; [3]; []; [4; 5; 6]]
let test3 = concat'' [[1; 2]; [3]; []; [4; 5; 6]]

let test1b = concat [[[1]; [2]]; [[3]]; [[]]; [[4; 5; 6]]]
let test2b = concat' [[[1]; [2]]; [[3]]; [[]]; [[4; 5; 6]]] 
let test3b = concat'' [[[1]; [2]]; [[3]]; [[]]; [[4; 5; 6]]]

type BillingDetails = { 
    name : string
    billing :  string
    delivery : string option }
let order1 = {
    name = "Adam Smith"
    billing = "112 Fibonacci Street\n35813" 
    delivery = None }
let order2 = {
    name = "John Doe"
    billing = "314 Pi Avenue\n35999"
    delivery = Some "16 Planck Parkway\n62291" }

let addressForPackage (details : BillingDetails) = 
    let address =
        match details.delivery with 
        | Some s -> s
        | None -> details.billing
    sprintf "%s\n%s" details.name address
printfn "%s" (addressForPackage order1)
printfn "%s" (addressForPackage order2)

open System
let tryLastLine (address : string) = 
    let parts = address.Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries)
    parts |> Array.tryLast
let tryPostalCode (codeString : string) = 
    match Int32.TryParse(codeString) with 
    | true, i -> i |> Some
    | false, _ -> None
let postalCodeHub (code : int) = 
    if code = 62291 then "Hub 1" else "Hub 2"
let tryHub (details : BillingDetails) = 
    details.delivery
    |> Option.bind tryLastLine 
    |> Option.bind tryPostalCode 
    |> Option.map postalCodeHub

let test1 = order1 |> tryHub
let test2 = order2 |> tryHub

open System
let checkString (s : string) =
    if isNull(s) then
        raise <| ArgumentNullException("Must not be null")
    elif String.IsNullOrEmpty(s) then
        raise <| ArgumentException("Must not be empty")
    elif String.IsNullOrWhiteSpace(s) then
        raise <| ArgumentException("Must not be white space")
    else
        s
//checkString null
//checkString ""
checkString " "

open System
let notEmpty (s : string) =
    if isNull(s) then Error "Must not be null"
    elif String.IsNullOrEmpty(s) then Error "Must not be empty"
    elif String.IsNullOrWhiteSpace(s) then Error "Must not be white space"
    else Ok s
let t1 = notEmpty null;;
let t2 = notEmpty "";;
let t3 = notEmpty " ";;
t1, t2, t3

open System
type ValidationError =
    | MustNotBeNull
    | MustNotBeEmpty
    | MustNotBeWhiteSpace
let notEmpty (s : string) =
    if isNull(s) then Error MustNotBeNull
    elif String.IsNullOrEmpty(s) then Error MustNotBeEmpty
    elif String.IsNullOrWhiteSpace(s) then Error MustNotBeWhiteSpace
    else Ok s
let t1 = notEmpty null;;
let t2 = notEmpty "";;
let t3 = notEmpty " ";;
notEmpty, t1, t2, t3
