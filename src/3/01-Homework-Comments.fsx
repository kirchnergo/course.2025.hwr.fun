// -   [ ] Queen Attack
// -   [ ] Raindrops
// -   [ ] Gigasecond
// -   [ ] Bank Account


// Allgemein

// - Publish on exercism.io
// - Formatierung (dotnet fantomas)
// - Vermeide "mutable"!!
// - nur wichtiges verdient einen Namen
// - Vertraue der "Pipe"!!

// - If-Then-Else mit Boolean ist unnötig
// - If-Then-Else vermeiden ... besser match
// - Be lazy! ("for" vermeiden)
// - Parametrisiere!


// Queen Attack

module QueenAttack =

    open System

    let create (row, col) = row >= 0 && row < 8 && col >= 0 && col < 8

    let canAttack (queen1: int * int) (queen2: int * int) =
        let (r1, c1) = queen1
        let (r2, c2) = queen2
        Math.Abs(r1 - r2) = Math.Abs(c1 - c2) || r1 = r2 || c1 = c2

module QueenAttack2 =
    let create (x: int, y: int) = 
        if x < 0 || y < 0 then
            false
        else
            x < 8 && y < 8  

    let canAttack (queen1: int * int) (queen2: int * int) = 
        if fst queen1 = fst queen2 then
            true
        elif snd queen1 = snd queen2 then
            true 
        else
            abs (fst queen1 - fst queen2) = abs (snd queen1 - snd queen2)

    // besser
    let canAttack2 (queen1: int * int) (queen2: int * int) = 
        (fst queen1 = fst queen2) || 
        (snd queen1 = snd queen2) || 
        abs (fst queen1 - fst queen2) = abs (snd queen1 - snd queen2)

module QueenAttack2 =
    let xAttack (position1: int * int) (position2: int * int): bool =
        let (x1, _) = position1
        let (x2, _) = position2
        x1 = x2

    let yAttack (position1: int * int) (position2: int * int): bool =
        let (_, y1) = position1
        let (_, y2) = position2
        y1 = y2

    let rec diagonalAttack (position1: int * int) (position2: int * int): bool =
        let (x1, y1) = position1
        let (x2, y2) = position2
        abs (x1 - x2) = abs (y1 - y2)

    let canAttack (queen1: int * int) (queen2: int * int): bool =
        xAttack queen1 queen2 || yAttack queen1 queen2 || diagonalAttack queen1 queen2

    // Idee: Parametrisiere!
    let funAttack f (position1: int * int) (position2: int * int): bool =
        let a = f position1
        let b = f position2
        a = b
    let xAttack2 = funAttack fst
    let yAttack2 = funAttack snd


// Raindrops

module Raindrops =

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

module Raindrops2 = 
    let convert (number: int): string = 
        let div3(x: int): string =
            if x % 3 = 0 then
                "Pling"
            else 
                ""
        let div5(x: int): string =
            if x % 5 = 0 then
                "Plang"
            else 
                ""
        let div7(x: int): string =
            if x % 7 = 0 then
                "Plong"
            else 
                ""

        let res = (div3 number) + (div5 number) + (div7 number)
        let numStr x = string x

        if res = "" then
            numStr number
        else    
            res

    // Idee: Parametrisiere!
    let convert (number: int): string = 
        let divX d s (x: int): string =  if x % d = 0 then s else ""
        let div3 = divX 3 "Pling"     
        let div5 = divX 5 "Plang"
        let div7 = divX 7 "Plong"
       
        let res = (div3 number) + (div5 number) + (div7 number)
        let numStr x = string x

        if res = "" then
            numStr number
        else    
            res


// Gigasecond

module Gigasecond =
    let add (beginDate: System.DateTime) = beginDate.AddSeconds 1e9

// Bank Account

module BankAccount =

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

    // for lock see: https://technet.microsoft.com/en-ca/ee370413(v=vs.90) 
    let updateBalance0 change account =
        match account with
        | Open openAccount ->
            openAccount.Balance <- openAccount.Balance + change
            Open openAccount
        | Closed -> failwith "Account is closed"

    // Test!!
    let ``Account can be updated from multiple threads`` updateFn =
        let account = 
            mkBankAccount()
            |> openAccount

        let updateAccountAsync =        
            async {                             
                account 
                |> updateFn 1.0m
                |> ignore
            }

        updateAccountAsync
        |> List.replicate 1000
        |> Async.Parallel 
        |> Async.RunSynchronously
        |> ignore

        getBalance account //= (Some 1000.0m)

    ``Account can be updated from multiple threads`` updateBalance
    ``Account can be updated from multiple threads`` updateBalance0
