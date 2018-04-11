using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Acklann.GlobN.Tests
{
    [TestClass]
    public class ExtensionMethodTest
    {
        [TestMethod]
        public void ReplacePath_should_replace_old_values()
        {
            // Arrange
            var cases = new(string, string, string, string)[]
            {
                ("ssiteee", "site", "", "see"),
                //(@"\site\content\img\bg.jpg", "content/img", "assets/images", @"\site\assets/images\bg.jpg"),
                //("/site/content/img/bg.jpg", "content/img", "assets/images", "/site/assets/images/bg.jpg"),
                //("/site/content/img/bg.jpg", "img", "images", "/site/content/images/bg.jpg"),
                //("/site/content/img/bg.jpg", @"\site\", "/wwwroot/", "/wwwroot/content/img/bg.jpg"),
                //(@"\site\content\img\bg.jpg", "content/img", "", @"\site\assets/images\bg.jpg"),
            };

            // Act & Assert
            foreach (var (value, oldValue, newValule, expected) in cases)
            {
                
            }
        }

        [TestMethod]
        public void ReplacePath_should_throw_exception_when_value_is_null()
        {
            Should.Throw<ArgumentException>(() => { "/site/content/bg.jpg".ReplacePath("", "bg.png"); });
            Should.Throw<ArgumentException>(() => { "/site/content/bg.jpg".ReplacePath(null, "bg.png"); });
        }

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
        public void SplitPath_should_return_a_segment_of_a_path()
        {
            string err = null;
            var samplePath = Path.Combine("C:\\", "app", "cool_app", "settings", "user.config");

            "".SplitPath(2).ShouldBeEmpty();
            samplePath.SplitPath(0).ShouldBeEmpty();
            samplePath.SplitPath(100).ShouldBe(samplePath);
            samplePath.SplitPath(2).ShouldBe(@"\settings\user.config");
            Should.Throw<ArgumentNullException>(() => { err.SplitPath(2); });
        }

        [TestMethod]
        public void MoveUpDirectory_should_return_a_path_parent_directory()
        {
            string err = null;
            var sample = @"C:\apps\coolapp\settings";

            sample.MoveUpDirectory(2).ShouldBe(@"C:\apps");
            sample.MoveUpDirectory(100).ShouldBeNullOrEmpty();
            Should.Throw<ArgumentException>(() => { err.MoveUpDirectory(2); });
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

        [DataTestMethod]
        [DataRow(false, "", "")]
        [DataRow(false, null, null)]
        [DataRow(false, "", @"C:\temp\file.txt")]
        [DataRow(false, @"\temp\content", @"\file.txt")]
        [DataRow(true, @"C:\temp", @"C:\temp\file.txt")]
        [DataRow(false, @"C:\temp\content", @"C:/temp/file.txt")]
        [DataRow(false, @"/user/docs/file.txt", @"/temp/file.txt")]
        [DataRow(true, @"C:\temp\content", @"C:\TeMp/content\file.txt")]
        public void IsChildOf_should_determine_if_path_is_subFolder(bool expected, string root, string path)
        {
            path.IsChildOf(root).ShouldBe(expected, $"root:'{root}' | path:'{path}'");
        }

        //[TestMethod]
        public void WriteToFile_should_create_a_new_file()
        {
            //string path = Path.Combine(Path.GetTempPath(), "delete_me", "remove_me.txt");
            //var sample = "she sell sea shells by the seashore";

            //path.WriteToFile(sample);

            //File.ReadAllText(path).ShouldBe(sample);
        }
    }
}