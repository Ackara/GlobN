using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Order;
using System.Linq;
using System.Text.RegularExpressions;

namespace Acklann.GlobN
{
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(BenchmarkDotNet.Mathematics.NumeralSystem.Stars)]
    public class GlobBenchmark
    {
        [Benchmark]
        public int GlobN()
        {
            int matches = 0;
            foreach (var pattern in GetGlobs())
            {
                var sut = new GlobN.Glob(pattern);
                foreach (var path in SamplePaths())
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        [Benchmark]
        public int DotNetGlob()
        {
            int matches = 0;
            foreach (var pattern in GetGlobs())
            {
                var sut = DotNet.Globbing.Glob.Parse(pattern);
                foreach (var path in SamplePaths())
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        [Benchmark(Baseline = true)]
        public int Regex()
        {
            int matches = 0;
            foreach (var pattern in GetRegex())
            {
                var sut = new Regex(pattern);
                foreach (var path in SamplePaths())
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        private string[] SamplePaths()
        {
            return new string[]
            {
            };
        }

        private (string Glob, string Regex)[] Patterns()
        {
            return new(string, string)[]
            {
                ("*.png", @".+\.png")
            };
        }

        private string[] GetGlobs() => Patterns().Select(x => x.Glob).ToArray();

        private string[] GetRegex() => Patterns().Select(x => x.Regex).ToArray();
    }
}