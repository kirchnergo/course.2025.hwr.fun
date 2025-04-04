

# Ziel 


## Programm

-   Domain Driven Design (DDD)
-   Property Based Testing


# DDD (Domain Driven Design) 


## DDD

$\leadsto$ [Domain Driven Design](./4.1 Dmmf.pdf)

\null\hfill&#x2013;Scott Wlashin: [F# for Fun and Profit](https://fsharpforfunandprofit.com/series/designing-with-types/)


## Prinzipien

-   Verwende die Sprache der Domäne (ubiquitous Language)
-   Values und Entities
-   der Code ist das Design (kein UML nötig)
-   Design mit (algebraischen) Typen
    -   Option statt Null
    -   DU statt Vererbung
-   illegale Konstellationen sollten nicht repräsentierbar sein!


## Pause

1.  

    Are you quite sure that all those bells and whistles, all those wonderful facilities of your so called powerful programming languages, belong to the solution set rather than the problem set?
    
    \null\hfill &#x2013; Edsger Dijkstra


## DDD Übung 1 (Contacts &#x2013; ex 2)

A Contact has

-   a personal name
-   an optional email address
-   an optional postal address
-   Rule: a contact must have an email or a postal address

A Personal Name consists of a first name, middle initial, last name

-   Rule: the first name and last name are required
-   Rule: the middle initial is optional
-   Rule: the first name and last name must not be more than 50 chars
-   Rule: the middle initial is exactly 1 char, if present

A postal address consists of a four address fields plus a country

-   Rule: An Email Address can be verified or unverified


## DDD Übung 2 (Payments &#x2013; ex 3)

The payment taking system should accept:

-   Cash
-   Credit cards
-   Cheques
-   Paypal
-   Bitcoin

A payment consists of a:

-   payment
-   non-negative amount

After designing the types, create functions that will:

-   print a payment method
-   print a payment, including the amount
-   create a new payment from an amount and method


## DDD Übung 3 (Refactoring &#x2013; ex 4)

Much C# code has implicit states that you can recognize by fields called "IsSomething", or nullable date.

This is a sign that states transitions are present but not being modelled properly.


## DDD Übung 4 (Shopping Cart &#x2013; fsm ex 3)

Create types that model an e-commerce shopping cart.

-   Rule: "You can't remove an item from an empty cart"!
-   Rule: "You can't change a paid cart"!
-   Rule: "You can't pay for a cart twice"!

States are:

-   Empty
-   ActiveCartData
-   PaidCartData


## Pause

1.  

    About the use of language: it is impossible to sharpen a pencil with a blunt axe. 
    It is equally vain to try to do it with ten blunt axes instead.
    
    \null\hfill &#x2013; Edsger Dijkstra


# Property Based Testing 


## Example Based Tests :)

    module Test1 =
        open Implementation1
        let tests = testList "implementation 1" [
            test "add 1 3 = 4" {
                let actual = add 1 3
                let expected = 4
                Expect.equal expected actual "" }
            test "add 2 2 = 4" {
                let actual = add 2 2
                let expected = 4
                Expect.equal expected actual "" } ];;
    runTests expectoConfig Test1.tests

    runTests expectoConfig Test1.tests;;
    Expecto Running...
    [14:51:49 INF] EXPECTO? Running tests... <Expecto>
    [14:51:49 INF] EXPECTO! 2 tests run in 00:00:00.0366264 for implementation 1 – 2 passed, 0 ignored, 0 failed, 0 errored. Success! <Expecto>
    val it: int = 0


## Evil Developer From Hell :(

    module Implementation1 =
        let add x y =
            4

    module Implementation1 =
      val add: x: 'a -> y: 'b -> int


## PBT

$\leadsto$ [Property Based Testing](./4.2 An introduction to property based testing.pdf)

\null\hfill&#x2013;Scott Wlashin: [F# for Fun and Profit](https://fsharpforfunandprofit.com/series/property-based-testing/)


