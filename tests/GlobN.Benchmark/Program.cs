namespace Acklann.GlobN.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            var test = new GlobBenchmark();
            string format = "{1:0,00}  {0}";

            System.Console.WriteLine(string.Format(format, nameof(GlobBenchmark.GlobN), test.GlobN()));
            System.Console.WriteLine(string.Format(format, nameof(GlobBenchmark.DotNetGlob), test.DotNetGlob()));
            System.Console.WriteLine(string.Format(format, nameof(GlobBenchmark.GlobGlob), test.GlobGlob()));
            System.Console.WriteLine(string.Format(format, nameof(GlobBenchmark.Regex), test.Regex()));

            System.Console.WriteLine("press any key to exit ...");
            System.Console.ReadKey();
#else
            BenchmarkDotNet.Running.BenchmarkRunner.Run<GlobBenchmark>();
#endif
        }
    }
}