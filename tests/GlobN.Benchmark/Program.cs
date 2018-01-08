using BenchmarkDotNet.Attributes;
using System.Linq;
using System.Reflection;

namespace Acklann.GlobN.Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            var instance = new GlobBenchmark();
            foreach (var method in (from m in typeof(GlobBenchmark).GetMethods()
                                    where m.GetCustomAttribute<BenchmarkAttribute>() != null
                                    orderby m.Name.Length
                                    select m))
            {
                System.Console.WriteLine(string.Format(" {0,-15} {1:0,00} matches", method.Name, method.Invoke(instance, new object[0])));
            }

            System.Console.WriteLine();
            System.Console.WriteLine("press any key to exit ...");
            System.Console.ReadKey();
#else
            BenchmarkDotNet.Running.BenchmarkRunner.Run<GlobBenchmark>();
#endif
        }
    }
}