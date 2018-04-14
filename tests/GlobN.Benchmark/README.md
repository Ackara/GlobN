# Glob Benchmark
This project benchmarks the speed of other Glob/Minimatch libraries on nuget.org against the `System.Text.RegularExpressions.Regex` class. The test is conducted by measuring the time it takes for each class to find all the matches within a list of 1,000 file paths.

## Results

``` ini

BenchmarkDotNet=v0.10.14, OS=Windows 10.0.16299.309 (1709/FallCreatorsUpdate/Redstone3)
Intel Core i5-3317U CPU 1.70GHz (Ivy Bridge), 1 CPU, 4 logical and 2 physical cores
Frequency=1656401 Hz, Resolution=603.7185 ns, Timer=TSC
.NET Core SDK=2.1.103
  [Host]     : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.6 (CoreCLR 4.6.26212.01, CoreFX 4.6.26212.01), 64bit RyuJIT


```
|      Method |      Mean |     Error |    StdDev | Scaled | Rank |      Gen 0 |   Allocated |
|------------ |----------:|----------:|----------:|-------:|-----:|-----------:|------------:|
| DotNet.Glob |  1.230 ms | 0.0054 ms | 0.0045 ms |   0.05 |    1 |     3.9063 |     8.92 KB |
|       GlobN |  2.864 ms | 0.0108 ms | 0.0096 ms |   0.12 |    2 |  1320.3125 |  2031.06 KB |
|        Glob |  8.403 ms | 0.0251 ms | 0.0222 ms |   0.35 |    3 | 10484.3750 | 16108.56 KB |
|       Regex | 24.306 ms | 0.0646 ms | 0.0605 ms |   1.00 |    4 |          - |    21.55 KB |

