``` ini

BenchmarkDotNet=v0.12.1, OS=macOS 13.7.4 (22H420) [Darwin 22.6.0]
Intel Core i7-7920HQ CPU 3.10GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=9.0.200
  [Host]     : .NET Core 9.0.2 (CoreCLR 9.0.225.6610, CoreFX 9.0.225.6610), X64 RyuJIT DEBUG
  DefaultJob : .NET Core 9.0.2 (CoreCLR 9.0.225.6610, CoreFX 9.0.225.6610), X64 RyuJIT


```
|   Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|--------- |---------:|----------:|----------:|------:|--------:|-------:|------:|------:|----------:|
| Baseline | 6.308 μs | 0.1256 μs | 0.2328 μs |  1.00 |    0.00 | 3.3188 |     - |     - |  13.56 KB |
|     Mine | 3.567 μs | 0.0703 μs | 0.1250 μs |  0.57 |    0.03 | 1.8196 |     - |     - |   7.45 KB |
