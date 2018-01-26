using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using BenchmarkDotNet.Order;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using G = Glob;

namespace Acklann.GlobN.Benchmark
{
    [OrderProvider(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn(BenchmarkDotNet.Mathematics.NumeralSystem.Roman)]
    public class GlobBenchmark
    {
        public GlobBenchmark()
        {
            var list = new Stack<string>();
            using (var stream = File.OpenRead("fileList.txt"))
            {
                using (var reader = new StreamReader(stream))
                {
                    string path;
                    while (!reader.EndOfStream)
                    {
                        path = reader.ReadLine().Trim();
                        if (!string.IsNullOrEmpty(path)) list.Push(path);
                    }
                }
            }

            FileList = list.ToArray();
            Globs = Patterns().Select(x => x.Glob).ToArray();
            RegexExp = Patterns().Select(x => x.Regex).ToArray();
        }

        public string[] FileList, Globs, RegexExp;

        /* Case-Insensitive */
        [Benchmark]
        public int GlobN()
        {
            int matches = 0;
            foreach (var pattern in Globs)
            {
                var sut = new GlobN.Glob(pattern);
                foreach (var path in FileList)
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        /* Case-Sensitive */
        [Benchmark(Description = "DotNet.Glob")]
        public int DotNetGlob()
        {
            int matches = 0;
            foreach (var pattern in Globs)
            {
                var sut = DotNet.Globbing.Glob.Parse(pattern);
                foreach (var path in FileList)
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        [Benchmark(Description = "Glob")]
        public int GlobGlob()
        {
            int matches = 0;
            foreach (var pattern in Globs)
            {
                var sut = new G.Glob(pattern, G.GlobOptions.Compiled);
                foreach (var path in FileList)
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
            foreach (var pattern in RegexExp)
            {
                var sut = new Regex(pattern, RegexOptions.Compiled);
                foreach (var path in FileList)
                {
                    if (sut.IsMatch(path)) matches++;
                }
            }

            return matches;
        }

        private static (string Glob, string Regex)[] Patterns()
        {
            return new(string, string)[]
            {
                ("**/*", ".+"),
                ("**/*.png", @".+\.png"),
                ("**/purus/**/*", "purus/.+"),
                ("/sed/**/*/felis.html", @"sed/.+/felis\.html"),
                ("nullam/sit/amet/turpis/elementum/ligula/vehicula.jsp", "nullam/sit/amet/turpis/elementum/ligula/vehicula.jsp")
            };
        }
    }
}