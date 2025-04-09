#load "ParserLibrary.fsx"

open System
open ParserLibrary

(*
// --------------------------------
JSON spec from http://www.json.org/
// --------------------------------

The JSON spec is available at [json.org](http://www.json.org/). I'll paraphase it here:

* A `value` can be a `string` or a `number` or a `bool` or `null` or an `object` or an `array`. 
  * These structures can be nested.
* A `string` is a sequence of zero or more Unicode characters, wrapped in double quotes, using backslash escapes. 
* A `number` is very much like a C or Java number, except that the octal and hexadecimal formats are not used.
* A `boolean` is the literal `true` or `false`
* A `null` is the literal `null`
* An `object` is an unordered set of name/value pairs. 
  * An object begins with { (left brace) and ends with } (right brace). 
  * Each name is followed by : (colon) and the name/value pairs are separated by , (comma).
* An `array` is an ordered collection of values. 
  * An array begins with [ (left bracket) and ends with ] (right bracket). 
  * Values are separated by , (comma).
* Whitespace can be inserted between any pair of tokens. 

*)


type JValue = 
    | JString of string
    | JNumber of float
    | JBool   of bool
    | JNull
    | JObject of Map<string, JValue>
    | JArray  of JValue list


// ======================================
// Parsing a JNull
// ======================================

let jNull_v1 = 
    pstring "null" 
    |>> (fun _ -> JNull)  // map to JNull
    <?> "null"            // give it a label

// ======================================
// Utility function
// ======================================

// applies the parser p, ignores the result, and returns x.
let (>>%) p x =
    p |>> (fun _ -> x)

// ======================================
// Parsing a JNull again
// ======================================

let jNull = 
    pstring "null" 
    >>% JNull   // using new utility combinator
    <?> "null"  

// --------------
// Test
// --------------
module Null_Test = 

    run jNull "null"   
    // Success: JNull

    run jNull "nulp" |> printResult  
    // Line:0 Col:3 Error parsing null
    // nulp
    //    ^Unexpected 'p'

// ======================================
// Parsing a JBool
// ======================================

let jBool =   
    let jtrue = 
        pstring "true" 
        >>% JBool true   // map to JBool
    let jfalse = 
        pstring "false" 
        >>% JBool false  // map to JBool 

    // choose between true and false
    jtrue <|> jfalse
    <?> "bool"           // give it a label

// --------------
// Test
// --------------
module Bool_Test = 

    run jBool "true"   
    // Success: JBool true

    run jBool "false"
    // Success: JBool false

    run jBool "truX" |> printResult  
    // Line:0 Col:0 Error parsing bool
    // truX
    // ^Unexpected 't'

    // misleading error due to backtracking issue discussed in previous post


// ======================================
// Parsing a JString
// ======================================

/// Parse an unescaped char
let jUnescapedChar = 
    let label = "char"
    satisfy (fun ch -> ch <> '\\' && ch <> '\"') label 

/// Parse an escaped char
let jEscapedChar = 
    [ 
    // (stringToMatch, resultChar)
    ("\\\"",'\"')      // quote
    ("\\\\",'\\')      // reverse solidus 
    ("\\/",'/')        // solidus
    ("\\b",'\b')       // backspace
    ("\\f",'\f')       // formfeed
    ("\\n",'\n')       // newline
    ("\\r",'\r')       // cr
    ("\\t",'\t')       // tab
    ] 
    // convert each pair into a parser
    |> List.map (fun (toMatch,result) -> 
        pstring toMatch >>% result)
    // and combine them into one
    |> choice
    <?> "escaped char" // set label

/// Parse a unicode char
let jUnicodeChar = 
    
    // set up the "primitive" parsers        
    let backslash = pchar '\\'
    let uChar = pchar 'u'
    let hexdigit = anyOf (['0'..'9'] @ ['A'..'F'] @ ['a'..'f'])

    // convert the parser output (nested tuples)
    // to a char
    let convertToChar (((h1,h2),h3),h4) = 
        let str = sprintf "%c%c%c%c" h1 h2 h3 h4
        Int32.Parse(str,Globalization.NumberStyles.HexNumber) |> char

    // set up the main parser
    backslash  >>. uChar >>. hexdigit .>>. hexdigit .>>. hexdigit .>>. hexdigit
    |>> convertToChar 


/// Parse a quoted string
let quotedString = 
    let quote = pchar '\"' <?> "quote"
    let jchar = jUnescapedChar <|> jEscapedChar <|> jUnicodeChar 

    // set up the main parser
    quote >>. manyChars jchar .>> quote 

/// Parse a JString
let jString = 
    // wrap the string in a JString
    quotedString
    |>> JString           // convert to JString
    <?> "quoted string"   // add label

// --------------
// Test
// --------------
module String_Test = 

    // test jUnescapedChar 
    run jUnescapedChar "a"   // Success 'a'

    run jUnescapedChar "\\" |> printResult
    // Line:0 Col:0 Error parsing char
    // \
    // ^Unexpected '\'

    // test jEscapedChar
    run jEscapedChar "\\\\" // Success '\'
    run jEscapedChar "\\t"  // Success '\009'

    run jEscapedChar "a" |> printResult
    // Line:0 Col:0 Error parsing escaped char
    // a
    // ^Unexpected 'a'

    // test \u263A - smiley
    run jUnicodeChar "\\u263A"  // Success '☺'

    // test 
    run jString "\"\""    // Success ""
    run jString "\"a\""   // Success "a"
    run jString "\"ab\""  // Success "ab"
    run jString "\"ab\\tde\""      // Success "ab\tde"
    run jString "\"ab\\u263Ade\""  // Success "ab☺de"


// ======================================
// Parsing a JNumber
// ======================================

/// Parse a JNumber
let jNumber = 

    // set up the "primitive" parsers        
    let optSign = opt (pchar '-')

    let zero = pstring "0"

    let digitOneNine = 
        satisfy (fun ch -> Char.IsDigit ch && ch <> '0') "1-9"

    let digit = 
        satisfy (fun ch -> Char.IsDigit ch ) "digit"

    let point = pchar '.'

    let e = pchar 'e' <|> pchar 'E'

    let optPlusMinus = opt (pchar '-' <|> pchar '+')

    let nonZeroInt = 
        digitOneNine .>>. manyChars digit 
        |>> fun (first,rest) -> string first + rest

    let intPart = zero <|> nonZeroInt

    let fractionPart = point >>. manyChars1 digit

    let exponentPart = e >>. optPlusMinus .>>. manyChars1 digit

    // utility function to convert an optional value to a string, or "" if missing
    let ( |>? ) opt f = 
        match opt with
        | None -> ""
        | Some x -> f x

    let convertToJNumber (((optSign,intPart),fractionPart),expPart) = 
        // convert to strings and let .NET parse them! - crude but ok for now.

        let signStr = 
            optSign 
            |>? string   // e.g. "-"

        let fractionPartStr = 
            fractionPart 
            |>? (fun digits -> "." + digits )  // e.g. ".456"

        let expPartStr = 
            expPart 
            |>? fun (optSign, digits) ->
                let sign = optSign |>? string
                "e" + sign + digits          // e.g. "e-12"

        // add the parts together and convert to a float, then wrap in a JNumber
        (signStr + intPart + fractionPartStr + expPartStr)
        |> float
        |> JNumber

    // set up the main parser
    optSign .>>. intPart .>>. opt fractionPart .>>. opt exponentPart
    |>> convertToJNumber
    <?> "number"   // add label

// --------------
// Test
// --------------
module Number_Test = 

    // test
    run jNumber "123"     // JNumber 123.0
    run jNumber "-123"    // JNumber -123.0
    run jNumber "123.4"   // JNumber 123.4

    // test failures
    run jNumber "-123."   // JNumber -123.0 -- should fail!
    run jNumber "00.1"    // JNumber 0      -- should fail!

    let jNumber_ = jNumber .>> spaces1

    // test int part only
    run jNumber_ "123"     // JNumber 123.0
    run jNumber_ "-123"    // JNumber -123.0
    run jNumber_ "-123." |> printResult
    // Line:0 Col:4 Error parsing number andThen many1 whitespace
    // -123.
    //     ^Unexpected '.'
  
    // test fraction only
    run jNumber_ "123.4"   // JNumber 123.4
    run jNumber_ "00.4" |> printResult
    // Line:0 Col:1 Error parsing number andThen many1 whitespace
    // 00.4
    //  ^Unexpected '0'

    // test exponent only
    run jNumber_ "123e4"     // JNumber 1230000.0

    // test fraction and exponent 
    run jNumber_ "123.4e5"   // JNumber 12340000.0
    run jNumber_ "123.4e-5"  // JNumber 0.001234


    // test error
    run jNumber "A" |> printResult
    // Line:0 Col:0 Error parsing number
    // A
    // ^Unexpected 'A'

// ======================================
// Parsing a JArray
// ======================================

(*
let jArray = 

    // set up the "primitive" parsers        
    let left = pchar '[' .>> spaces
    let right = pchar ']' .>> spaces
    let comma = pchar ',' .>> spaces
    let value = jValue .>> spaces   // compiler error

    // set up the list parser
    let values = sepBy1 value comma

    // set up the main parser
    between left values right 
    |>> JArray
    <?> "array"
*)

/// Create a forward reference
let createParserForwardedToRef<'a>() =

    let dummyParser= 
        let innerFn input : Result<'a * Input> = failwith "unfixed forwarded parser"
        {parseFn=innerFn; label="unknown"}
    
    // ref to placeholder Parser
    let parserRef = ref dummyParser 

    // wrapper Parser
    let innerFn input = 
        // forward input to the placeholder
        runOnInput !parserRef input 
    let wrapperParser = {parseFn=innerFn; label="unknown"}

    wrapperParser, parserRef

let jValue,jValueRef = createParserForwardedToRef<JValue>()

let jArray = 

    // set up the "primitive" parsers        
    let left = pchar '[' .>> spaces
    let right = pchar ']' .>> spaces
    let comma = pchar ',' .>> spaces
    let value = jValue .>> spaces   

    // set up the list parser
    let values = sepBy1 value comma

    // set up the main parser
    between left values right 
    |>> JArray
    <?> "array"

// --------------
// Test
// --------------
module Array_Test = 
    
    // run jArray "[ 1, 2 ]"
    // System.Exception: unfixed forwarded parser

    jValueRef := jNumber  
    // warning: only int values supported for parsing!

    run jArray "[ 1, 2 ]"
    // Success (JArray [JNumber 1.0; JNumber 2.0],

    run jArray "[ 1, 2, ]" |> printResult
    // Line:0 Col:6 Error parsing array
    // [ 1, 2, ]
    //       ^Unexpected ','

// ======================================
// Parsing a JObject
// ======================================

let jObject = 

    // set up the "primitive" parsers        
    let left = pchar '{' .>> spaces
    let right = pchar '}' .>> spaces
    let colon = pchar ':' .>> spaces
    let comma = pchar ',' .>> spaces
    let key = quotedString .>> spaces 
    let value = jValue .>> spaces

    // set up the list parser
    let keyValue = (key .>> colon) .>>. value
    let keyValues = sepBy1 keyValue comma

    // set up the main parser
    between left keyValues right 
    |>> Map.ofList  // convert the list of keyValues into a Map
    |>> JObject     // wrap in JObject     
    <?> "object"    // add label


// --------------
// Test
// --------------
module Object_Test = 

    // warning: only int JValues supported still!

    run jObject """{ "a":1, "b"  :  2 }"""
    // JObject (map [("a", JNumber 1.0); ("b", JNumber 2.0)]),

    run jObject """{ "a":1, "b"  :  2, }""" |> printResult
    // Line:0 Col:18 Error parsing object
    // { "a":1, "b"  :  2, }
    //                   ^Unexpected ','

// ======================================
// Finishing up
// ======================================


// fixup the forward ref
jValueRef := choice 
    [
    jNull 
    jBool
    jNumber
    jString
    jArray
    jObject
    ]


// ======================================
// Test: Example 1
// ======================================

let example1 = """{
    "name" : "Hans",
    "isMale" : true,
    "bday" : {"year":2001, "month":12, "day":25 },
    "favouriteColors" : ["blue", "green"]
}"""
run jValue example1

(*
JObject
    (map
        [("bday", JObject(map
                [("day", JNumber 25.0); 
                ("month", JNumber 12.0);
                ("year", JNumber 2001.0)]));
        ("favouriteColors", JArray [JString "blue"; JString "green"]);
        ("isMale", JBool true); 
        ("name", JString "Hans")
        ])
*)

// ======================================
// Test: Example 2
// ======================================

let example2= """{"widget": {
    "debug": "on",
    "window": {
        "title": "Sample Konfabulator Widget",
        "name": "main_window",
        "width": 500,
        "height": 500
    },
    "image": { 
        "src": "Images/Sun.png",
        "name": "sun1",
        "hOffset": 250,
        "vOffset": 250,
        "alignment": "center"
    },
    "text": {
        "data": "Click Here",
        "size": 36,
        "style": "bold",
        "name": "text1",
        "hOffset": 250,
        "vOffset": 100,
        "alignment": "center",
        "onMouseUp": "sun1.opacity = (sun1.opacity / 100) * 90;"
    }
}}  """

run jValue example2

(*
JObject(map
    [("widget",JObject(map
            [("debug", JString "on");
            ("image",JObject(map
                [("alignment", JString "center");
                    ("hOffset", JNumber 250.0); ("name", JString "sun1");
                    ("src", JString "Images/Sun.png");
                    ("vOffset", JNumber 250.0)]));
            ("text",JObject(map
                [("alignment", JString "center");
                    ("data", JString "Click Here");
                    ("hOffset", JNumber 250.0); 
                    ("name", JString "text1");
                    ("onMouseUp", JString "sun1.opacity = (sun1.opacity / 100) * 90;");
                    ("size", JNumber 36.0); 
                    ("style", JString "bold");
                    ("vOffset", JNumber 100.0)]));
            ("window",JObject(map
                [("height", JNumber 500.0);
                    ("name", JString "main_window");
                    ("title", JString "Sample Konfabulator Widget");
                    ("width", JNumber 500.0)]))]))]),

*)

// ======================================
// Test: Example 3
// ======================================

let example3 = """{"menu": {
  "id": "file",
  "value": "File",
  "popup": {
    "menuitem": [
      {"value": "New", "onclick": "CreateNewDoc()"},
      {"value": "Open", "onclick": "OpenDoc()"},
      {"value": "Close", "onclick": "CloseDoc()"}
    ]
  }
}}"""

run jValue example3

(*
JObject(map
    [("menu",JObject(map
            [("id", JString "file");
            ("popup",JObject(map
                [("menuitem",
                    JArray
                    [JObject(map
                            [("onclick", JString "CreateNewDoc()");
                            ("value", JString "New")]);
                        JObject(map
                            [("onclick", JString "OpenDoc()");
                            ("value", JString "Open")]);
                        JObject(map
                            [("onclick", JString "CloseDoc()");
                            ("value", JString "Close")])])]));
            ("value", JString "File")]))])
*)