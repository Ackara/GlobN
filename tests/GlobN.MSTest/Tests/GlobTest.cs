using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;

namespace Acklann.GlobN.Tests
{
    [TestClass]
    public class GlobTest
    {
        [TestMethod]
        public void Can_determine_equality()
        {
            Glob A = "a";
            var a = new Glob("a");
            var b = new Glob("b");

            (A == a).ShouldBeTrue();
            (A == b).ShouldBeFalse();

            (A != a).ShouldBeFalse();
            (a != b).ShouldBeTrue();

            A.Equals(a).ShouldBeTrue();
            a.Equals(b).ShouldBeFalse();

            A.GetHashCode().ShouldBe(a.GetHashCode());
            a.GetHashCode().ShouldNotBe(b.GetHashCode());
        }

        [DataTestMethod]
        [DynamicData(nameof(GetTestCases), DynamicDataSourceType.Method)]
        public void Can_match_pattern_to_file_path(string filePath, string pattern, bool expected)
        {
            Glob.IsMatch(filePath, pattern).ShouldBe(expected);
        }

        private static IEnumerable<object[]> GetTestCases()
        {
            // ===== Null/Empty =====
            yield return new object[] { null, null, true };
            yield return new object[] { string.Empty, null, true };
            yield return new object[] { null, "foo.txt", false };
            yield return new object[] { string.Empty, "foo.txt", false };
            yield return new object[] { @"C:\files\foo.txt", null, true };
            yield return new object[] { @"C:\files\foo.txt", string.Empty, true };

            string win(string x) => string.Concat("C:", x.Replace('/', '\\'));
            var cases = new (string, string, bool)[]
            {
                // ===== Plan Text =====
                ("/user/deer.jpg", "deer.jpg", true)

                // ===== Wild-card (*) =====

                // ===== Directory-Wild-card (**) =====
            };

            foreach ((string path, string pattern, bool expected) in cases)
            {
                yield return new object[] { path, pattern, expected };
                yield return new object[] { win(path), pattern, expected };
            }
        }
    }
}