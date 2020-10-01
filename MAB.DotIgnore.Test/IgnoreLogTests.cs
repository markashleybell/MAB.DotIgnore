using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace MAB.DotIgnore.Tests
{
    [TestFixture(Category = "IgnoreLog Tests")]
    public class IgnoreLogTests
    {
        private string _basePath;

        [OneTimeSetUp]
        public void OneTimeSetUp() =>
            _basePath = TestContext.CurrentContext.TestDirectory + "/test_content";

        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Log_Matched_Rules()
        {
            var list = new IgnoreList(new string[] { "*.txt", "*.cs", "!sub1/*.txt", "sub1/README2.txt" });

            var log = new IgnoreLog();

            list.IsIgnored("sub1/README2.txt", true, log);

            Assert.IsTrue(log.Count == 1);
            Assert.IsTrue(log["sub1/README2.txt"].Count == 3);
            Assert.IsTrue(log["sub1/README2.txt"][0] == "IGNORED by *.txt");
            Assert.IsTrue(log["sub1/README2.txt"][1] == "INCLUDED by !sub1/*.txt");
            Assert.IsTrue(log["sub1/README2.txt"][2] == "IGNORED by sub1/README2.txt");
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
            Assert.IsTrue(log[file.FullName][0] == "IGNORED by test.txt");
        }

        [Test]
        public void DirectoryInfo_Match_Log()
        {
            var directory = new DirectoryInfo(_basePath + "/test");

            var list = new IgnoreList(new string[] { "test" });

            var log = new IgnoreLog();

            Assert.IsTrue(list.IsIgnored(directory, log));
            Assert.IsTrue(log.Count == 1);
            Assert.IsTrue(log[directory.FullName].Count == 1);
            Assert.IsTrue(log[directory.FullName][0] == "IGNORED by test");
        }

        [Test]
        public void Log_ToString()
        {
            var log = new IgnoreLog();

            var list = new IgnoreList(new string[] { "one/", "two/", "!one/two/" });

            var paths = new List<string> { "one", "one/two", "two" };

            paths.ForEach(path => list.IsIgnored(path, true, log));

            const string expectedResult = @"one
    IGNORED by one/

one/two
    IGNORED by one/
    INCLUDED by !one/two/

two
    IGNORED by two/";

            Assert.IsTrue(log.ToString() == expectedResult);
        }

        [Test]
        public void File_Rule_Line_Numbers_ToString()
        {
            var log = new IgnoreLog();

            var list = new IgnoreList(new string[] { "*.cs" });

            list.AddRules(_basePath + "/multiplematch.gitignore");

            var paths = new List<string> { "test/test1.cs", "test/test2.cs" };

            paths.ForEach(path => list.IsIgnored(path, true, log));

            const string expectedResult = @"test/test1.cs
    IGNORED by *.cs
    INCLUDED by !test/*.cs (line 3)
    IGNORED by test/test*.cs (line 4)

test/test2.cs
    IGNORED by *.cs
    INCLUDED by !test/*.cs (line 3)
    IGNORED by test/test*.cs (line 4)
    INCLUDED by !test/test2.cs (line 6)";

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
