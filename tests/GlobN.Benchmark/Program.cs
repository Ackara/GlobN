using BenchmarkDotNet.Running;

namespace Acklann.GlobN.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkRunner.Run<GlobBenchmark>();
        }
    }
}