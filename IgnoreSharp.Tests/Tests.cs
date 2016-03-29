using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace IgnoreSharp.Tests
{
    [TestFixture]
    public class Tests
    {
        private string _basePath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _basePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Load_Rules_From_File()
        {
            var ignoreList = new IgnoreList(_basePath + @"\loadfromfile.gitignore");

            Assert.IsTrue(ignoreList.Rules.Count == 1);
        }

        [Test]
        public void Load_Rules_From_List()
        {
            var ignoreList = new IgnoreList(new List<string> { "*.cs" });

            Assert.IsTrue(ignoreList.Rules.Count == 1);
        }

        [Test]
        public void Ignore_File_By_Name()
        {
            var ignoreList = new IgnoreList(new List<string> { "README.txt" });

            Assert.IsTrue(ignoreList.IsMatch("readme.txt"));
            Assert.IsFalse(ignoreList.IsMatch("readme1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("readme1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/readme.txt"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/readme1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/readme.txtfile"));
        }

        [Test]
        public void Ignore_Dir_By_Name()
        {
            var ignoreList = new IgnoreList(new List<string> { "folder/" });

            Assert.IsTrue(ignoreList.IsMatch("folder"));
            Assert.IsFalse(ignoreList.IsMatch("folders"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/folder"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/folders"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/sub2/folder"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/sub2/folders"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/folder/sub2"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/folders/sub2"));
        }

        [Test]
        public void Ignore_Files_By_Extension()
        {
            var ignoreList = new IgnoreList(new List<string> { "*.txt" });

            Assert.IsTrue(ignoreList.IsMatch("test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("test1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("folder/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test1.txtfile"));
        }

        [Test]
        public void Ignore_Files_In_Specific_Dir_By_Extension()
        {
            var ignoreList = new IgnoreList(new List<string> { "folder/sub1/*.txt" });

            Assert.IsFalse(ignoreList.IsMatch("test1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("test1.txtfile"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test1.txtfile"));
        }

        [Test]
        public void Ignore_Files_In_Specific_Dir_By_Wildcard()
        {
            var ignoreList = new IgnoreList(new List<string> { "folder/sub1/*" });

            Assert.IsFalse(ignoreList.IsMatch("test1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("test1.txtfile"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test2.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test3.cs"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test1.txtfile"));
        }

        [Test]
        public void Exclude_Specific_File_From_Ignore()
        {
            var ignoreList = new IgnoreList(new List<string> { "*.txt", "!folder/test1.txt" });

            Assert.IsTrue(ignoreList.IsMatch("test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("test1.txtfile"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test1.txtfile"));
        }

        [Test]
        public void Exclude_File_From_Ignore_By_Extension()
        {
            var ignoreList = new IgnoreList(new List<string> { "*.txt", "!folder/*.txt" });

            Assert.IsTrue(ignoreList.IsMatch("test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("test1.txtfile"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/sub1/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test1.txtfile"));
        }

        [Test]
        public void Exclude_File_From_Ignore_By_Wildcard()
        {
            var ignoreList = new IgnoreList(new List<string> { "*.txt", "!folder/sub1/*" });

            Assert.IsTrue(ignoreList.IsMatch("test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("test1.txtfile"));
            Assert.IsTrue(ignoreList.IsMatch("folder/test1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("folder/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/test1.txtfile"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test2.txt"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test3.cs"));
            Assert.IsFalse(ignoreList.IsMatch("folder/sub1/test1.txtfile"));
        }

        [Test]
        public void Ignore_Dir_In_Specific_Dirs()
        {
            var ignoreList = new IgnoreList(new List<string> { "test/**/sub/" });

            Assert.IsFalse(ignoreList.IsMatch("test"));
            Assert.IsFalse(ignoreList.IsMatch("tests"));
            Assert.IsFalse(ignoreList.IsMatch("test/sub1"));
            Assert.IsFalse(ignoreList.IsMatch("test/sub1/subs"));
            Assert.IsTrue(ignoreList.IsMatch("test/sub1/sub"));
            Assert.IsFalse(ignoreList.IsMatch("test/sub2"));
            Assert.IsFalse(ignoreList.IsMatch("test/sub2/subs"));
            Assert.IsTrue(ignoreList.IsMatch("test/sub2/sub"));
            Assert.IsFalse(ignoreList.IsMatch("test/sub3"));
            Assert.IsFalse(ignoreList.IsMatch("test/sub3/subs"));
            Assert.IsTrue(ignoreList.IsMatch("test/sub3/sub"));
        }

        [Test]
        public void Ignore_After_Dynamic_Add()
        {
            var ignoreList = new IgnoreList(new List<string> { "README1.txt" });

            ignoreList.AddRule("README2.txt");
            ignoreList.AddRules(new List<string> { "README3.txt", "README4.txt" });

            Assert.IsTrue(ignoreList.IsMatch("README1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("README2.txt"));
            Assert.IsTrue(ignoreList.IsMatch("README3.txt"));
            Assert.IsTrue(ignoreList.IsMatch("README4.txt"));
        }

        [Test]
        public void Ignore_After_Dynamic_Remove()
        {
            var ignoreList = new IgnoreList(new List<string> { "README1.txt", "README2.txt", "README3.txt", "README4.txt" });

            ignoreList.RemoveRule("README2.txt");

            Assert.IsTrue(ignoreList.IsMatch("README1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("README2.txt"));
            Assert.IsTrue(ignoreList.IsMatch("README3.txt"));
            Assert.IsTrue(ignoreList.IsMatch("README4.txt"));
        }

        [Test]
        public void Add_Rules_From_File()
        {
            var ignoreList = new IgnoreList(new string[] { "README.txt" });

            ignoreList.AddRules(_basePath + @"\loadfromfile.gitignore");

            Assert.IsTrue(ignoreList.IsMatch("README.txt"));
            Assert.IsFalse(ignoreList.IsMatch("test.jpg"));
            Assert.IsTrue(ignoreList.IsMatch("test.cs"));
        }

        [Test]
        public void Clone()
        {
            var original = new IgnoreList(new string[] { "README1.txt", "README2.txt" });

            var clone = original.Clone();

            Assert.IsTrue(clone.IsMatch("README1.txt"));
            Assert.IsTrue(clone.IsMatch("README2.txt"));
            Assert.IsFalse(clone.IsMatch("README3.txt"));
        }

        [Test]
        public void Respect_Rule_Overrides()
        {
            var ignoreList = new IgnoreList(new string[] { "*.txt", "!sub1/*.txt", "sub1/README2.txt" });

            var matched = new List<IgnoreRule>();
            
            Assert.IsTrue(ignoreList.IsMatch("README1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/README1.txt"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/README2.txt"));
        }

        [Test]
        public void Log_Matched_Rules()
        {
            var ignoreList = new IgnoreList(new string[] { "*.txt", "!sub1/*.txt", "sub1/README2.txt" });

            var log = new List<string>();
            ignoreList.IsMatch("sub1/README2.txt", log);

            Assert.IsTrue(log.Count == 3);
            Assert.IsTrue(log[0] == "Ignored by ");
            Assert.IsFalse(log[1] == "Included by ");
            Assert.IsTrue(log[2] == "Ignored by ");
        }

        [TearDown]
        public void TearDown()
        {

        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {

        }
    }
}
