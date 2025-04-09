// =============================================
// Section 1 - Parse a hard-coded character
// =============================================

module Section1 =
    open System

    let A_Parser str =
        if String.IsNullOrEmpty(str) then
            (false,"")
        else if str.[0] = 'A' then
            let remaining = str.[1..]
            (true,remaining)
        else
            (false,str)

    // --------- Signature of "A_Parser" ---------     
    // val A_Parser :
    //     string -> (bool * string)
    // ------------------------------------------- 

    let inputABC = "ABC"
    A_Parser inputABC    // (true, "BC")

    let inputZBC = "ZBC"
    A_Parser inputZBC    // (false, "ZBC")

// =============================================
// Section 2 - Match a specified character
// =============================================

module Section2 =
    open System

    // parse a single character
    let pchar (charToMatch,str) =
        if String.IsNullOrEmpty(str) then
            let msg = "No more input"
            (msg,"")
        else 
            let first = str.[0] 
            if first = charToMatch then
                let remaining = str.[1..]
                let msg = sprintf "Found %c" charToMatch
                (msg,remaining)
            else
                let msg = sprintf "Expecting '%c'. Got '%c'" charToMatch first
                (msg,str)

    // --------- Signature of "pchar" ---------     
    // val pchar :
    //     (char * string) -> (string * string)
    // ---------------------------------------------


    // --------- 
    // test
    // --------- 

    let inputABC = "ABC"
    pchar('A',inputABC)  // ("Found A", "BC")

    let inputZBC = "ZBC"
    pchar('A',inputZBC)  // ("Expecting 'A'. Got 'Z'", "ZBC")
    pchar('Z',inputZBC)  // ("Found Z", "BC")

// =============================================
// Section 3 - Return a Result<char*string>
// =============================================

module Section3 =
    open System

    // parse a single character
    let pchar (charToMatch:char, str:string):Result<(char*string), string> = 
        if String.IsNullOrEmpty(str) then
            Error "No more input"
        else
            let first = str.[0] 
            if first = charToMatch then
                let remaining = str.[1..]
                Ok (charToMatch, remaining)
            else
                let msg = sprintf "Expecting '%c'. Got '%c'" charToMatch first
                Error msg

    // --------- Signature of "pchar" ---------     
    // pchar :
    //     (char * string) -> Result<char * string>
    // ---------------------------------------------

    // ---------  
    // test "pchar"
    // --------- 

    let inputABC = "ABC"
    pchar('A',inputABC)  // Ok ('A', "BC")

    let inputZBC = "ZBC"
    pchar('A',inputZBC)  // Error "Expecting 'A'. Got 'Z'"
    pchar('Z',inputZBC)  // Ok ('Z', "BC")

// =============================================
// currying examples
// =============================================

(*
module CurryingExamples =

    let add x y = 
        x + y

    let add x = 
        fun y -> x + y  // return a lambda

    let add x = 
        let innerFn y = x + y
        innerFn // return innerFn 

    let add3 x y z = 
        x + y + z

    let add3 x = 
        fun y -> 
            (fun z -> x + y + z)
*)

// =============================================
// Section 4 - Use currying 
// =============================================

module Section4a =
    open System

    // parse a single character
    let pchar charToMatch str = 
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

    // --------- Signature of "pchar" ---------     
    // pchar :
    //     char -> string -> Result<char * string>
    // ---------------------------------------------

    let parseA = pchar 'A' 

    // --------- Signature of "parseA" ---------     
    // parseA :
    //     string -> Result<char * string>
    // ---------------------------------------------

    // ---------  
    // test "parseA"
    // --------- 

    let inputABC = "ABC"
    let inputZBC = "ZBC"
    parseA inputABC  // Ok ('A', "BC")
    parseA inputZBC  // Error "Expecting 'A'. Got 'Z'"

    let parseZ = pchar 'Z' 
    parseZ inputZBC  // Ok ('Z', "BC")


module Section4b =
    open System

    // parse a single character
    // same as Section4a, but with explicit inner function
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
        // return the inner function
        innerFn 

    // --------- Signature of "pchar" ---------     
    // SAME signature as version 4a!
    // pchar :
    //     char -> string -> Result<char * string>
    // ---------------------------------------------

    let parseA = pchar 'A' 

    // --------- Signature of "parseA" ---------     
    // SAME signature as version 4a!
    // parseA :
    //     string -> Result<char * string>
    // ---------------------------------------------
    
    // ---------  
    // test "parseA"
    // --------- 

    let inputABC = "ABC"
    parseA inputABC  // Ok ('A', "BC")

    let inputZBC = "ZBC"
    parseA inputZBC  // Error "Expecting 'A'. Got 'Z'"

    let parseZ = pchar 'Z' 
    parseZ inputZBC  // Ok ('Z', "BC")

// =============================================
// Section 5 - Create a type to wrap the parser function
// =============================================

module Section5 =
    open System

    /// Type that wraps a parsing function
    type Parser<'T> = Parser of (string -> Result<'T , string>)

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

    // --------- Signature of "parseA" ---------     
    // pchar :
    //    char -> Parser<char * string>
    // -----------------------------------------

    let parseA = pchar 'A' 

    // --------- Signature of "parseA" ---------     
    // parseA :
    //     Parser<char * string>
    // ---------------------------------------------

    (*
    let inputABC = "ABC"
    parseA inputABC  // compiler error 
                     // error FS0003: This value is not a function and cannot be applied
    
    // so we need to create a helper function
    *)

    // ============================
    // Introducing "run"
    // ============================

    /// Run a parser with some input
    let run parser input = 
        // unwrap parser to get inner function
        let (Parser innerFn) = parser 
        // call inner function with input
        innerFn input

    // --------- Signature of "run" ---------     
    // run : 
    //    parser:Parser<'a> -> input:string -> Result<'a,string>
    // --------------------------------------

    // ---------  
    // test "run"
    // --------- 
    module Test_Run =

        let parseA = pchar 'A' 

        let inputABC = "ABC"
        run parseA inputABC  // Ok ('A', "BC")

        let inputZBC = "ZBC"
        run parseA inputZBC  // Error "Expecting 'A'. Got 'Z'"

