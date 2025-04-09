// =============================================
// The basic parser library from part 1
// =============================================

module ParserLibrary1 =
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

// =============================================
// Beginning of code used in part 2
// =============================================

module CodeForPart2 =
    open System
    open ParserLibrary1

// =============================================
// Section 2.1 - transforming a parser with "map"
// =============================================

    // ============================
    // How to handle sequences of parsers?
    // ============================

    module ParseDigits_1 = 
        let parseDigit = anyOf ['0'..'9']

        // combine all parsers using andThen
        let parseTwoDigits = 
            parseDigit .>>. parseDigit 
        run parseTwoDigits "12A"  // Ok (('1', '2'), "A")

        let parseThreeDigits = 
            parseDigit .>>. parseDigit .>>. parseDigit 
        
        run parseThreeDigits "123A"  // Ok ((('1', '2'), '3'), "A")

    (*
    The tuple inside of Ok ((('1', '2'), '3'), "A") is ugly -- let's turn it into a string
    *)


    // ============================
    // Introducing "mapP"
    // ============================

    /// apply a function to the value inside a parser
    let mapP f parser = 
        let innerFn input =
            // run parser with the input
            let result = run parser input

            // test the result for Error/Ok
            match result with
            | Ok (value,remaining) -> 
                // if Ok, return the value transformed by f
                let newValue = f value
                Ok (newValue, remaining)

            | Error err -> 
                // if failed, return the error
                Error err
        // return the inner function
        Parser innerFn 

    // infix version of mapP
    let ( <!> ) = mapP

    // "piping" version of mapP
    let ( |>> ) x f = mapP f x

    // --------- Signature of "mapP" --------- 
    // val mapP : 
    //     f:('a -> 'b) -> Parser<'a * 'c> -> Parser<'b * 'c>
    // ------------------------------------------ 

    // ---------  
    // test "mapP"
    // --------- 

    module ParseDigits_2 = 
        let parseDigit = anyOf ['0'..'9']

        let parseThreeDigitsAsStr = 
            // create a parser that returns a tuple
            let tupleParser = 
                parseDigit .>>. parseDigit .>>. parseDigit

            // create a function that turns the tuple into a string
            let transformTuple ((c1, c2), c3) = 
                String [| c1; c2; c3 |]

            // use "mapP" to combine them
            mapP transformTuple tupleParser 
            // val parseThreeDigitsAsStr : Parser<String>

        /// Alternative, more compact, implementation
        let parseThreeDigitsAsStr' = 
            (parseDigit .>>. parseDigit .>>. parseDigit)
            |>> fun ((c1, c2), c3) -> String [| c1; c2; c3 |]

        run parseThreeDigitsAsStr "123A"  // Ok ("123", "A")

        let parseThreeDigitsAsInt = 
            mapP int parseThreeDigitsAsStr 
            // val parseThreeDigitsAsInt : Parser<int>

        run parseThreeDigitsAsInt "123A"  // Ok (123, "A")

// =============================================
// Section 2.2 - Transforming a list of Parsers into a single Parser containing a list
// =============================================

    // ============================
    // Introducing "return" and "apply
    // ============================

    let returnP x = 
        let innerFn input =
            // ignore the input and return x
            Ok (x,input)
        // return the inner function
        Parser innerFn 

    // --------- Signature of "returnP" --------- 
    // val returnP : 
    //     'a -> Parser<'a * string>
    // ------------------------------------------ 

    let applyP fP xP = 
        // create a Parser containing a pair (f,x)
        (fP .>>. xP) 
        // map the pair by applying f to x
        |> mapP (fun (f,x) -> f x)

    // --------- Signature of "applyP" --------- 
    // val applyP : 
    //     Parser<('a -> 'b) * string> -> Parser<'a * 'c> -> Parser<'b * 'c>
    // ------------------------------------------ 

    // infix version of apply
    let ( <*> ) = applyP

    // lift a two parameter function to Parser World
    let lift2 f xP yP =
        returnP f <*> xP <*> yP

    // --------- Signature of "lift2" --------- 
    // val lift2 : 
    //     f:('a -> 'b -> 'c) -> xP:Parser<'a> -> yP:Parser<'b> -> Parser<'c>
    // ------------------------------------------ 

    module Lift2_Test =

        let addP xP yP = 
            lift2 (+) xP yP
        // val addP : (Parser<int> -> Parser<int> -> Parser<int>)

        let startsWith (str:string) (prefix:string) =
            str.StartsWith(prefix)  
        // val startsWith : str:string -> prefix:string -> bool
           
        let startsWithP xP yP =
            lift2 startsWith xP yP
        // val startsWithP : (Parser<string> -> Parser<string> -> Parser<bool>)

