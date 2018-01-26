using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Acklann.GlobN.Tests
{
    [TestClass]
    public class GlobTest
    {
        [TestMethod]
        public void Can_determine_equality()
        {
            var s = "a";
            var a = new Glob("a");
            var b = new Glob("b");
            var c = new Glob("a");
            Glob n = null;

            (s == a).ShouldBeTrue();
            (a == c).ShouldBeTrue();
            (a != b).ShouldBeTrue();
            (a == b).ShouldBeFalse();
            (n == null).ShouldBeTrue();
            a.Equals(b).ShouldBeFalse();
            a.Equals("a").ShouldBeTrue();
            (a == null).ShouldBeFalse();
            a.GetHashCode().ShouldBe(c.GetHashCode());
        }

        // Plain Text <DefaultState>

        [DataTestMethod, TestCategory("Plain-Text")]
        [DataRow(@"C:\folder\file.txt", "")]
        [DataRow("C:/folder/file.txt", "file.txt")]
        [DataRow("C:/folder/file.txt", "folder/file.txt")]
        public void IsMatch_should_accept_a_plain_text_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod, TestCategory("Plain-Text")]
        [DataRow(@"C:/folder/file.txt", "folder")]
        [DataRow("/rroot/file.txt", "root/file.txt")]
        [DataRow(@"C:/folder/file01.txt", "file.txt")]
        [DataRow(@"C:/folder/big_file.txt", "file.txt")]
        public void IsMatch_should_reject_a_plain_text_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Dot Operators (.) <DefaultState>

        [DataTestMethod, TestCategory("Plain-Text")]
        [DataRow(@"C:/folder/file.txt", "./file.txt")]
        [DataRow(@"C:/folder/file.txt", "../file.txt")]
        [DataRow(@"C:/folder/file.txt", @"..\..\file.txt")]
        public void IsMatch_should_accept_a_dotted_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        // Negate Operators (!) <DefaultState>

        [DataTestMethod, TestCategory("Negate")]
        [DataRow(@"C:/folder/file.txt", "file.xml")]
        [DataRow(@"C:/folder/file.txt", "!file.txt")]
        public void IsMatch_should_accept_a_negate_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Wildcard (*) <WildcardState>

        [DataTestMethod, TestCategory("Wildcard '*'")]
        [DataRow(@"C:\folder\file.txt", "*")]
        [DataRow(@"C:\folder\file.txt", "*.*")]
        [DataRow(@"C:\folder\file.txt", "*/*.*")]
        [DataRow(@"C:\folder\file.txt", "*.txt")]
        [DataRow(@"C:\folder\file.txt", "file.*")]
        [DataRow(@"/folder/file.txt", "*/file.txt")]
        [DataRow(@"C:\folder\file.txt", "file*.txt")]
        [DataRow(@"C:\folder\file01.txt", "file*.txt")]
        [DataRow(@"/folder/file.txt", "*/f*i*l*e*tx*")]
        [DataRow(@"C:\folder\file01.txt", "*file*.txt")]
        [DataRow(@"C:\root\folder\file.txt", "*/*/*.*")]
        [DataRow(@"C:\folder\sub\file.txt", "folder/*/*.*")]
        [DataRow("/folder/sub/file.txt", "folder/*/file.txt")]
        [DataRow(@"C:\folder\sub\file.txt", "folder/*u*/file.txt")]
        [DataRow(@"C:\folder\sub\file.txt", @"C:\*folder\sub\file.txt")]
        public void IsMatch_should_accept_a_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod, TestCategory("Wildcard '*'")]
        [DataRow(@"/file.txt", "*/*.*")]
        [DataRow(@"/file.txt", "*/file.txt")]
        [DataRow(@"\root\aaa\file.txt", "**")]
        [DataRow(@"C:/file.txt", "*/file.txt")]
        [DataRow(@"C:\folder\file.txt", "*/*/*.*")]
        [DataRow(@"C:\folder\image.txt", "img*.txt")]
        [DataRow(@"\root\aaa\file.txt", "roo*/file.txt")]
        [DataRow(@"\root\aaa\file.txt", "ro**/file.txt")]
        [DataRow("/folder/sub/file.txt", "forlder/sub/fi**le.txt")]
        public void IsMatch_should_reject_a_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Directory-Wildcard (**) <DirectoryWildcardState>

        [DataTestMethod, TestCategory("Directory-Wildcard '**'")]
        [DataRow(@"/file.txt", "**/file.txt")]
        [DataRow(@"C:/file.txt", "**/file.txt")]
        [DataRow(@"/file.txt", "**/**/file.txt")]
        [DataRow(@"\root\folder\sub\file.txt", "root/**/file.txt")]
        [DataRow(@"\root\folder\sub\context\file.txt", "root/**/sub/**/file.txt")]
        public void IsMatch_should_accept_a_directory_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod, TestCategory("Directory-Wildcard '**'")]
        [DataRow("/folder/sub/file.txt", "folder/***/file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", "folder/**/**")]
        [DataRow(@"C:\root\folder\sub\file.txt", "foot/**/file.**")]
        [DataRow(@"C:\root\folder\sub\file.txt", "foot/**/file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", "foot/***/file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", "foot/**/**/file.txt")]
        public void IsMatch_should_reject_a_directory_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Character-Wildcard (?) <CharacterWildcardState>

        [DataTestMethod, TestCategory("Character-Wildcard '?'")]
        [DataRow(@"C:/folder/file.txt", "file.???")]
        [DataRow(@"C:/folder/file1.txt", "file?.txt")]
        public void IsMatch_should_accept_a_character_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod, TestCategory("Character-Wildcard '?'")]
        [DataRow(@"C:/folder/file.txt", "file?.txt")]
        [DataRow(@"C:/folder/file.jpeg", "file?.???")]
        public void IsMatch_should_reject_a_character_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Wild-card Combinations <Multiple-States>

        [DataTestMethod, TestCategory("Combinations")]
        [DataRow(@"C:\root\folder\file.txt", "folder/")]
        [DataRow(@"C:\root\folder\sub\file.txt", "folder/")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"root\*\**\file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"root\**\*\file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"root\f*\**\file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"r?o*\*f*\**\file.???")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"roo*\folde?\**\file.txt")]
        public void IsMatch_should_accept_a_wildcard_combinations(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod, TestCategory("Combinations")]
        [DataRow(@"C:\root\file.txt", "root/**/*/file.txt")]
        [DataRow(@"C:\root\app_folder\sub\file.txt", "folder\\")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"sub\*\**\file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"sub\**\*\file.txt")]
        [DataRow(@"C:\root\bolder\sub\file.txt", @"root\f*\**\file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"roo*\foldg?\**\file.txt")]
        [DataRow(@"C:\Root\Folder\sub\file.txt", @"!roo*\folde?\**\file.txt")]
        public void IsMatch_should_reject_a_wildcard_combinations(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        private static void RunIsMatchTest(string pattern, string samplePath, bool shouldBe = true)
        {
            var result = Glob.IsMatch(samplePath, pattern);
            var failureMsg = $"'{pattern}' {(shouldBe ? "SHOULD OF MATCHED" : "SHOULD NOT OF MATCHED")} '{samplePath}'";

            if (shouldBe == true) result.ShouldBeTrue(failureMsg); else result.ShouldBeFalse(failureMsg);
        }
    }
}