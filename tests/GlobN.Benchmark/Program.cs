using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
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
        private static readonly Header[] Headers = new[]
        {
            new Header("Method"),
            new Header("Mean", units: "ms"),
            new Header("StdDev", units: "ms"),
            new Header("Scaled", units: "ms"),
            new Header("Allocated", units: "KB")
        };

        private static void Main(string[] args)
        {
            //RunTest();

            var app = new BenchmarkSwitcher(GetBenchmarks());
            IEnumerable<Summary> results = app.Run(args);
            BuildTimeline(results, @"..\..\..\..\timeline.csv");
        }

        private static void BuildTimeline(IEnumerable<Summary> results, string timelinePath)
        {
            IEnumerable<JObject> newData = ConvertToJson(results);
            MergeDocument(timelinePath.ExpandPath(AppContext.BaseDirectory), newData);
        }

        private static IEnumerable<JObject> ConvertToJson(IEnumerable<Summary> results)
        {
            var numeric = new Regex(@"^[-+]?[0-9]*\.?[0-9]+\b", RegexOptions.Compiled);

            foreach (Summary summary in results)
            {
                string name, value;
                int nRows = summary.Benchmarks.Length;
                int nColumns = summary.Table.ColumnCount;

                for (int x = 0; x < nRows; x++)
                {
                    var obj = new JObject(
                        new JProperty("Date", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")),
                        new JProperty("Class", summary.Title));

                    for (int y = 0; y < nColumns; y++)
                    {
                        name = summary.Table.FullHeader[y];
                        if (wanted(ref name))
                        {
                            value = summary.Table.FullContent[x][y];
                            Match match = numeric.Match(value);

                            obj.Add(new JProperty(name, (match.Success ? match.Value : value)));
                        }
                    }

                    yield return obj;
                }
            }

            bool wanted(ref string value)
            {
                foreach (Header header in Headers)
                {
                    if (string.Equals(value, header.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        value = header.ToString();
                        return true;
                    }
                }
                return false;
            }
        }

        private static void MergeDocument(string timelinePath, IEnumerable<JObject> newData)
        {
            string dir = Path.GetDirectoryName(timelinePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            using (var stream = new FileStream(timelinePath, FileMode.Append, FileAccess.Write, FileShare.Read))
            {
                using (var writer = new StreamWriter(stream))
                {
                    if (stream.Length == 0)
                    {
                        JObject first = newData.FirstOrDefault();
                        if (first == null) return;
                        else writer.WriteLine(string.Join(",", (first.Properties()).Select((x) => $"\"{x.Name}\"")));
                    }

                    foreach (JObject record in newData)
                    {
                        writer.WriteLine(string.Join(",", record.PropertyValues().Select((x) => $"\"{x.Value<string>()}\"")));
                    }
                    writer.Flush();
                }
            }
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

        private static void RunTest()
        {
            var instance = new GlobComparisons();
            foreach (var method in (from m in typeof(GlobComparisons).GetMethods()
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