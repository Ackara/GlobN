# Glob Benchmark
This project benchmarks the speed of other Glob/Minimatch libraries on nuget.org against the `System.Text.RegularExpressions.Regex` class. The test is conducted by measuring the time it takes for each class to find all the matches within a list of 1,000 file paths.

## Results

``` ini

BenchmarkDotNet=v0.10.12, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.192)
Intel Core i5-3317U CPU 1.70GHz (Ivy Bridge), 1 CPU, 4 logical cores and 2 physical cores
Frequency=1656403 Hz, Resolution=603.7178 ns, Timer=TSC
.NET Core SDK=2.1.4
  [Host]     : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.5 (Framework 4.6.26020.03), 64bit RyuJIT


```
|      Method |      Mean |     Error |    StdDev | Scaled | Rank |
|------------ |----------:|----------:|----------:|-------:|-----:|
| DotNet.Glob |  1.255 ms | 0.0052 ms | 0.0046 ms |   0.05 |    I |
|       GlobN |  2.899 ms | 0.0060 ms | 0.0050 ms |   0.12 |   II |
|        Glob |  8.733 ms | 0.0172 ms | 0.0134 ms |   0.35 |  III |
|       Regex | 24.735 ms | 0.1580 ms | 0.1478 ms |   1.00 |   IV |

