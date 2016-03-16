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

            Assert.IsFalse(ignoreList.IsMatch("test.jpg"));
            Assert.IsTrue(ignoreList.IsMatch("test.cs"));
        }

        [Test]
        public void Load_Rules_From_List()
        {
            var ignoreList = new IgnoreList(new List<string> { "*.cs" });

            Assert.IsFalse(ignoreList.IsMatch("test.jpg"));
            Assert.IsTrue(ignoreList.IsMatch("test.cs"));
        }

        [Test]
        public void Ignore_File_Or_Dir_By_Name()
        {
            var ignoreList = new IgnoreList(new List<string> { "README" });
            
            Assert.IsTrue(ignoreList.IsMatch("README"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/README"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/sub2/README"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/README/sub2"));
            Assert.IsTrue(ignoreList.IsMatch("README/test.txt"));
            Assert.IsTrue(ignoreList.IsMatch("readme"));
        }

        [Test]
        public void Ignore_File_By_Name()
        {
            var ignoreList = new IgnoreList(new List<string> { "README.txt" });

            Assert.IsTrue(ignoreList.IsMatch("README.txt"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/README.txt"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/sub2/README.txt"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/README/sub2"));
            Assert.IsFalse(ignoreList.IsMatch("README/test.txt"));
            Assert.IsTrue(ignoreList.IsMatch("readme.txt"));
        }

        [Test]
        public void Ignore_Dir_By_Name()
        {
            var ignoreList = new IgnoreList(new List<string> { "README/" });

            Assert.IsFalse(ignoreList.IsMatch("README"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/README"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/sub2/README"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/README/sub2"));
            Assert.IsTrue(ignoreList.IsMatch("README/test.txt"));
            Assert.IsFalse(ignoreList.IsMatch("readme"));
        }

        [Test]
        public void Ignore_File_By_Wildcard()
        {
            var ignoreList = new IgnoreList(new List<string> { "*.txt", "!sub1/README.txt" });

            Assert.IsTrue(ignoreList.IsMatch("README.txt"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/README.txt"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/sub2/README.txt"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/README/sub2"));
            Assert.IsTrue(ignoreList.IsMatch("README/test.txt"));
            Assert.IsTrue(ignoreList.IsMatch("readme.txt"));
        }

        [Test]
        public void Ignore_Files_In_Dir_By_Wildcard()
        {
            var ignoreList = new IgnoreList(new List<string> { "sub1/sub2/*.txt" });

            Assert.IsFalse(ignoreList.IsMatch("README1.txt"));
            Assert.IsFalse(ignoreList.IsMatch("README2.txt"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/sub2/README3.txt"));
            Assert.IsTrue(ignoreList.IsMatch("sub1/sub2/README4.txt"));
            Assert.IsFalse(ignoreList.IsMatch("sub1/sub2/README5.md"));
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
