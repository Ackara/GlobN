using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Acklann.GlobN
{
    [MemoryDiagnoser]
    public class EvaluationTest
    {
        [Params("file.txt", "*.txt", "temp/**/file.txt", "mock/**/*.png")]
        public string Pattern { get; set; }

        [Benchmark]
        public bool IsMatch()
        {
            return Glob.IsMatch(@"C:\Temp\GlobN\mock\lvl-A\file.txt", Pattern);
        }

        [Benchmark]
        public int ResovlePaths()
        {
            string[] files = GlobExtensions.ResolvePaths(Pattern, Mock.RootDirectory).ToArray();
            return files.Length;
        }
    }
}