## FsCheck

    let add1 x y = x + y
    let add2 x y = x - y
    let commutativeProperty f x y =
       let result1 = f x y
       let result2 = f y x
       result1 = result2;;
    FsCheck.Check.Quick (commutativeProperty add1)
    FsCheck.Check.Quick (commutativeProperty add2)

    FsCheck.Check.Quick (commutativeProperty add1)
    FsCheck.Check.Quick (commutativeProperty add2);;
    Ok, passed 100 tests.
    Falsifiable, after 1 test (3 shrinks) (StdGen (568786575, 297460729)):
    Original:
    2
    -2
    Shrunk:
    0
    1
    val it: unit = ()


## FsCheck (Generate)

    type Temp = F of int | C of float;;
    let fGen =
        FsCheck.Gen.choose(32,212)
        |> FsCheck.Gen.map (fun i -> F i);;
    let cGen =
        FsCheck.Gen.choose(0,100)
        |> FsCheck.Gen.map (fun i -> C (float i));;
    let tempGen =
        FsCheck.Gen.oneof [fGen; cGen]
    
    let test = tempGen |> FsCheck.Gen.sample 0 20
    test

    let tempGen =
        FsCheck.Gen.oneof [fGen; cGen]
    
    let test = tempGen |> FsCheck.Gen.sample 0 20
    test;;
    val tempGen: Gen<Temp> = Gen <fun:Bind@88>
    val test: Temp list =
      [C 90.0; F 156; F 171; C 94.0; C 84.0; C 3.0; C 94.0; F 38; F 187; C 23.0;
       F 132; F 190; F 206; F 120; F 57; F 69; F 175; F 178; C 35.0; F 47]
    val it: Temp list =
      [C 90.0; F 156; F 171; C 94.0; C 84.0; C 3.0; C 94.0; F 38; F 187; C 23.0;
       F 132; F 190; F 206; F 120; F 57; F 69; F 175; F 178; C 35.0; F 47]


## FsCheck (Shrink)

    open FsCheck
    let smallerThan81Property x = x < 81
    FsCheck.Check.Quick smallerThan81Property
    
    let test1 = FsCheck.Arb.shrink 100 |> Seq.toList
    let test2 = FsCheck.Arb.shrink 88 |> Seq.toList
    test2

    Falsifiable, after 90 tests (2 shrinks) (StdGen (569811125, 297460729)):
    Original:
    84
    Shrunk:
    81
    val smallerThan81Property: x: int -> bool
    val test1: int list = [0; 50; 75; 88; 94; 97; 99]
    val test2: int list = [0; 44; 66; 77; 83; 86; 87]
    val it: int list = [0; 44; 66; 77; 83; 86; 87]


## Auswahl der Eigenschaften

-   Unterschiedlicher Weg, gleiches Ziel (Map(f)(Option(x))=Option(f x))
-   Hin und Her (z.B. Reverse einer Liste)
-   Invarianten (z.B. Länge einer Liste bei Sortierung)
-   Idempotenz (noch einmal bringt nichts mehr)
-   Divide et Impera! (teile und herrsche)
-   Hard to prove, easy to verify (Primzahlzerlegung)
-   Test-Orakel (z.B. einfach aber langsam)


# Ende 


## Zusammenfassung

-   funktionales Domain Modeling (DDD)
-   eigenschaftsbasiertes Testen (Property Based Testing)


## Links

-   [Domain Driven Design](https://fsharpforfunandprofit.com/ddd/)
-   [Domain Modeling Made Functional](https://fsharpforfunandprofit.com/books/)
-   [FsCheck](https://github.com/fscheck/FsCheck)
-   [An introduction to property-based testing](https://fsharpforfunandprofit.com/posts/property-based-testing/)
-   [Choosing properties for property-based testing](https://fsharpforfunandprofit.com/posts/property-based-testing-2/)


## Hausaufgabe  (Erinnerung)

-   exercism.io (bis 07.04)
    -   [ ] Accumulate
    -   [ ] Space Age
-   exercism.io (bis 07.04)
    -   [ ] Poker (Programmieraufgabe)


## Termine

-   [X] 12.03 11:30 - 15:45
-   [X] 26.03 11:30 - 15:45 (online)
-   [X] 02.04  8:45 - 11:45 (online)
-   [X] 04.04 13:00 - 17:15 (online)
-   [ ] 09.04 11:30 - 15:45

