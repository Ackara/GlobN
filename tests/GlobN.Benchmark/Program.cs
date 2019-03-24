using Acklann.VBench;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using System;

namespace Acklann.GlobN
{
    internal class Program
    {
        private static void Main(string[] args)
        {
#if DEBUG
            var result = (new ResolveTest().ResovlePaths());
            Console.ReadKey();
#endif
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, DefaultConfig.Instance
                .With(new TimelineExporter()));
        }
    }
}