// =============================================
// Section 2.3 - `sequence` -- transforming a list of Parsers into a single Parser 
// =============================================

    // ============================
    // Introducing "sequence"
    // ============================

    let rec sequence parserList =
        // define the "cons" function, which is a two parameter function
        let cons head tail = head::tail

        // lift it to Parser World
        let consP = lift2 cons

        // process the list of parsers recursively
        match parserList with
        | [] -> 
            returnP []
        | head::tail ->
            consP head (sequence tail)


    // --------- Signature of "sequence" --------- 
    // val sequence : 
    //     Parser<'a> list -> Parser<'a list>
    // ------------------------------------------ 

    module Sequence_Test =

        let parsers = [ pchar 'A'; pchar 'B'; pchar 'C' ]
        let combined = sequence parsers

        run combined "ABCD" 
        // Ok (['A'; 'B'; 'C'], "D")



    // ============================
    // Introducing "pstring"
    // ============================

    /// Helper to create a string from a list of chars
    let charListToStr charList = 
         String(List.toArray charList)

    // match a specific string
    let pstring str = 
        str
        // convert to list of char
        |> List.ofSeq
        // map each char to a pchar
        |> List.map pchar 
        // convert to Parser<char list>
        |> sequence
        // convert Parser<char list> to Parser<string>
        |> mapP charListToStr 

    // ---------  
    // test "pstring"
    // --------- 

    module Pstring_Test =

        let parseABC = pstring "ABC"

        run parseABC "ABCDE"  // Ok ("ABC", "DE")
        run parseABC "A|CDE"  // Error "Expecting 'B'. Got '|'"
        run parseABC "AB|DE"  // Error "Expecting 'C'. Got '|'"


// =============================================
// Section 2.4 - `many` and `many1` -- greedily matching a parser multiple times
// =============================================

    // ============================
    // Introducing "many"
    // ============================

    /// (helper) match zero or more occurences of the specified parser
    let rec parseZeroOrMore parser input =
        // run parser with the input
        let firstResult = run parser input 
        // test the result for Error/Ok
        match firstResult with
        | Error err -> 
            // if parse fails, return empty list
            ([],input)  
        | Ok (firstValue,inputAfterFirstParse) -> 
            // if parse succeeds, call recursively
            // to get the subsequent values
            let (subsequentValues,remainingInput) = 
                parseZeroOrMore parser inputAfterFirstParse
            let values = firstValue::subsequentValues
            (values,remainingInput)  

    /// match zero or more occurences of the specified parser
    let many parser = 

        let rec innerFn input =
            // parse the input -- wrap in Ok as it always succeeds
            Ok (parseZeroOrMore parser input)

        Parser innerFn

    // --------- Signature of "many" --------- 
    // val many : 
    //     Parser<'a> -> Parser<'a list>
    // ------------------------------------------ 

    // ---------  
    // test "many"
    // --------- 
    module Many_Test = 

        let manyA = many (pchar 'A')

        run manyA "ABCD"  // Ok (['A'], "BCD")
        run manyA "AACD"  // Ok (['A'; 'A'], "CD")
        run manyA "AAAD"  // Ok (['A'; 'A'; 'A'], "D")
        run manyA "|BCD"  // Ok ([], "|BCD")

        let manyAB = many (pstring "AB")

        run manyAB "ABCD"  // Ok (["AB"], "CD")
        run manyAB "ABABCD"  // Ok (["AB"; "AB"], "CD")
        run manyAB "ZCD"  // Ok ([], "ZCD")
        run manyAB "AZCD"  // Ok ([], "AZCD")


        // parse whitespace
        let whitespaceChar = anyOf [' '; '\t'; '\n']
        let whitespace = many whitespaceChar 

        run whitespace "ABC"  // Ok ([], "ABC")
        run whitespace " ABC"  // Ok ([' '], "ABC")
        run whitespace "\tABC"  // Ok (['\t'], "ABC")

    // ============================
    // Introducing "many1"
    // ============================

    /// match one or more occurences of the specified parser
    let many1 parser = 
        let rec innerFn input =
            // run parser with the input
            let firstResult = run parser input 
            // test the result for Error/Ok
            match firstResult with
            | Error err -> 
                Error err // failed
            | Ok (firstValue,inputAfterFirstParse) -> 
                // if first found, look for zeroOrMore now
                let (subsequentValues,remainingInput) = 
                    parseZeroOrMore parser inputAfterFirstParse
                let values = firstValue::subsequentValues
                Ok (values,remainingInput)  
        Parser innerFn

    // --------- Signature of "many1" --------- 
    // val many1 : 
    //     Parser<'a> -> Parser<'a list>
    // ------------------------------------------ 

    // ---------  
    // test "many1"
    // --------- 
    module Many1_Test = 

        // define parser for one digit
        let digit = anyOf ['0'..'9']

        // define parser for one or more digits
        let digits = many1 digit 

        run digits "1ABC"  // Ok (['1'], "ABC")
        run digits "12BC"  // Ok (['1'; '2'], "BC")
        run digits "123C"  // Ok (['1'; '2'; '3'], "C")
        run digits "1234"  // Ok (['1'; '2'; '3'; '4'], "")
        run digits "ABC"   // Error "Expecting '9'. Got 'A'"

    // ============================
    // Introducing "pint"
    // ============================

    // parse an integer
    let pint = 

        // helper
        let resultToInt digitList = 
            // ignore int overflow for now
            String(List.toArray digitList) |> int
            
        // define parser for one digit
        let digit = anyOf ['0'..'9']

        // define parser for one or more digits
        let digits = many1 digit 

        // map the digits to an int
        digits 
        |> mapP resultToInt

    // ---------  
    // test "pint"
    // --------- 

    module Pint_Test = 

        run pint "1ABC"  // Ok (1, "ABC")
        run pint "12BC"  // Ok (12, "BC")
        run pint "123C"  // Ok (123, "C")
        run pint "1234"  // Ok (1234, "")
        run pint "ABC"   // Error "Expecting '9'. Got 'A'"

