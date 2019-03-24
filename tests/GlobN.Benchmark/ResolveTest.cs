using BenchmarkDotNet.Attributes;
using System.Linq;

namespace Acklann.GlobN
{
    [MemoryDiagnoser]
    public class ResolveTest
    {
        [Params("file.txt", "*.txt", "temp/**/file.txt", "mock/**/*.png")]
        public string Pattern { get; set; }

        [Benchmark]
        public int ResovlePaths()
        {
            string[] files = GlobExtensions.ResolvePaths(Pattern, Mock.RootDirectory).ToArray();
            return files.Length;
        }
    }
}