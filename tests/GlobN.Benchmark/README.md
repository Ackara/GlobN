# Glob Benchmark
This project benchmarks the speed of other Glob/Minimatch libraries on nuget.org against the `System.Text.RegularExpressions.Regex` class. The test is conducted by measuring the time it takes for each class to find all the matches within a list of 1,000 file paths.

## Results

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.17134
Intel Core i7-6700K CPU 4.00GHz (Skylake), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=2.1.201
  [Host]     : .NET Core 2.0.7 (CoreCLR 4.6.26328.01, CoreFX 4.6.26403.03), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.7 (CoreCLR 4.6.26328.01, CoreFX 4.6.26403.03), 64bit RyuJIT


```
|      Method |        Mean |      Error |    StdDev | Scaled | Rank |     Gen 0 |   Allocated |
|------------ |------------:|-----------:|----------:|-------:|-----:|----------:|------------:|
|       GlobN |    578.4 us |  0.3824 us | 0.3193 us |   0.05 |    1 |         - |     1.13 KB |
| DotNet.Glob |    604.5 us |  0.5462 us | 0.5109 us |   0.05 |    2 |    1.9531 |     8.92 KB |
|        Glob |  4,096.2 us |  9.0823 us | 8.0513 us |   0.36 |    3 | 3929.6875 | 16108.56 KB |
|       Regex | 11,349.3 us | 11.2148 us | 9.9416 us |   1.00 |    4 |         - |    21.55 KB |

