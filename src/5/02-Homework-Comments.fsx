// -   [ ] Poker

// Allgemein

// - Publish on exercism.io
// - Formatierung (dotnet fantomas)
// - Vermeide "mutable"!!
// - nur wichtiges verdient einen Namen
// - Vertraue der "Pipe"!!
// - If-Then-Else mit Boolean ist unnötig
// - Parametrisiere!

// - If-Then-Else vermeiden ... besser match
// - Be lazy! ("for" vermeiden)


// Poker

module Poker = 

    open System

    type PokerFailure =
        | InvalidRank of char
        | InvalidSuit of char
        | InvalidCardSyntax of string
        | HandHasDuplicates
        | InvalidHandSize
        | ShouldNotHappen

    type Rank =
        | Two
        | Three
        | Four
        | Five
        | Six
        | Seven
        | Eight
        | Nine
        | Ten
        | Jack
        | Queen
        | King
        | Ace

    type Suit =
        | Clubs
        | Spades
        | Hearts
        | Diamonds

    let parseRank =
        function
        | '2' -> Ok Two
        | '3' -> Ok Three
        | '4' -> Ok Four
        | '5' -> Ok Five
        | '6' -> Ok Six
        | '7' -> Ok Seven
        | '8' -> Ok Eight
        | '9' -> Ok Nine
        | 'T' -> Ok Ten
        | 'J' -> Ok Jack
        | 'Q' -> Ok Queen
        | 'K' -> Ok King
        | 'A' -> Ok Ace
        | char -> Error [ InvalidRank char ]

    let parseSuit =
        function
        | 'C' -> Ok Clubs
        | 'S' -> Ok Spades
        | 'H' -> Ok Hearts
        | 'D' -> Ok Diamonds
        | c -> Error [ InvalidSuit c ]

    type Card = Rank * Suit

    let toUpperArray (card: string) = card.ToUpper().ToCharArray()

    let toResultTuple (card: char []) =
        match card with
        | [| (r: char); (s: char) |] -> Ok(r, s)
        | _ -> Error [ InvalidCardSyntax(string card) ]

    let mergeResult =
        function
        | (Ok r1, Ok r2) -> Ok(r1, r2)
        | (Ok _, Error r2) -> Error r2
        | (Error r1, Ok _) -> Error r1
        | (Error r1, Error r2) -> Error(List.append r1 r2)

    let parseCard (card: string): Result<Card, PokerFailure list> =
        card.Replace("10", "T")
        |> toUpperArray
        |> toResultTuple
        |> Result.bind (fun (r, s) ->
            let rank = parseRank r
            let suit = parseSuit s
            mergeResult (rank, suit))

    type Hand = Card * Card * Card * Card * Card

    let tokenize (hand: string) = hand.Split [| ' ' |] |> Array.toList

    let validate (cards) =
        let unique = List.groupBy id cards
        if unique.Length = cards.Length then Ok cards else Error [ HandHasDuplicates ]

    let cardsToHand cards: Result<Hand, PokerFailure list> =
        match cards with
        | [ Ok c0; Ok c1; Ok c2; Ok c3; Ok c4 ] -> Ok(c0, c1, c2, c3, c4)
        | [ _; _; _; _; _ ] ->
            let fails =
                cards
                |> List.choose (function
                    | Ok _ -> None
                    | Error f -> Some f)
                |> List.collect id
            Error fails
        | _ -> Error [ InvalidHandSize ]

    let parseHand (hand: string): Result<Hand, PokerFailure list> =
        hand
        |> tokenize
        |> validate
        |> Result.map (List.map (fun cards -> cards |> parseCard))
        |> Result.map List.sort
        |> Result.bind cardsToHand

    let toArray (hand: Hand) =
        let (c1, c2, c3, c4, c5) = hand
        [| c1; c2; c3; c4; c5 |]

    let isNextRank (r1: Rank, r2: Rank) =
        match (r1, r2) with
        | (Two, Three)
        | (Three, Four)
        | (Four, Five)
        | (Five, Six)
        | (Six, Seven)
        | (Seven, Eight)
        | (Eight, Nine)
        | (Nine, Ten)
        | (Ten, Jack)
        | (Jack, Queen)
        | (Queen, King)
        | (King, Ace) -> true
        | _ -> false

    let isStraight (hand: Hand) =
        match hand with
        | ((Two, _), (Three, _), (Four, _), (Five, _), (Ace, _)) -> true
        | _ ->
            (toArray
             >> Array.toSeq
             >> Seq.pairwise
             >> Seq.forall (fun ((r1, _), (r2, _)) -> isNextRank (r1, r2))) hand

    let isFlush (hand: Hand) =
        let cardArray = hand |> toArray
        cardArray |> Array.forall (fun card -> snd card = snd cardArray.[0])

    let getRankCounts (hand: Hand) =
        hand
        |> toArray
        |> Array.countBy (fun (r, _) -> r)
        |> Array.map (fun (rank, count) -> (count, rank))
        |> Array.sort
        |> Array.rev
        |> Array.toList

    type HandCategory =
        | HighCard of Rank * Rank * Rank * Rank * Rank
        | OnePair of Rank * Rank * Rank * Rank
        | TwoPair of Rank * Rank * Rank
        | ThreeKind of Rank * Rank
        | Straight of Rank
        | Flush of Rank
        | FullHouse of Rank * Rank
        | FourKind of Rank * Rank
        | StraightFlush of Rank
        | RoyalFlush

    let scoreHand (hand: Hand): Result<HandCategory, PokerFailure list> =
        let isStraight = isStraight hand
        let isFlush = isFlush hand
        let rankCounts = getRankCounts hand
        match (isStraight, isFlush, rankCounts) with
        | (true, true, [ (1, Ace); (1, Five); (1, Four); (1, Three); (1, Two) ]) -> Ok(StraightFlush Five)
        | (true, false, [ (1, Ace); (1, Five); (1, Four); (1, Three); (1, Two) ]) -> Ok(Straight Five)
        | (true, true, (1, Ace) :: _) -> Ok RoyalFlush
        | (true, true, (1, high) :: _) -> Ok(StraightFlush high)
        | (false, true, (1, high) :: _) -> Ok(Flush high)
        | (true, false, (1, high) :: _) -> Ok(Straight high)
        | (false, false, _) ->
            match rankCounts with
            | [ (4, rank); (1, kick) ] -> Ok(FourKind(rank, kick))
            | [ (3, rank3); (2, rank2) ] -> Ok(FullHouse(rank3, rank2))
            | [ (3, rank); (1, kick); _ ] -> Ok(ThreeKind(rank, kick))
            | [ (2, rH); (2, rL); (1, kick) ] -> Ok(TwoPair(rH, rL, kick))
            | [ (2, r); (1, k1); (1, k2); (1, k3) ] -> Ok(OnePair(r, k1, k2, k3))
            | [ (1, h); (1, k1); (1, k2); (1, k3); (1, k4) ] -> Ok(HighCard(h, k1, k2, k3, k4))
            | _ -> Error [ ShouldNotHappen ]
        | _ -> Error [ ShouldNotHappen ]

    let bestHands (hands: string list) =
        let score = parseHand >> Result.bind scoreHand
        let maxhand = hands |> List.maxBy score
        hands |> List.filter (fun h -> score h = score maxhand)

