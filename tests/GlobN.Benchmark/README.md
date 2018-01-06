# Glob Comparison Benchmark



## Results

``` ini

BenchmarkDotNet=v0.10.11, OS=Windows 10 Redstone 3 [1709, Fall Creators Update] (10.0.16299.125)
Processor=Intel Core i5-3317U CPU 1.70GHz (Ivy Bridge), ProcessorCount=4
Frequency=1656394 Hz, Resolution=603.7211 ns, Timer=TSC
.NET Core SDK=2.1.2
  [Host]     : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT
  DefaultJob : .NET Core 2.0.3 (Framework 4.6.25815.02), 64bit RyuJIT


```
|               Method |      Mean |     Error |    StdDev | Scaled | Rank |
|--------------------- |----------:|----------:|----------:|-------:|-----:|
| DotNet.Globbing.Glob |  1.219 ms | 0.0013 ms | 0.0012 ms |   0.05 |    I |
|                GlobN |  1.951 ms | 0.0093 ms | 0.0087 ms |   0.08 |   II |
|            Glob.Glob |  8.658 ms | 0.0136 ms | 0.0121 ms |   0.36 |  III |
|                Regex | 24.130 ms | 0.0307 ms | 0.0287 ms |   1.00 |   IV |

