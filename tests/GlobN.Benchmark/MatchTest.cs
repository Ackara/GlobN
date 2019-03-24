using BenchmarkDotNet.Attributes;

namespace Acklann.GlobN
{
    [MemoryDiagnoser]
    public class MatchTest
    {
        [Params("file.txt", "*.txt", "temp/**/file.txt", "mock/**/*.png")]
        public string Pattern { get; set; }

        [Benchmark]
        public bool IsMatch()
        {
            return Glob.IsMatch(@"C:\Temp\GlobN\mock\lvl-A\file.txt", Pattern);
        }
    }
}