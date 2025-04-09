open System
let A_Parser str =
    if String.IsNullOrEmpty(str) then
        (false,"")
    else if str.[0] = 'A' then
        let remaining = str.[1..]
        (true,remaining)
    else
        (false,str)
let inputABC = "ABC";;
let inputZBC = "ZBC";;
let test11 = A_Parser inputABC
let test12 = A_Parser inputZBC
test12

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

let inputABC = "ABC";;
let inputZBC = "ZBC";;
let test21 = pchar('A',inputABC) 
let test22 = pchar('A',inputZBC)
test21, test22

let pchar (charToMatch, s) =
    if String.IsNullOrEmpty(s) then
        Error "No more input"
    else
        let first = s.[0] 
        if first = charToMatch then
            let remaining = s.[1..]
            Ok (charToMatch, remaining)
        else
            let msg = sprintf "Expecting '%c'. Got '%c'" charToMatch first
            Error msg

let test31 = pchar('A',inputABC) 
let test32 = pchar('A',inputZBC) 
let test33 = pchar('Z',inputZBC)
[test31; test32; test33]

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

let parseA = pchar 'A'
let inputABC = "ABC"
let inputZBC = "ZBC"
let test41 = parseA inputABC
let test42 = parseA inputZBC
let parseZ = pchar 'Z' 
let test43 = parseZ inputZBC
[test41; test42; test43]

type Parser<'T> =
    | Parser of (string -> Result<'T , string>)
let pchar charToMatch = 
    let innerFn str =
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
    Parser innerFn

let parseA = pchar 'A'
let inputABC = "ABC"
parseA inputABC

let run parser input = 
    let (Parser innerFn) = parser 
    innerFn input
let parseA = pchar 'A' 
let inputABC = "ABC"
let test1 = run parseA inputABC
let inputZBC = "ZBC"
let test2 = run parseA inputZBC
[test1; test2]

#r "../../src/5/02-fparsec/lib/FParsecCS.dll";; 
#r "../../src/5/02-fparsec/lib/FParsec.dll";;
open FParsec
let test p str =
    match run p str with
    | Success(result, _, _)   -> printfn "Success: %A" result
    | Failure(errorMsg, _, _) -> printfn "Failure: %s" errorMsg;;
test pfloat "1.25"
test pfloat "1.25E 2"

let str s = pstring s
let floatBetweenBrackets:Parser<float, unit> = str "[" >>. pfloat .>> str "]";;

test floatBetweenBrackets "[1.0]"
test floatBetweenBrackets "[]"
test floatBetweenBrackets "[1.0]"

let betweenStrings s1 s2 p = str s1 >>. p .>> str s2;;
let floatBetweenBrackets_:Parser<float, unit>  = pfloat |> betweenStrings "[" "]";;
let floatBetweenDoubleBrackets_:Parser<float, unit>  = pfloat |> betweenStrings "[[" "]]";;
test floatBetweenBrackets_ "[1.0]"
test floatBetweenDoubleBrackets_ "[[1.0]]"
let between_ pBegin pEnd p  = pBegin >>. p .>> pEnd;;
let betweenStrings_ s1 s2 p = p |> between_ (str s1) (str s2);;
test (many floatBetweenBrackets) ""
test (many floatBetweenBrackets) "[1.0]"
test (many floatBetweenBrackets) "[2][3][4]"
test (many floatBetweenBrackets) "[1][2.0E]"
