using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgnoreSharp.Tests
{
    [TestFixture]
    public class IgnoreListTests
    {
        private string _basePath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _basePath = AppDomain.CurrentDomain.BaseDirectory + @"test_content";
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
            Assert.IsTrue(log[0] == "Ignored by *.txt");
            Assert.IsTrue(log[1] == "Included by !sub1/*.txt");
            Assert.IsTrue(log[2] == "Ignored by sub1/README2.txt");
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
