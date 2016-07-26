using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace MAB.DotIgnore.Tests
{
    [TestFixture]
    public class IgnoreLogTests
    {
        private string _basePath;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _basePath = TestContext.CurrentContext.TestDirec‌​tory + @"\test_content";
        }

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Log_Matched_Rules()
        {
            var ignoreList = new IgnoreList(new string[] { "*.txt", "*.cs", "!sub1/*.txt", "sub1/README2.txt" });
            var log = new IgnoreLog();
            ignoreList.IsIgnored("sub1/README2.txt", true, log);
            Assert.IsTrue(log.Count == 1);
            Assert.IsTrue(log["sub1/README2.txt"].Count == 3);
            Assert.IsTrue(log["sub1/README2.txt"][0] == "IGNORED by *.txt > *.txt");
            Assert.IsTrue(log["sub1/README2.txt"][1] == "INCLUDED by !sub1/*.txt > sub1/*.txt");
            Assert.IsTrue(log["sub1/README2.txt"][2] == "IGNORED by sub1/README2.txt > sub1/README2.txt");
        }

        [Test]
        public void FileInfo_Match_Log()
        {
            var directory = new DirectoryInfo(_basePath);
            var file = directory.GetFiles("*.txt")[0];
            var list = new IgnoreList(new string[] { "test.txt" });
            var log = new IgnoreLog();
            Assert.IsTrue(list.IsIgnored(file, log));
            Assert.IsTrue(log.Count == 1);
            Assert.IsTrue(log[file.FullName].Count == 1);
            Assert.IsTrue(log[file.FullName][0] == "IGNORED by test.txt > test.txt");
        }

        [Test]
        public void DirectoryInfo_Match_Log()
        {
            var directory = new DirectoryInfo(_basePath + @"\test");
            var list = new IgnoreList(new string[] { "test" });
            var log = new IgnoreLog();
            Assert.IsTrue(list.IsIgnored(directory, log));
            Assert.IsTrue(log.Count == 1);
            Assert.IsTrue(log[directory.FullName].Count == 1);
            Assert.IsTrue(log[directory.FullName][0] == "IGNORED by test > test");
        }
        
        [Test]
        public void Log_ToString()
        {
            var log = new IgnoreLog();
            var ignoreList = new IgnoreList(new string[] { "one/", "two/", "!one/two/" });
            var paths = new List<string> { "one", "one/two", "two" };
            paths.ForEach(path => ignoreList.IsIgnored(path, true, log));
            var expectedResult = @"one
    IGNORED by one/ > one

one/two
    IGNORED by one/ > one
    IGNORED by two/ > two
    INCLUDED by !one/two/ > one/two

two
    IGNORED by two/ > two";
            Assert.IsTrue(log.ToString() == expectedResult);
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
