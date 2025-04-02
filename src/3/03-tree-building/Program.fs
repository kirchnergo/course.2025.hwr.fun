// This file was created manually and its version is 1.0.0.
// This file supports running the performance benchmarks. Do not modify it.

open System.Collections.Generic
open System.Linq

// open BenchmarkDotNet.Columns
// open BenchmarkDotNet.Configs
// open BenchmarkDotNet.Diagnosers
// open BenchmarkDotNet.Loggers
// open BenchmarkDotNet.Reports
open BenchmarkDotNet.Attributes
open BenchmarkDotNet.Running

open TreeBuildingBenchmark


[<EntryPoint>]
let main _ =
    BenchmarkRunner.Run typeof<Benchmarks> |> ignore

    //BenchmarkRunner.Run<Benchmarks>(BenchmarkConfig()) |> ignore

    0
