open System

// Partial Active Patterns

// Partial active patterns return an option as output because only 
// a subset of the possible input values will produce a positive match.
// One area where Partial Active Patterns are really helpful is for validation and parsing. 


// This is an example of parsing a string to a DateTime using a function rather than an active pattern:

// string -> DateTime option
let parse (input:string) =
    match DateTime.TryParse(input) with 
    | true, value -> Some value
    | false, _ -> None
let isDate = parse "2019-12-20" // Some 2019-12-20 00:00:00 
let isNotDate = parse "Hello" // None


// It works but we can’t re-use the function body logic directly in a pattern match, except in the guard clause. 
// Let’s create a partial active pattern to handle the DateTime parsing for us:
let (|ValidDate|_|) (input:string) = 
    match DateTime.TryParse(input) with 
    | true, value -> Some value
    | false, _ -> None

let parse' input =
    match input with
    | ValidDate dt -> printfn "%A" dt
    | _ -> printfn $"'%s{input}' is not a valid date"

parse' "2019-12-20" // 2019-12-20 00:00:00 
// string -> unit
parse' "Hello" // 'Hello' is not a valid date



// Parameterized Partial Active Patterns

// Parameterized partial active patterns differ from basic partial active patterns 
// by supplying additional input items.

let calculate i =
    if i % 3 = 0 && i % 5 = 0 
    then "FizzBuzz" 
    elif i % 3 = 0 then "Fizz"
    elif i % 5 = 0 then "Buzz"
    else i |> string

[1..15] |> List.map calculate

// It works but can we do better with Pattern Matching? How about this?
let calculate' i =
    match (i % 3, i % 5) with 
    | (0, 0) -> "FizzBuzz"
    | (0, _) -> "Fizz"
    | (_, 0) -> "Buzz"
    | _ -> i |> string

[1..15] |> List.map calculate'

// How about if we could use an active pattern to do something like this?

// int -> int -> unit option
let (|IsDivisibleBy|_|) divisor n =
    if n % divisor = 0 then Some () else None

let calculate'' i = 
    match i with
    | IsDivisibleBy 3 & IsDivisibleBy 5 -> "FizzBuzz" | IsDivisibleBy 3 -> "Fizz"
    | IsDivisibleBy 5 -> "Buzz"
    | _ -> i |> string

[1..15] |> List.map calculate''

let (|IsDivisibleBy|_|) divisors n =
    if divisors |> List.forall (fun div -> n % div = 0) then Some () else None
let calculate i = 
    match i with
    | IsDivisibleBy [3;5;7] -> "FizzBuzzBazz"
    | IsDivisibleBy [3;5] -> "FizzBuzz"
    | IsDivisibleBy [3;7] -> "FizzBazz"
    | IsDivisibleBy [5;7] -> "BuzzBazz"
    | IsDivisibleBy [3] -> "Fizz" 
    | IsDivisibleBy [5] -> "Buzz" 
    | IsDivisibleBy [7] -> "Bazz" 
    | _ -> i |> string

[100..135] |> List.map calculate

let calculate mapping n = 
    mapping
    |> List.map (fun (divisor, result) -> if n % divisor = 0 then result else "") 
    |> List.reduce (+)
    |> fun input -> if input = "" then string n else input

[100..135] |> List.map (calculate [(3, "Fizz"); (5, "Buzz"); (7, "Bazz"); (11, "Bizz")])