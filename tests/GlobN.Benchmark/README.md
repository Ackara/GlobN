# Glob Benchmark
This project benchmarks the speed of other Glob/Minimatch libraries on nuget.org against the `System.Text.RegularExpressions.Regex` class. The test is conducted by measuring the time it takes for each class to find all the matches within a list of 1,000 file paths.

## Results

``` ini

BenchmarkDotNet=v0.10.11, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.125)
Processor=Intel Core i5-3317U CPU 1.70GHz (Ivy Bridge), ProcessorCount=4
Frequency=1656394 Hz, Resolution=603.7211 ns, Timer=TSC
.NET Core SDK=2.1.2
  [Host]     : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|      Method |      Mean |     Error |    StdDev | Scaled | Rank |
|------------ |----------:|----------:|----------:|-------:|-----:|
| DotNet.Glob |  1.216 ms | 0.0013 ms | 0.0011 ms |   0.05 |    I |
|       GlobN |  2.070 ms | 0.0031 ms | 0.0025 ms |   0.09 |   II |
|        Glob |  8.721 ms | 0.0411 ms | 0.0385 ms |   0.36 |  III |
|       Regex | 23.987 ms | 0.0187 ms | 0.0175 ms |   1.00 |   IV |