// =============================================
// Section 2.5 - optional parsing
// =============================================

    /// Parses an optional occurrence of p and returns an option value.
    let opt p = 
        let some = p |>> Some
        let none = returnP None
        some <|> none

    module Opt_Test = 
        let digit = anyOf ['0'..'9']
        let digitThenSemicolon = digit .>>. opt (pchar ';')

        run digitThenSemicolon "1;"  // Ok (('1', Some ';'), "")
        run digitThenSemicolon "1"   // Ok (('1', None), "")
        

    // parse an integer (with minus sign)
    let pint' = 

        // helper
        let resultToInt (sign,digitList) = 
            let i = String(List.toArray digitList) |> int
            match sign with
            | Some ch -> -i  // negate the int
            | None -> i
            
        // define parser for one digit
        let digit = anyOf ['0'..'9']

        // define parser for one or more digits
        let digits = many1 digit 

        // parse and convert
        opt (pchar '-') .>>. digits 
        |>> resultToInt 

    // ---------  
    // test "pint"
    // --------- 

    module Pint'_Test = 

        run pint' "123C"   // Ok (123, "C")
        run pint' "-123C"  // Ok (-123, "C")

// =============================================
// Section 2.6 - throwing results away
//
// throwing away things
// [1;2;3]  throw away the brackets and 
//          the semicolons to get a list of 1 and 2 and 3
//
// =============================================

    // ============================
    // Introducing the "throwing away things" combinators
    // ============================

    let (.>>) p1 p2 = 
        // create a pair
        p1 .>>. p2 
        // then only keep the first value
        |> mapP (fun (a,b) -> a) 

    let (>>.) p1 p2 = 
        // create a pair
        p1 .>>. p2 
        // then only keep the second value
        |> mapP (fun (a,b) -> b) 

    // ---------  
    // test 
    // --------- 
    module Discard_Test = 

        let digit = anyOf ['0'..'9']
        // use .>> below
        let digitThenSemicolon = digit .>> opt (pchar ';')

        run digitThenSemicolon "1;"  // Ok ('1', "")
        run digitThenSemicolon "1"   // Ok ('1', "")


        // whitespace example
        let whitespaceChar = anyOf [' '; '\t'; '\n']
        let whitespace = many1 whitespaceChar 
        
        let ab = pstring "AB"
        let cd = pstring "CD"
        let ab_cd = (ab .>> whitespace) .>>. cd

        run ab_cd "AB \t\nCD"   // Ok (("AB", "CD"), "")

    // ---------  
    // between
    // --------- 

    let between p1 p2 p3 = 
        p1 >>. p2 .>> p3 


    // ---------  
    // test 
    // --------- 
    module Between_Test = 

        let pdoublequote = pchar '"'
        let quotedInteger = between pdoublequote pint pdoublequote

        run quotedInteger "\"1234\""   // Ok (1234, "")
        run quotedInteger "1234"       // Error "Expecting '"'. Got '1'"

        let pspace = anyOf [' '; '\t'; '\n'; '\r']
        let pwhitespace = many pspace
        let ignoreWhitespaceAround p1 = between pwhitespace p1 pwhitespace 

        let parseABC = pstring "ABC"
        run parseABC " ABC "   // fails because of whitespace
                               // Error "Expecting 'A'. Got ' '"

        let parse_ABC_ = ignoreWhitespaceAround parseABC 
        run parse_ABC_ " ABC " //  Ok ("ABC", "")  
        run parse_ABC_ " \tABC\n " //  Ok ("ABC", "")  

