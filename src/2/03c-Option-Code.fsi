type BillingDetails = { name : string;
    billing : string;
    delivery : string option } 

let order1 = {
    name = "Adam Smith"
    billing = "112 Fibonacci Street\n35813" 
    delivery = None }

let order2 = {
    name = "John Doe"
    billing = "314 Pi Avenue\n35999"
    delivery = Some "16 Planck Parkway\n62291" }

open System
let tryLastLine (address : string) =
    let parts = address.Split([|'\n'|], StringSplitOptions.RemoveEmptyEntries) 
    parts |> Array.tryLast
let tryPostalCode (codeString : string) = 
    match Int32.TryParse(codeString) with 
        | true, i -> i |> Some
        | false, _ -> None
let postalCodeHub (code : int) =
    if code = 62291 then "Hub 1" else "Hub 2"

let tryHub (details : BillingDetails) = 
    details.delivery
    |> Option.bind tryLastLine 
    |> Option.bind tryPostalCode 
    |> Option.map postalCodeHub

let test1 = order1 |> tryHub 
let test2 = order2 |> tryHub