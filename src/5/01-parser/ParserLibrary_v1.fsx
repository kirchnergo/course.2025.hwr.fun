open System

/// Type that wraps a parsing function
type Parser<'T> = Parser of (string -> Result<'T, string>)

/// Parse a single character
let pchar charToMatch = 
    // define a nested inner function
    let innerFn str =
        if String.IsNullOrEmpty(str) then
            Error "No more input"
        else
            let first = str.[0] 
            if first = charToMatch then
                let remaining = str.[1..]
                Ok (charToMatch,remaining)
            else
                let msg = sprintf "Expecting '%c'. Got '%c'" charToMatch first
                Error msg
    // return the "wrapped" inner function
    Parser innerFn 

/// Run a parser with some input
let run parser input = 
    // unwrap parser to get inner function
    let (Parser innerFn) = parser 
    // call inner function with input
    innerFn input

/// Combine two parsers as "A andThen B"
let andThen parser1 parser2 =
    let innerFn input =
        // run parser1 with the input
        let result1 = run parser1 input
            
        // test the result for Error/Ok
        match result1 with
        | Error err -> 
            // return error from parser1
            Error err  

        | Ok (value1,remaining1) -> 
            // run parser2 with the remaining input
            let result2 =  run parser2 remaining1
                
            // test the result for Error/Ok
            match result2 with 
            | Error err ->
                // return error from parser2 
                Error err 
                
            | Ok (value2,remaining2) -> 
                // combine both values as a pair
                let newValue = (value1,value2)
                // return remaining input after parser2
                Ok (newValue,remaining2)

    // return the inner function
    Parser innerFn 

/// Infix version of andThen
let ( .>>. ) = andThen

/// Combine two parsers as "A orElse B"
let orElse parser1 parser2 =
    let innerFn input =
        // run parser1 with the input
        let result1 = run parser1 input

        // test the result for Error/Ok
        match result1 with
        | Ok result -> 
            // if Ok, return the original result
            result1

        | Error err -> 
            // if failed, run parser2 with the input
            let result2 = run parser2 input

            // return parser2's result
            result2 

    // return the inner function
    Parser innerFn 

/// Infix version of orElse
let ( <|> ) = orElse

/// Choose any of a list of parsers
let choice listOfParsers = 
    List.reduce ( <|> ) listOfParsers 

/// Choose any of a list of characters
let anyOf listOfChars = 
    listOfChars
    |> List.map pchar // convert into parsers
    |> choice