// =============================================
// Section 2.7 - parsing lists with separators
// =============================================

    /// Parses one or more occurrences of p separated by sep
    let sepBy1 p sep =
        let sepThenP = sep >>. p            
        p .>>. many sepThenP 
        |>> fun (p,pList) -> p::pList

    /// Parses zero or more occurrences of p separated by sep
    let sepBy p sep =
        sepBy1 p sep <|> returnP []
        
    module Sep_Test =
        let comma = pchar ',' 
        let digit = anyOf ['0'..'9']

        let zeroOrMoreDigitList = sepBy digit comma
        let oneOrMoreDigitList = sepBy1 digit comma

        run oneOrMoreDigitList "1;"      // Ok (['1'], ";")
        run oneOrMoreDigitList "1,2;"    // Ok (['1'; '2'], ";")
        run oneOrMoreDigitList "1,2,3;"  // Ok (['1'; '2'; '3'], ";")
        run oneOrMoreDigitList "Z;"      // Error "Expecting '9'. Got 'Z'"

        run zeroOrMoreDigitList "1;"     // Ok (['1'], ";")
        run zeroOrMoreDigitList "1,2;"   // Ok (['1'; '2'], ";")
        run zeroOrMoreDigitList "1,2,3;" // Ok (['1'; '2'; '3'], ";")
        run zeroOrMoreDigitList "Z;"     // Ok ([], "Z;")

// =============================================
// What about bind?
// =============================================

module ParserLibraryWithBind =
    open System
    open ParserLibrary1

    let returnP x = 
        let innerFn input =
            Ok (x,input)
        Parser innerFn 

    /// "bindP" takes a parser-producing function f, and a parser p
    /// and passes the output of p into f, to create a new parser
    let bindP f p =
        let innerFn input =
            let result1 = run p input 
            match result1 with
            | Error err -> 
                // return error from parser1
                Error err  
            | Ok (value1,remainingInput) ->
                // apply f to get a new parser
                let p2 = f value1
                // run parser with remaining input
                run p2 remainingInput
        Parser innerFn 

    // --------- Signature of "bindP" --------- 
    // val bindP : 
    //     f:('a -> Parser<'b>) -> Parser<'a> -> Parser<'b>
    // ------------------------------------------ 

    /// Infix version of bindP
    let ( >>= ) p f = bindP f p

    let mapP f =         
        bindP (f >> returnP)

    let andThen p1 p2 =         
        p1 >>= (fun p1Result -> 
        p2 >>= (fun p2Result -> 
            returnP (p1Result,p2Result) ))

    let applyP fP xP =         
        fP >>= (fun f -> 
        xP >>= (fun x -> 
            returnP (f x) ))

    // infix version of apply
    let ( <*> ) = applyP

    // lift a two parameter function to Parser World
    let lift2 f xP yP =
        returnP f <*> xP <*> yP

    /// (helper) match zero or more occurences of the specified parser
    let rec parseZeroOrMore parser input =
        // run parser with the input
        let firstResult = run parser input 
        // test the result for Error/Ok
        match firstResult with
        | Error err -> 
            // if parse fails, return empty list
            ([],input)  
        | Ok (firstValue,inputAfterFirstParse) -> 
            // if parse succeeds, call recursively
            // to get the subsequent values
            let (subsequentValues,remainingInput) = 
                parseZeroOrMore parser inputAfterFirstParse
            let values = firstValue::subsequentValues
            (values,remainingInput)  

    let many parser = 
        let rec innerFn input =
            Ok (parseZeroOrMore parser input)
        Parser innerFn

    let many1 p =         
        p      >>= (fun head -> 
        many p >>= (fun tail -> 
            returnP (head::tail) ))

    module Test = 

        // define parser for one digit
        let digit = anyOf ['0'..'9']

        // test "andThen"
        let pair = digit .>>. digit 
        run pair "1234"  // Ok (('1', '2'), "34")

        // test "map"
        let pairStr = 
            digit .>>. digit 
            |> mapP (fun (x,y) -> String [|x;y|] )
        run pairStr "1234"  // Ok ("12", "34")

        // test "many1"
        let digits = many1 digit 
        run digits "1234"  // Ok (['1'; '2'; '3'; '4'], "")
        run digits "ABC"   // Error "Expecting '9'. Got 'A'"
