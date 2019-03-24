using BenchmarkDotNet.Attributes;
using System.Linq;
using System.Text.RegularExpressions;
using GE = GlobExpressions;

namespace Acklann.GlobN
{
    [MemoryDiagnoser]
    [RankColumn(BenchmarkDotNet.Mathematics.NumeralSystem.Arabic)]
    public class ComparisonTest
    {
        private readonly string[] _fileList = Mock.GetFileList().Take(100).ToArray();
        private readonly string[] _regex = new string[] { ".+", @".+\.png", "purus/.+", @"sed/.+/felis\.html", "nullam/sit/amet/turpis/elementum/ligula/vehicula.jsp" };
        private readonly string[] _glob = new string[] { "**/*", "**/*.png", "**/purus/**/*", "/sed/**/*/felis.html", "nullam/sit/amet/turpis/elementum/ligula/vehicula.jsp" };

        [Benchmark(Baseline = true)]
        public int Regex()
        {
            int matches = 0;
            foreach (var pattern in _regex)
            {
                var sut = new Regex(pattern, RegexOptions.Compiled);
                foreach (var path in _fileList)
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        [Benchmark]
        public int GlobN()
        {
            int matches = 0;
            foreach (var pattern in _glob)
            {
                var sut = new GlobN.Glob(pattern);
                foreach (var path in _fileList)
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        [Benchmark(Description = "DotNet.Glob")]
        public int DotNetGlob()
        {
            int matches = 0;
            foreach (var pattern in _glob)
            {
                var sut = DotNet.Globbing.Glob.Parse(pattern);

                foreach (var path in _fileList)
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        [Benchmark]
        public int GlobExpressions()
        {
            int matches = 0;
            foreach (var pattern in _glob)
            {
                var sut = new GE.Glob(pattern, GE.GlobOptions.Compiled);
                foreach (var path in _fileList)
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }
    }
}