// online Football 
// (the sport where participants kick a spherical ball with their feet 
// rather than throw an egg-shaped object with their hands) 
// Score Predictor:

// The rules are simple;
// • 300 points for predicting the correct score (e.g. 2-3 vs 2-3)
// • 100 points for predicting the correct result (e.g. 2-3 vs 0-2)
// • 15 points per home goal & 20 points per away goal using the lower of the predicted and actual scores

// We have some sample predictions, actual scores, and expected calculated points 
// that we can use to validate the code we write;
// (0, 0) (0, 0) = 400 // 300 + 100 + (0 * 15) + (0 * 20) 
// (3, 2) (3, 2) = 485 // 300 + 100 + (3 * 15) + (2 * 20) 
// (5, 1) (4, 3) = 180 // 0 + 100 + (4 * 15) + (1 * 20) 
// (2, 1) (0, 7) = 20 // 0 + 0 + (0 * 15) + (1 * 20)
// (2, 2) (3, 3) = 170 // 0 + 100 + (2 * 15) + (2 * 20) 

// Firstly we define a simple tuple type to represent a score:
type Score = int * int

// Score * Score -> option<unit>
let (|CorrectScore|_|) (expected:Score, actual:Score) = 
    if expected = actual then Some () else None

let (|Draw|HomeWin|AwayWin|) (score:Score) = 
    match score with
    | (h, a) when h = a -> Draw
    | (h, a) when h > a -> HomeWin
    | _ -> AwayWin

let (|CorrectResult|_|) (expected:Score, actual:Score) = 
    match (expected, actual) with
    | (Draw, Draw) -> Some ()
    | (HomeWin, HomeWin) -> Some ()
    | (AwayWin, AwayWin) -> Some ()
    | _ -> None

let goalScore (expected:Score) (actual:Score) =
    let home = [ fst expected; fst actual ] |> List.min 
    let away = [ snd expected; snd actual ] |> List.min 
    (home * 15) + (away * 20)

let calculatePoints (expected:Score) (actual:Score) = 
    let pointsForCorrectScore =
        match (expected, actual) with 
        | CorrectScore -> 300
        | _ -> 0
    let pointsForCorrectResult = 
        match (expected, actual) with 
        | CorrectResult -> 100
        | _ -> 0
    let pointsForGoals = goalScore expected actual 
    pointsForCorrectScore + pointsForCorrectResult + pointsForGoals

// We can simplify the calculatePoints function by combining the pattern matching for 
// CorrectScore and CorrectResult into a new function:
let resultScore (expected:Score) (actual:Score) = 
    match (expected, actual) with
    | CorrectScore -> 400
    | CorrectResult -> 100
    | _ -> 0

// Note that we had to return 400 from CorrectScore in this function 
// as we are no longer able to add the CorrectResult points later. 
// This allows us to simplify the calculatePoints function:
let calculatePoints (expected:Score) (actual:Score) = 
    let pointsForResult = resultScore expected actual 
    let pointsForGoals = goalScore expected actual 
    pointsForResult + pointsForGoals

// As the resultScore and goalScore functions have the same signature, 
// we can use a higher order function to remove the duplication:
let calculatePoints (expected:Score) (actual:Score) = 
    [ resultScore; goalScore ]
    |> List.sumBy (fun f -> f expected actual)


let assertnoScoreDrawCorrect = calculatePoints (0, 0) (0, 0) = 400
let assertHomeWinExactMatch = calculatePoints (3, 2) (3, 2) = 485
let assertHomeWin = calculatePoints (5, 1) (4, 3) = 180
let assertIncorrect = calculatePoints (2, 1) (0, 7) = 20
let assertDraw = calculatePoints (2, 2) (3, 3) = 170
