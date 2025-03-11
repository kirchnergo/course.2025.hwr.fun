open System // Gets access to functionality in System namespace.

// Defines a function that takes a name and produces a greeting.
let getGreeting name =
    sprintf "Hello, %s! Isn't F# great?" name

let main args =
    // Defines a list of names
    let names = [ "Don"; "Julia"; "Xi" ]
    // Prints a greeting for each name!
    names
    |> List.map getGreeting
    |> List.iter (fun greeting -> printfn "%s" greeting)
    0

module HelloWorld =
  let hello: string = "Hello, World!"

let s = "hello"  // string
let i = 42       // int
let f = 3.141    // float
let b = true     // bool
let l = [1;2;3]  // list
printfn "%s, %i, %f, %g, %b, %A" s i f f b l

let t1 = (1, 2)
let t2 = ("one", "two", "three")
let t3 = (10, 10.0, "ten")
printfn "%A, %A %A" t1 t2 t3

type Suit = 
    | Hearts 
    | Clubs 
    | Diamonds 
    | Spades
type Rank = 
    | Value of int
    | Ace
    | King
    | Queen
    | Jack
    static member GetAllRanks() = 
        [ yield Ace
          for i in 2 .. 10 do yield Value i
          yield Jack; yield Queen; yield King ]

type Card = { Suit: Suit; Rank: Rank }

/// This computes a list representing all the cards in the deck.
let fullDeck = 
    [ for suit in [Hearts; Diamonds; Clubs; Spades] do
              for rank in Rank.GetAllRanks() do 
                  yield {Suit=suit; Rank=rank} ];;
fullDeck |> Seq.length

let list1 = [ 1; 2; 3 ]
let list2 = [ for i in 1 .. 8 -> i*i ]
let list3 = []
let list4 = 100 :: list2
let list5 = list1 @ list2
let list6 = [1 .. 10]
let array1 = [| 1; 2; 3 |]
let seq1 = seq {1 .. 3}
printfn "%A, %A, %A" list1 array1 seq1

/// A random-number generator 
let rand = System.Random() ;;
/// An infinite sequence of numbers
let randomNumbers = seq { while true do yield rand.Next(100000) };;
/// The first 10 random numbers, sorted
let firstTenRandomNumbers = 
    randomNumbers
    |> Seq.take 10 
    |> Seq.sort
    |> Seq.toList;;
firstTenRandomNumbers

let primeCubes = List.map (fun n -> n * n * n) [2;3;5;7;11;13;17;19]
primeCubes

/// Get the contents of the URL via a web request
let getAsync (url:string) = 
    async {
        let httpClient = new System.Net.Http.HttpClient()
        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        return! response.Content.ReadAsStringAsync() |> Async.AwaitTask
    } |> Async.RunSynchronously

let sites = ["http://www.bing.com"; "http://www.google.com"]
let fetch url = (url, getAsync url)
let ps = List.map fetch sites
let ls = List.map (fun (_,p) -> String.length p) ps
printfn "%A" ls

let fold1 = List.fold (fun acc x -> acc + x) 0 [1..10]
let fold2 = [1..100] |> List.fold (+) 0
let fold3 = (0, [1..1000]) ||> List.fold (+)
printfn "%i, %i, %i" fold1 fold2 fold3

let matchInt i =
    match i with
    | 1 -> printfn "One"
    | 2 -> printfn "Two"
    | _ -> printfn "Other"  // "_" is a wildcard

matchInt 1
matchInt 2
matchInt 77

let caseSwitch input =
    match input with
    | 1 -> printfn "One"
    | 2 -> printfn "A couple"
    | x when x < 12 -> printfn "Less than a dozen" 
    | x when x = 12 -> printfn "A dozen"
    | _ -> printfn "More than a dozen"

caseSwitch 2
caseSwitch 5
caseSwitch 12
caseSwitch 18

let extremes (s : seq<_>) = 
    s |> Seq.min,
    s |> Seq.max

let l, h = [1; 2; 9; 3; -1] |> extremes
(l,h)

open System
let tryParseInt (s:string) =
    match System.Int32.TryParse(s) with 
    | true, i -> Some i
    | false, _ -> None

let a = "30" |> tryParseInt // Some 30
let b = "3X" |> tryParseInt // None
(a,b)

type Track = { Title : string; Artist : string } ;;
let songs = [ { Title = "Summertime"; Artist = "Ray Barretto" };
      { Title = "La clave, maraca y guiro";
        Artist = "Chico Alvarez" };
      { Title = "Summertime";
        Artist = "DJ Jazzy Jeff & The Fresh Prince" } ] ;;
let dist = 
    songs 
    |> Seq.map (fun s -> match s with | {Title = title} -> title) 
    |> Seq.distinct |> Seq.toList
dist

let caseList l = 
    match l with
    | [] -> printfn "An empty pond" 
    | [fish] -> printfn "A pond with one fish only: %s" fish 
    | head::tail -> printfn "A pond with one fish: \
         %s (and %i more fish)" head (tail |> List.length)

caseList []
caseList ["One fish"]
caseList ["One fish"; "Two fish"; "Red fish" ]
caseList ["One fish"; "Two fish"; "Red fish"; "Blue fish" ]

let (|Even|Odd|) input = if input % 2 = 0 then Even else Odd

let TestNumber input =
   match input with
   | Even -> printfn "%d is even" input
   | Odd -> printfn "%d is odd" input

TestNumber 7
TestNumber 8
TestNumber 9

let squareIt1 n = n * n
let squareIt2 = fun n -> n * n
let r1 = squareIt1 8
let r2 = squareIt2 9

let listOfFunctions = [squareIt1; squareIt2]
for fn in listOfFunctions do
    let result = fn 100
    printfn "If 100 is the input, the output is %i" result

/// Computes the greatest common factor of two integers.
///
/// Since all of the recursive calls are tail calls,
/// the compiler will turn the function into a loop,
/// which improves performance and reduces memory consumption.
let rec gcf a b =
    match a with
    | 0 -> b
    | a when a < b -> gcf a (b - a)
    | _ -> gcf (a - b) b

printfn "The Greatest Common Factor of 300 \
         and 620 is %d" (gcf 300 620)

let add1 = (+) 1        
let r1 = add1 2   // result => 3
let multiplyBy2 = (*) 2
let r2 = multiplyBy2 3   // result => 6
let equals3 = (=) 3
let r3 = equals3 3   // result => true

printfn "%i, %i, %b" r1 r2 r3

let negate x = -1 * x
let square x = x*x
let print x = printfn "The number is: %d" x

let snp x = print (negate (square x))
let ``sqr, neg, and print`` x = x |> square |> negate |> print
let snp' = square >> negate >> print

snp 9, ``sqr, neg, and print`` 10, snp' 11
