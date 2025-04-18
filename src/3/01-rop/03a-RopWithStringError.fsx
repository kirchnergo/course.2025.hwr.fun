﻿(*
Railway oriented programming
*)
open System
//#load "Result.fsx"

type Request = {
    UserId: int
    Name: string
    Email: string
}

let nameNotBlank input =
  if input.Name = "" then
    Error "Name must not be blank"
  else
    Ok input

let name50 input =
  if input.Name.Length > 50 then
    Error "Name must not be longer than 50 chars"
  else
    Ok input

let emailNotBlank input =
  if input.Email = "" then
    Error "Email must not be blank"
  else
    Ok input


let validateRequest input =
  input
  |> nameNotBlank
  |> Result.bind name50
  |> Result.bind emailNotBlank


// -------------------------------
// test data
// -------------------------------
let goodRequest = {
  UserId=0
  Name= "Alice"
  Email="ABC@gmail.COM"
}
goodRequest |> validateRequest

let badRequest1 = {
  UserId=0
  Name= ""
  Email="abc@example.com"
}
badRequest1 |> validateRequest

let unsendableRequest = {
  UserId=0
  Name= "Alice"
  Email="ABC@example.COM"
}
unsendableRequest |> validateRequest


// ------------------------
// Add another step
// ------------------------

// trim spaces and lowercase
let canonicalizeEmail (input:Request) =
  { input with Email = input.Email.Trim().ToLower() }



// test so far
goodRequest
|> validateRequest
|> Result.map canonicalizeEmail

(*
let canonicalizeEmailR =   // value restriction error
  Result.map canonicalizeEmail
*)

let canonicalizeEmailR twoTrackInput =  // value restriction error fixed!!!
  twoTrackInput |> Result.map canonicalizeEmail

goodRequest
|> validateRequest
|> canonicalizeEmailR


// ------------------------
// Update the database
// ------------------------

let updateDb (request:Request) =
    // do something
    // return nothing at all
    printfn "Database updated with userId=%i email=%s" request.UserId request.Email
    ()

let tee f result =
  f result
  result

let updateDbR twoTrackInput =
  twoTrackInput |> Result.map (tee updateDb)


// test so far
goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR


// ------------------------
// Send an email
// ------------------------

let sendEmail (request:Request) =
    if request.Email.EndsWith("example.com") then
        failwithf "Can't send email to %s" request.Email
    else
        printfn "Sending email=%s" request.Email
        request // return request for processing by next step

(*
// this code will throw an exception :(
unsendableRequest
|> validateRequest
|> canonicalizeEmailR
|> Result.map sendEmail
*)

// The fix is to convert the exception-throwing code
// into Result-returning code


let catch exceptionThrowingFunction handler oneTrackInput =
    try
        Ok (exceptionThrowingFunction oneTrackInput)
    with
    | ex ->
        Error (handler ex)

let catchR exceptionThrowingFunction exnConverter =
    Result.bind (catch exceptionThrowingFunction exnConverter)

let sendEmailR twoTrackInput =
    let exnConverter (ex:exn) = ex.Message
    catchR sendEmail exnConverter twoTrackInput


// test so far
goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR

unsendableRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR


// ------------------------
// log the errors
// ------------------------

let loggerR twoTrackInput =
    match twoTrackInput with
    | Ok (req:Request) ->
        printfn "LOG INFO Name=%s EMail=%s" req.Name req.Email
    | Error err ->
        printfn "LOG ERROR %s" err
    twoTrackInput

// test so far
goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR
|> loggerR


// ------------------------
// return the response
// ------------------------

let returnMessageR result =
    match result with
    | Ok obj ->
        sprintf "200 %A" obj
    | Error msg ->
        sprintf "400 %s" msg


// test so far
goodRequest
|> validateRequest
|> canonicalizeEmailR
|> updateDbR
|> sendEmailR
|> loggerR
|> returnMessageR


// ------------------------
// final code
// ------------------------

let updateCustomerR input =
  input
  |> validateRequest
  |> canonicalizeEmailR
  |> updateDbR
  |> sendEmailR
  |> loggerR
  |> returnMessageR


// test
goodRequest
|> updateCustomerR

badRequest1
|> updateCustomerR

unsendableRequest
|> updateCustomerR