// =============================================
// Section 6 - Combining two parsers in sequence: the "and then" combinator
// =============================================

    (*
    // given two parsers...
    let parseA = pchar 'A'   // Parser<char>
    let parseB = pchar 'B'
    
    // ...how to compose the two functions?
    let parseAThenB = parseA >> parseB  // compiler error
    *)

    // ============================
    // Implementing "andThen" 
    // ============================

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

    // --------- Signature of "andThen" --------- 
    // val andThen : 
    //     parser1:Parser<'a * string> -> parser2:Parser<'b * 'c> -> Parser<('a * 'b) * 'c>
    // ------------------------------------------ 

    // ---------  
    // test .>>. 
    // --------- 

    module Test_AndThen =

        let parseA = pchar 'A'   // Parser<char>
        let parseB = pchar 'B'
        let parseAThenB = parseA .>>. parseB 

        run parseAThenB "ABC"  // Ok (('A', 'B'), "C")
        run parseAThenB "ZBC"  // Error "Expecting 'A'. Got 'Z'"
        run parseAThenB "AZC"  // Error "Expecting 'B'. Got 'Z'"



// =============================================
// Section 7 - Choosing between two parsers: the "or else" combinator
// =============================================


    // ============================
    // Implementing "orElse" 
    // ============================

    /// Combine two parsers as "A orElse B"
    let orElse parser1 parser2 =
        let innerFn input =
            // run parser1 with the input
            let result1 = run parser1 input

            // test the result for Error/Ok
            match result1 with
            | Ok result -> 
                // if success, return the original result
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

    // --------- Signature of "orElse" --------- 
    // val orElse : 
    //     parser1:Parser<'a> -> parser2:Parser<'a> -> Parser<'a>
    // ------------------------------------------ 


    // ---------  
    // test <|>
    // --------- 
    module Test_OrElse =
        let parseA = pchar 'A'   
        let parseB = pchar 'B'
    
        let parseAOrElseB = parseA <|> parseB 

        run parseAOrElseB "AZZ"  // Success ('A', "ZZ")
        run parseAOrElseB "BZZ"  // Success ('B', "ZZ")
        run parseAOrElseB "CZZ"  // Failure "Expecting 'B'. Got 'C'"

        // ---------  
        // combining "AndThen" and "OrElse"
        // --------- 

        let parseC = pchar 'C'
        let bOrElseC = parseB <|> parseC
        let aAndThenBorC = parseA .>>. bOrElseC 

        run aAndThenBorC "ABZ"  // Success (('A', 'B'), "Z")
        run aAndThenBorC "ACZ"  // Success (('A', 'C'), "Z")
        run aAndThenBorC "QBZ"  // Failure "Expecting 'A'. Got 'Q'"
        run aAndThenBorC "AQZ"  // Failure "Expecting 'C'. Got 'Q'"


// =============================================
// Section 8 - Choosing from a list of parsers: "choice" and "anyOf"
// =============================================

    // ============================
    // Introducing "choice" 
    // ============================

    /// Choose any of a list of parsers
    let choice listOfParsers = 
        List.reduce ( <|> ) listOfParsers 

    // --------- Signature of "choice" --------- 
    // val choice : 
    //     listOfParsers:Parser<'a> list -> Parser<'a>
    // ------------------------------------------ 

    // ---------  
    // test "choice"
    // --------- 
    module Test_Choice =
        let digitChars = ['0'..'9']

        // map each char to a Parser using pchar
        let digitParsers = 
            List.map pchar digitChars 
            // Parser<char> list 

        // combine all parsers using choice
        let parseDigit = choice digitParsers 
           // Parser<char>
    
        run parseDigit  "1ZZ"  // Success ('1', "ZZ")
        run parseDigit  "2ZZ"  // Success ('2', "ZZ")
        run parseDigit  "9ZZ"  // Success ('9', "ZZ")
        run parseDigit  "AZZ"  // Failure "Expecting '9'. Got 'A'"

    // ============================
    // Introducing "anyOf"
    // ============================

    /// Choose any of a list of characters
    let anyOf listOfChars = 
        listOfChars
        |> List.map pchar // convert into parsers
        |> choice

    // --------- Signature of "anyOf" --------- 
    // val anyOf : 
    //     listOfChars:char list -> Parser<char>
    // ------------------------------------------ 

    // ---------  
    // test "anyOf"
    // --------- 
    module Test_AnyOf =

        let parseLowercase = 
            anyOf ['a'..'z']

        let parseDigit = 
            anyOf ['0'..'9']

        run parseLowercase "aBC"  // Ok ('a', "BC")
        run parseLowercase "ABC"  // Error "Expecting 'z'. Got 'A'"

        run parseDigit "1ABC"  // Ok ("1", "ABC")
        run parseDigit "9ABC"  // Ok ("9", "ABC")
        run parseDigit "|ABC"  // Error "Expecting '9'. Got '|'"
