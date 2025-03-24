module ListOpsSolution

let length list = 
    let rec _length list acc =
        match list with
        | [] -> acc
        | _::xs -> _length xs (acc + 1)
    _length list 0    

let reverse list =
    let rec _reverse list acc =
        match list with
        | [] -> acc
        | x::xs -> _reverse xs (x::acc)
    _reverse list []

let rec foldl folder state list = 
    match list with
    | [] -> state
    | x::xs -> foldl folder (folder state x) xs

let flip f b a = f a b 
let rec foldr folder state list = 
    foldl (flip folder) state (reverse list)

let map f list = 
    let rec _map f list acc =
        match list with
        | [] -> acc |> reverse
        | x::xs -> _map f xs ((f x)::acc)
    _map f list []    

let filter f list = 
    let rec _filter f list acc = 
        match list with
        | [] -> acc |> reverse
        | x::xs ->  match f x with
                    | true -> _filter f xs (x::acc)
                    | false -> _filter f xs acc
    _filter f list []

let append xs ys = foldr (fun x acc -> x :: acc) ys xs

let concat xs = foldr append [] xs

// let rec concat xs = 
//     match xs with
//     | [] -> []
//     | []::ys -> concat ys
//     | (x::xs)::ys -> x:: (concat (xs::ys))

// let concat xs =
//     let rec _concat xs acc = 
//         match xs with
//         | [] -> acc |> reverse
//         | []::ys -> _concat ys acc
//         | (x::xs)::ys -> _concat (xs::ys) (x::acc)
//     _concat xs []
