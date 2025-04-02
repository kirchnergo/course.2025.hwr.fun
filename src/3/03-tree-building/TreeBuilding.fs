// This is the file you need to modify for your own solution.
// The unit tests will use this code, and it will be used by the benchmark tests
// for the "Mine" row of the summary table.

// Remember to not only run the unit tests for this exercise, but also the
// benchmark tests using `dotnet run -c Release`.
// Please refer to the instructions for more information about the benchmark tests.

module TreeBuilding

open TreeBuildingTypes

type Tree =
    | Branch of int * Tree list
    | Leaf of int

let recordId t =
    match t with
    | Branch (id, _) -> id
    | Leaf id -> id

let isBranch t =
    match t with
    | Branch _ -> true
    | _ -> false

let children t =
    match t with
    | Branch (i_, c) -> c
    | _ -> []

let validate records =
    match records with
    | [] -> failwith "Input must be non-empty"
    | x :: _ when x.RecordId <> 0 -> failwith "Root must have id 0"
    | x :: _ when x.ParentId <> 0 -> failwith "Root node must have parent id 0"
    | _ :: xs when xs |> List.exists (fun r -> r.RecordId < r.ParentId) -> failwith "ParentId should be less than RecordId"
    | _ :: xs when xs |> List.exists (fun r -> r.RecordId = r.ParentId) -> failwith "ParentId cannot be the RecordId except for the root node."
    | rs when (rs |> List.map (fun r -> r.RecordId) |> List.max) > (List.length rs - 1) -> failwith "Ids must be continuous"
    | _ -> records

let rec makeTree id map =
    match map |> Map.tryFind id with
    | None -> Leaf id
    | Some list -> Branch (id, list |> List.map (fun r -> makeTree r.RecordId map))

let buildTree records = 
    records
    |> List.sortBy (fun r -> r.RecordId)
    |> validate
    |> List.tail
    |> List.groupBy (fun r -> r.ParentId)
    |> Map.ofList
    |> makeTree 0

let input =
    [
        { RecordId = 5; ParentId = 2 };
        { RecordId = 3; ParentId = 2 };
        { RecordId = 2; ParentId = 0 };
        { RecordId = 4; ParentId = 1 };
        { RecordId = 1; ParentId = 0 };
        { RecordId = 0; ParentId = 0 };
        { RecordId = 6; ParentId = 2 }
    ]

let test = buildTree input
