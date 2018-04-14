using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Acklann.GlobN.Benchmark
{
    internal class Program
    {
        internal static readonly string[] Headers =
            { "Mean", "StdDev", "Scaled" };

        private static readonly Regex _digit = new Regex(@"[0-9\.]+", RegexOptions.Compiled);

        private static void Main(string[] args)
        {
            //RunTest();

            var app = new BenchmarkDotNet.Running.BenchmarkSwitcher(GetBenchmarks());
            var results = app.Run(new[] { "jobs=dry", $"class={nameof(GlobBenchmark)}" });
            AppendResultsToTimeline(results, "../../../../timeline.csv");
        }

        private static Type[] GetBenchmarks()
        {
            var assemblyTypes = (from t in typeof(Program).Assembly.ExportedTypes
                                 where !t.IsAbstract && !t.IsInterface
                                 select t);

            var results = new Stack<Type>();
            foreach (Type type in assemblyTypes)
            {
                foreach (var item in type.GetRuntimeMethods())
                {
                    if (item.IsDefined(typeof(BenchmarkAttribute)))
                    {
                        results.Push(type);
                        break;
                    }
                }
            }

            return results.ToArray();
        }

        private static void AppendResultsToTimeline(IEnumerable<Summary> results, string timelinePath)
        {
            var newData = new LinkedList<JObject>();
            foreach (Summary summary in results)
            {
                ConvertToJson(summary, newData);
            }
            MergeDocument(timelinePath.ExpandPath(AppContext.BaseDirectory), newData);
        }

        private static void ConvertToJson(Summary summary, ICollection<JObject> data)
        {
            int nColumns = summary.Table.ColumnCount;
            int nRows = summary.Benchmarks.Length;
            string header, value;

            for (int x = 0; x < nRows; x++)
            {
                var obj = new JObject(
                    new JProperty("Date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
                    new JProperty("Namespace", summary.Title),
                    new JProperty("Method", summary.Benchmarks[x].DisplayInfo));
                data.Add(obj);

                for (int y = 0; y < nColumns; y++)
                {
                    header = summary.Table.FullHeader[y];
                    if (WantedColumn(header))
                    {
                        value = _digit.Match(summary.Table.FullContent[x][y]).Value;
                        obj.Add(new JProperty(header, value));
                    }
                }
            }
        }

        private static void MergeDocument(string filePath, IEnumerable<JObject> newData)
        {
            string dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new StreamWriter(stream))
                {
                    if (stream.Length == 0)
                    {
                        JObject first = newData.First();
                        writer.WriteLine(string.Join(",", (first.Properties()).Select((x) => $"\"{x.Name}\"")));
                    }

                    foreach (var record in newData)
                    {
                        writer.WriteLine(string.Join(",", record.PropertyValues().Select((x) => $"\"{x.Value<string>()}\"")));
                    }
                    writer.Flush();
                }
            }
        }

        private static bool WantedColumn(string header)
        {
            foreach (string value in Headers)
            {
                if (string.Equals(header, value, StringComparison.InvariantCultureIgnoreCase)) return true;
            }

            return false;
        }

        private static void RunTest()
        {
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
            System.Environment.Exit(0);
        }
    }
}