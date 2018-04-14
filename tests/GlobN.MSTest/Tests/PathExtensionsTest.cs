using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.IO;

namespace Acklann.GlobN.Tests
{
    [TestClass]
    public class PathExtensionsTest
    {
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

        [DataTestMethod]
        [DataRow(false, "", "")]
        [DataRow(false, null, null)]
        [DataRow(false, "", @"C:\temp\file.txt")]
        [DataRow(false, "/root/temp", "/root/temp")]
        [DataRow(false, @"\temp\content", @"\file.txt")]
        [DataRow(true, @"C:\temp", @"C:\temp\file.txt")]
        [DataRow(false, @"C:\temp\content", @"C:/temp/file.txt")]
        [DataRow(false, @"/user/docs/file.txt", @"/temp/file.txt")]
        [DataRow(true, @"C:\temp\content", @"C:\TeMp/content\file.txt")]
        public void IsChildOf_should_determine_if_path_is_subFolder(bool expected, string root, string path)
        {
            path.IsChildOf(root).ShouldBe(expected, $"root:'{root}' | path:'{path}'");
        }

        [DataTestMethod]
        [DataRow("", "", ""),]
        [DataRow(null, null, null)]
        [DataRow(@"/root/temp/file.txt", "", @"/root/temp/file.txt")]
        [DataRow(@"/root/temp/file.txt", null, @"/root/temp/file.txt")]
        /* *** */
        [DataRow(@"/root/temp/file.txt",
                 @"/root/temp/", 
                 @"file.txt")]
        [DataRow(@"\root\temp\lvl\file.txt",
                 @"\root\temp\",
                 @"lvl\file.txt")]
        [DataRow(@"C:\temp\file.txt",
                 @"D:\files",
                 @"C:\temp\file.txt")]
        [DataRow(@"C:\projects\coolapp\tests\mstest",
                 @"C:\projects\coolapp\src\bin",
                               @"..\..\tests\mstest")]
        public void GetRelativePath_should_return_a_relative_path_derived_from_a_base_path(string sample, string baseDir, string expect)
        {
            sample.GetRelativePath(baseDir).ShouldBe(expect);
        }
    }
}