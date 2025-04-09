
// Copyright (c) Stephan Tolksdorf 2008-2011
// License: Simplified BSD License. See accompanying documentation.

module Parser
#r "./lib/FParsecCS.dll"
#r "./lib/FParsec.dll"
open FParsec

#load "json-ast.fsx"
open Ast

// This is a general JSON parser that will parse any JSON file into an AST.
// See e.g. http://www.json.org/, for a specification of JSON.

// The FParsec tutorial discusses this parser in detail.

// Note that in typical applications you often don't need to parse any general
// JSON file, but only files describing objects of a certain type. In those cases
// it might be more convenient to parse the input with specialized parsers
// instead of using the indirect approach via an intermediate AST. The parser
// definitions below should be useful in any case.

let jnull: Parser<Json,unit>  = stringReturn "null" JNull
let jtrue: Parser<Json,unit>  = stringReturn "true"  (JBool true)
let jfalse: Parser<Json,unit> = stringReturn "false" (JBool false)

let jnumber: Parser<Json,unit> = pfloat |>> JNumber // pfloat will accept a little more than specified by JSON
                                 // as valid numbers (such as NaN or Infinity), but that makes
                                 // it only more robust

let str s = pstring s

let stringLiteral: Parser<string,unit> =
    let escape =  anyOf "\"\\/bfnrt"
                  |>> function
                      | 'b' -> "\b"
                      | 'f' -> "\u000C"
                      | 'n' -> "\n"
                      | 'r' -> "\r"
                      | 't' -> "\t"
                      | c   -> string c // every other char is mapped to itself

    let unicodeEscape =
        /// converts a hex char ([0-9a-fA-F]) to its integer number (0-15)
        let hex2int c = (int c &&& 15) + (int c >>> 6)*9

        str "u" >>. pipe4 hex hex hex hex (fun h3 h2 h1 h0 ->
            (hex2int h3)*4096 + (hex2int h2)*256 + (hex2int h1)*16 + hex2int h0
            |> char |> string
        )

    let escapedCharSnippet = str "\\" >>. (escape <|> unicodeEscape)
    let normalCharSnippet  = manySatisfy (fun c -> c <> '"' && c <> '\\')

    between (str "\"") (str "\"")
            (stringsSepBy normalCharSnippet escapedCharSnippet)

let jstring = stringLiteral |>> JString

// jvalue, jlist and jobject are three mutually recursive grammar productions.
// In order to break the cyclic dependency, we make jvalue a parser that
// forwards all calls to a parser in a reference cell.
let (jvalue: Parser<Json,unit>), (jvalueRef: Parser<Json,unit> ref) = createParserForwardedToRef() // initially jvalueRef holds a reference to a dummy parser

let ws = spaces // skips any whitespace

let listBetweenStrings sOpen sClose pElement f =
    between (str sOpen) (str sClose)
            (ws >>. sepBy (pElement .>> ws) (str "," .>> ws) |>> f)

let keyValue = tuple2 stringLiteral (ws >>. str ":" >>. ws >>. jvalue)

let jlist   = listBetweenStrings "[" "]" jvalue JList
let jobject = listBetweenStrings "{" "}" keyValue (Map.ofList >> JObject)

do jvalueRef := choice [jobject
                        jlist
                        jstring
                        jnumber
                        jtrue
                        jfalse
                        jnull]

let json = ws >>. jvalue .>> ws .>> eof

let parseJsonString str = run json str

// UTF8 is the default, but it will detect UTF16 or UTF32 byte-order marks automatically
let parseJsonFile fileName encoding =
    runParserOnFile json () fileName encoding

let parseJsonStream stream encoding =
    runParserOnStream json () "" stream System.Text.Encoding.UTF8


open FParsec.CharParsers
let file = System.IO.Path.Combine(__SOURCE_DIRECTORY__,"json-test.txt")

let test = parseJsonFile file System.Text.Encoding.UTF8
// for the moment we just print out the AST
match test with
| Success (v, _, _) ->
    printf "The AST of the input file is:\n%A\n" v
| Failure (msg, err, _) ->
    printfn "%s" msg

//fsi.PrintLength <- 2000
sprintf "%A" test
