
# Organisatorisches 


## Organisatorisches

-   Termine
    -   [12.03, 26.03, 02.04?, 04.04, 09.04, 16.04?, 29.04]
-   Bewertung
    -   .50: Hausaufgaben (10)
    -   .25: Programmieraufgabe (in der vorvorletzten Einheit)
    -   .25: Test (multiple choice, in der letzten Einheit)
-   Folien und Code auf [github.com/kirchnergo/course.2025.hwr.fun](https://github.com/kirchnergo/course.2025.hwr.fun)


## Was ist F#

F# ist eine funktionale Programmiersprache, die das Schreiben von korrektem und verwaltbarem Code erleichtert.
In F# können Typen und Funktionen geschrieben werden, deren Typen automatisch generalisiert werden. Dies ermöglicht es, sich auf die Problemdomäne zu konzentrieren und die Daten zu bearbeiten, statt sich um die Details der Programmierung zu kümmern.

F# verfügt über zahlreiche Features, einschließlich:

-   Leichtgewichtige Syntax
-   Standardmäßig unveränderliche Werte
-   Typrückschluss und automatische Generalisierung
-   Funktionen erster Klasse
-   Musterabgleich
-   Asynchrone Programmierung


## So sieht F# aus

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

