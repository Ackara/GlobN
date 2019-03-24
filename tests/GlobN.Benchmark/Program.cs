using Acklann.VBench;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Acklann.GlobN
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, DefaultConfig.Instance
                .With(new TimelineExporter()));
        }
    }
}