open System
let create (row, col) = row >= 0 && row < 8 && col >= 0 && col < 8
let canAttack (queen1: int * int) (queen2: int * int) = 
    let (r1, c1) = queen1
    let (r2, c2) = queen2
    Math.Abs(r1 - r2) = Math.Abs(c1 - c2) || r1 = r2 || c1 = c2
let whiteQueen1, blackQueen1 = (2, 2), (1, 1)
let test1 = canAttack blackQueen1 whiteQueen1
let whiteQueen2, blackQueen2 = (2, 4), (6, 6)
let test2 = canAttack blackQueen2 whiteQueen2

let rules =
    [ 3, "Pling"
      5, "Plang"
      7, "Plong" ]
let convert (number: int): string =
    let divBy n d = n % d = 0
    rules
    |> List.filter (fst >> divBy number)
    |> List.map snd
    |> String.concat ""
    |> function
       | "" -> string number
       | s -> s
let test = convert 105

let add (beginDate : System.DateTime) = beginDate.AddSeconds 1e9
let test = add (DateTime(2015, 1, 24, 22, 0, 0)) = (DateTime(2046, 10, 2, 23, 46, 40))

type OpenAccount =
    { mutable Balance: decimal }
type Account =
    | Closed
    | Open of OpenAccount
let mkBankAccount() = Closed
let openAccount account =
    match account with
    | Closed -> Open { Balance = 0.0m }
    | Open _ -> failwith "Account is already open"

let closeAccount account =
    match account with
    | Open _ -> Closed
    | Closed -> failwith "Account is already closed"
let getBalance account =
    match account with
    | Open openAccount -> Some openAccount.Balance
    | Closed -> None
let updateBalance change account =
    match account with
    | Open openAccount ->
        lock (openAccount) (fun _ ->
            openAccount.Balance <- openAccount.Balance + change
            Open openAccount)
    | Closed -> failwith "Account is closed"

let account = mkBankAccount() |> openAccount
let updateAccountAsync =        
    async { account |> updateBalance 1.0m |> ignore }
let ``updated from multiple threads`` =
    updateAccountAsync
        |> List.replicate 1000
        |> Async.Parallel 
        |> Async.RunSynchronously
        |> ignore
let test1 = getBalance account = (Some 1000.0m)

type Input = {Name : string; Email : string }
let checkNameNotBlank input =
  if input.Name = "" then
     Error "Name must not be blank"
  else Ok input
let checkName50 input =
  if input.Name.Length > 50 then
     Error "Name must not be longer than 50 chars"
  else Ok input
let checkEmailNotBlank input =
  if input.Email = "" then
     Error "Email must not be blank"
  else Ok input

let validateInput input =
    input
    |> checkNameNotBlank
    |> Result.bind checkName50
    |> Result.bind checkEmailNotBlank

let goodInput = {Name="Max"; Email="x@example.com"}
let blankName = {Name=""; Email="x@example.com"}
let blankEmail = {Name="Nora"; Email=""}
[validateInput goodInput; validateInput blankName; validateInput blankEmail]

type ErrorMessage =
  | ??   // name not blank
  | ?? of int  // name not longer than
  | ??   // email not longer than
let translateError_EN err =
  match err with
  | ?? -> "Name must not be blank"
  | ?? i -> sprintf "Name must not be longer than %i chars" i
  | ?? -> "Email must not be blank"
  | SmtpServerError msg -> sprintf "SmtpServerError [%s]" msg

type ErrorMessage =
    | NameMustNotBeBlank
    | NameMustNotBeLongerThan of int
    | EmailMustNotBeBlank
    | SmtpServerError of string
let translateError_FR err =
    match err with
    | NameMustNotBeBlank -> "Nom ne doit pas être vide"
    | NameMustNotBeLongerThan i -> sprintf "Nom ne doit pas être plus long que %i caractères" i
    | EmailMustNotBeBlank -> "Email doit pas être vide"
    | SmtpServerError msg -> sprintf "SmtpServerError [%s]" msg

let thinkOfANumber numberYouThoughtOf =
    let addOne x = x + 1
    let squareIt x = ??
    let subtractOne x = ??
    let divideByTheNumberYouFirstThoughtOf x = ??
    let subtractTheNumberYouFirstThoughtOf x = ??

    // define these functions
    // then combine them using piping

    numberYouThoughtOf
    |> ??
    |> ??
    |> ??

let thinkOfANumber numberYouThoughtOf =
    let addOne x = x + 1
    let squareIt x = x * x
    let subtractOne x = x - 1
    let divideByTheNumberYouFirstThoughtOf x = x / numberYouThoughtOf
    let subtractTheNumberYouFirstThoughtOf x = x - numberYouThoughtOf
    numberYouThoughtOf
    |> addOne
    |> squareIt
    |> subtractOne
    |> divideByTheNumberYouFirstThoughtOf
    |> subtractTheNumberYouFirstThoughtOf
thinkOfANumber 42

let buildTree records =
    let records' = List.sortBy (fun x -> x.RecordId) records
    if List.isEmpty records' then failwith "Empty input"
    else
        let root = records'.[0]
        if (root.ParentId = 0 |> not) then
            failwith "Root node is invalid"
        else
            if (root.RecordId = 0 |> not) then failwith "Root node is invalid"
            else
                let mutable prev = -1
                let mutable leafs = []
                for r in records' do
                    if (r.RecordId <> 0 && (r.ParentId > r.RecordId || r.ParentId = r.RecordId)) then
                        failwith "Nodes with invalid parents"
                    else
                        if r.RecordId <> prev + 1 then
                            failwith "Non-continuous list"
                        else
                            prev <- r.RecordId
                            if (r.RecordId = 0) then
                                leafs <- leafs @ [(-1, r.RecordId)]
                            else
                                leafs <- leafs @ [(r.ParentId, r.RecordId)]

let buildTree records = 
    records
    |> List.sortBy (fun r -> r.RecordId)
    |> validate
    |> List.tail
    |> List.groupBy (fun r -> r.ParentId)
    |> Map.ofList
    |> makeTree 0

let rec makeTree id map =
    match map |> Map.tryFind id with
    | None -> Leaf id
    | Some list -> Branch (id, 
        list |> List.map (fun r -> makeTree r.RecordId map))

let validate records =
    match records with
    | [] -> failwith "Input must be non-empty"
    | x :: _ when x.RecordId <> 0 -> 
        failwith "Root must have id 0"
    | x :: _ when x.ParentId <> 0 -> 
        failwith "Root node must have parent id 0"
    | _ :: xs when xs |> List.exists (fun r -> r.RecordId < r.ParentId) -> 
        failwith "ParentId should be less than RecordId"
    | _ :: xs when xs |> List.exists (fun r -> r.RecordId = r.ParentId) -> 
        failwith "ParentId cannot be the RecordId except for the root node."
    | rs when (rs |> List.map (fun r -> r.RecordId) |> List.max) > (List.length rs - 1) -> 
        failwith "Ids must be continuous"
    | _ -> records
