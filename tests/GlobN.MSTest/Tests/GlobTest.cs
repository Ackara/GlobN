using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Acklann.GlobN.Tests
{
    [TestClass]
    public class GlobTest
    {
        // Plain Text <Default State>

        [DataTestMethod]
        [DataRow(@"C:\folder\file.txt", "")]
        [DataRow("C:/folder/file.txt", "file.txt")]
        [DataRow("C:/folder/file.txt", "folder/file.txt")]
        [DataRow("C:/folder/file.txt", "C:\\folder/file.txt")]
        public void IsMatch_should_accept_a_plain_text_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod]
        [DataRow(@"C:/folder/file.txt", "folder")]
        [DataRow(@"C:/folder/file01.txt", "file.txt")]
        [DataRow(@"C:/folder/big_file.txt", "file.txt")]
        [DataRow("/rroot/file.txt", "root/file.txt")]
        public void IsMatch_should_reject_a_plain_text_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Dot (.) <Default State>

        [DataTestMethod]
        [DataRow(@"C:/folder/file.txt", "./file.txt")]
        [DataRow(@"C:/folder/file.txt", "../file.txt")]
        [DataRow(@"C:/folder/file.txt", @"..\..\file.txt")]
        public void IsMatch_should_accept_a_dotted_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        // Asterisk (*) <WildcardState>

        [DataTestMethod]
        [DataRow(@"C:\folder\file.txt", "*")]
        [DataRow(@"C:\folder\file.txt", "*.txt")]
        [DataRow(@"C:\folder\file.txt", "file.*")]
        [DataRow(@"/folder/file.txt", "*/file.txt")]
        [DataRow(@"C:\folder\file01.txt", "file*.txt")]
        [DataRow(@"/folder/file.txt", "*/f*i*l*e*tx*")]
        [DataRow(@"C:\folder\sub\file.txt", "folder/*/*.*")]
        [DataRow("/folder/sub/file.txt", "folder/*/file.txt")]
        [DataRow(@"C:\folder\sub\file.txt", "folder/*u*/file.txt")]
        public void IsMatch_should_accept_a_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod]
        [DataRow(@"/file.txt", "*/file.txt")]
        [DataRow(@"C:\folder\image.txt", "img*.txt")]
        public void IsMatch_should_reject_a_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Double-Asterisk (**) <DirectoryWildcardState>

        [DataTestMethod]
        [DataRow(@"C:/file.txt", "**/file.txt")]
        [DataRow("ab/sub/file.txt", "**/sub/file.txt")]
        [DataRow(@"C:\root\sub\file.txt", "**/file.txt")]
        [DataRow(@"\root\root\file.txt", "root/**/file.txt")]
        [DataRow(@"C:\root\sub\root\file.txt", "root/**/**/file.txt")]
        public void IsMatch_should_accept_a_directory_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod]
        [DataRow(@"/file.txt", "**/file.txt")]
        [DataRow("/root/file.txt", "root/**/**/file.txt")]
        [DataRow(@"C:\root\file.txt", "root/**/file.txt")]
        [DataRow(@"\root\root\file.txt", "root/**/**/file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", "foot/**/file.txt")]
        public void IsMatch_should_reject_a_directory_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Question-Mark <CharacterWildcardState>

        [DataTestMethod]
        [DataRow(@"C:/folder/file.txt", "file1?.txt")]
        [DataRow(@"C:/folder/file1.txt", "file1?.txt")]
        [DataRow(@"C:/root/folder/f.txt", "fi?l?e?1?.txt")]
        public void IsMatch_should_accept_a_character_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod]
        [DataRow(@"C:/folder/file2.txt", "file1?.txt")]
        public void IsMatch_should_reject_a_character_wildcard_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Character-Set [abc]

        [DataTestMethod]
        [DataRow(@"C:\root\folder\sub\fileA.txt", "file[AaBc].txt")]
        [DataRow(@"C:\root\folder\f3\file.txt", "f[135]/file.txt")]
        public void IsMatch_should_accept_a_charset_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod]
        [DataRow(@"C:\root\folder\sub\fileA.txt", "file[abc].txt")]
        [DataRow(@"C:\root\folder\f3\file.txt", "f[246]/file.txt")]
        public void IsMatch_should_reject_a_charset_pattern(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Wild-card Combinations <Multiple-States>

        [DataTestMethod]
        //[DataRow(@"C:\root\folder\sub\file.txt", "folder/")]
        //[DataRow(@"C:\root\folder\sub\file.txt", "folder/**/*")]
        //[DataRow(@"C:\root\folder\sub\file.txt", @"root\*\**\file.txt")]
        //[DataRow(@"C:\root\folder\sub\file.txt", @"root\f*\**\file.txt")]
        [DataRow(@"C:\root\folder\sub\file.txt", @"roo*\*f*\**\file.txt")]
        public void IsMatch_should_accept_a_wildcard_combinations(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: true);
        }

        [DataTestMethod]
        //[DataRow(@"C:\root\app_folder\sub\file.txt", "folder\\")]
        //[DataRow(@"C:\root\folder\file.txt", @"root\*\**\file.txt")]
        //[DataRow(@"C:\root\bolder\sub\file.txt", @"root\f*\**\file.txt")]
        public void IsMatch_should_reject_a_wildcard_combinations(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        // Exceptions (Illegal Patterns)

        [DataTestMethod]
        [DataRow(@"C:\root\folder\sub\file.txt", "folder/**/**")]
        [DataRow("/folder/sub/file.txt", "forlder/***/file.txt")]
        [DataRow("/folder/sub/file.txt", "forlder/sub/fi**le.txt")]
        public void IsMatch_should_reject_illegal_patterns(string filePath, string pattern)
        {
            RunIsMatchTest(pattern, filePath, shouldBe: false);
        }

        private static void RunIsMatchTest(string pattern, string samplePath, bool shouldBe = true)
        {
            var glob = new Glob(pattern);
            var result = glob.IsMatch(samplePath);
            var failureMsg = $"'{pattern}' {(shouldBe ? "SHOULD OF MATCHED" : "SHOULD NOT OF MATCHED")} '{samplePath}'";

            if (shouldBe == true) result.ShouldBeTrue(failureMsg); else result.ShouldBeFalse(failureMsg);
        }
    }
}