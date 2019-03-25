using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Acklann.GlobN.Tests
{
    [TestClass]
    public class GlobExtensionsTest
    {
        [TestMethod]
        public void Should_expand_a_pattern_to_its_absolute_path()
        {
            // Arrange
            Glob empty = null;
            Glob sampleFile = Path.GetTempFileName();
            Glob root = Path.Combine("C:\\", "websites", "coolapp.com", "src", "wwwroot");

            // Act
            var case1 = ((Glob)@"..\Views").Expand(root);
            var case2 = new Glob("../../index.html").Expand(root);
            var case3 = ((Glob)"..\\file.tmp").Expand(@"%TEMP%\foo", true);
            var case4 = ((Glob)"../").Expand(@"%TEMP%\foo", expandVariables: false);
            var case5 = empty.Expand(root);
            var case6 = sampleFile.Expand(root);

            // Assert
            case1.ShouldEndWith(@"src\Views");
            case2.ShouldEndWith("coolapp.com\\index.html");
            case3.ShouldContain(Environment.ExpandEnvironmentVariables("%TEMP%"));
            case4.ShouldBe("%TEMP%");
            case5.ShouldBe(root);
            case6.ShouldBe(sampleFile);
        }

        [TestMethod]
        public void Should_resolve_paths_that_match_pattern_within_a_folder()
        {
            // Arrange
            Glob err = null;
            var sampleFile = new FileInfo(Path.GetTempFileName());
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Act
            var case1 = ((Glob)"*.dll").ResolvePaths(directory).ToArray();
            var case2 = ((Glob)"../*").ResolvePaths(directory).ToArray();
            var case3 = new Glob(sampleFile.Name).ResolvePaths("%TEMP%", expandVariables: true).ToArray();
            var case4 = ((Glob)sampleFile.FullName).ResolvePaths(directory).ToArray();
            var case5 = ((Glob)$"./TestData/*.txt").ResolvePaths(directory).ToArray();
            var case6 = ((Glob)$"{directory}\\*deps*").ResolvePaths(directory, SearchOption.TopDirectoryOnly).ToArray();
            var case7 = ((Glob)@"TestData\sample.txt").ResolvePaths(Path.GetDirectoryName(directory), SearchOption.AllDirectories).ToArray();

            if (sampleFile.Exists) sampleFile.Delete();

            // Assert
            case1.ShouldNotBeEmpty();
            case1.ShouldAllBe(x => x.EndsWith(".dll"));

            case2.ShouldNotBeEmpty();

            case3.ShouldContain(sampleFile.FullName);

            case4.Length.ShouldBe(1);
            case4[0].ShouldBe(sampleFile.FullName);
            case5.ShouldNotBeEmpty();
            case6.Length.ShouldBe(1);
            case7.Length.ShouldBe(1);

            Should.Throw<ArgumentNullException>(() => { err.ResolvePaths().ToArray(); });
            Should.Throw<DirectoryNotFoundException>(() => { new Glob(sampleFile.Name).ResolvePaths("%TEMP%", expandVariables: false).ToArray(); });
        }

        [TestMethod]
        public void Should_resolve_files_that_match_pattern_within_a_folder()
        {
            // Arrange
            var sampleFile = new FileInfo(Path.GetTempFileName());
            var directory = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));

            // Act
            var set1 = directory.ResolvePaths("*.dll").ToArray();

            // Assert
            set1.ShouldNotBeEmpty();
            set1.ShouldAllBe(x => x.FullName.EndsWith(".dll"));
        }

        [TestMethod]
        public void Should_filter_paths_within_a_list()
        {
            // Arrange
            var sample = new string[]
            {
                "/root/file1.xml",
                "/root/file2.xml",
                "/root/file3.json",
                "/root/file4.json"
            };

            // Act
            var set1 = sample.Filter("*.xml").ToArray();
            var set2 = sample.Filter(new Glob("*.xml"), negate: true).ToArray();

            // Assert
            set1.Length.ShouldBe(2);
            set1.ShouldAllBe(x => x.EndsWith(".xml"));

            set2.Length.ShouldBe(2);
            set2.ShouldAllBe(x => x.EndsWith(".json"));
        }
    }
}