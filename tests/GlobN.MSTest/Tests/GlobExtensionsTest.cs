﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
        public void ExpandPath_should_convert_a_pattern_to_absolute_path()
        {
            // Arrange
            string empty = null;
            string sampleFile = Path.GetTempFileName();
            string root = Path.Combine("C:\\", "websites", "coolapp.com", "src", "wwwroot");

            // Act
            var result1 = @"..\Views".ExpandPath(root);
            var result2 = new Glob("../../index.html").ExpandPath(root);
            var result3 = "..\\file.tmp".ExpandPath(@"%TEMP%\foo");
            var result4 = "../".ExpandPath(@"%TEMP%\foo", expandVariables: false);
            var result5 = empty.ExpandPath(root);
            var result6 = sampleFile.ExpandPath(root);

            // Assert
            result1.ShouldEndWith(@"src\Views");
            result2.ShouldEndWith("coolapp.com\\index.html");
            result3.ShouldContain(Environment.ExpandEnvironmentVariables("%TEMP%"));
            result4.ShouldBe("%TEMP%");
            result5.ShouldBe(root);
            result6.ShouldBe(sampleFile);
        }

        [TestMethod]
        public void ResolvePath_should_return_a_file_list_that_match_the_pattern()
        {
            // Arrange
            Glob err = null;
            var sampleFile = new FileInfo(Path.GetTempFileName());
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Act
            var set1 = "*.dll".ResolvePath(directory).ToArray();
            var set2 = "../../../Tests/*.cs".ResolvePath(directory).ToArray();
            var set3 = new Glob(sampleFile.Name).ResolvePath("%TEMP%").ToArray();
            var set4 = sampleFile.FullName.ResolvePath(directory).ToArray();

            if (sampleFile.Exists) sampleFile.Delete();

            // Assert
            set1.ShouldNotBeEmpty();
            set1.ShouldAllBe(x => x.EndsWith(".dll"));

            set2.ShouldNotBeEmpty();
            set2.ShouldAllBe(x => x.EndsWith(".cs"));

            set3.ShouldContain(sampleFile.FullName);

            set4.Length.ShouldBe(1);
            set4[0].ShouldBe(sampleFile.FullName);

            Should.Throw<ArgumentNullException>(() => { err.ResolvePath().ToArray(); });
            Should.Throw<DirectoryNotFoundException>(() => { new Glob(sampleFile.Name).ResolvePath("%TEMP%", expandVariables: false).ToArray(); });
        }

        [TestMethod]
        public void GetFiles_should_return_a_file_list_that_match_the_pattern()
        {
            // Arrange
            var sampleFile = new FileInfo(Path.GetTempFileName());
            var directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            // Act
            var set1 = directory.GetFiles("*.dll").ToArray();
            var set2 = directory.GetFiles("../../../Tests/*.cs").ToArray();
            var set3 = "%TEMP%".GetFiles(new Glob(sampleFile.Name)).ToArray();

            // Assert

            set1.ShouldNotBeEmpty();
            set1.ShouldAllBe(x => x.EndsWith(".dll"));

            set2.ShouldNotBeEmpty();
            set2.ShouldAllBe(x => x.EndsWith(".cs"));

            set3.ShouldContain(sampleFile.FullName);

            Should.Throw<DirectoryNotFoundException>(() => { "%TEMP%".GetFiles("*.tmp", expandVariables: false).ToArray(); });
        }

        [TestMethod]
        public void Filter_should_extract_paths_from_a_list()
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