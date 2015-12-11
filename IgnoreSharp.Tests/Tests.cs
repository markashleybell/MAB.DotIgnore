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

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            _basePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void Ignore_List()
        {
            var files = new List<string> {
                @"cms\ignore\test.cs",
                @"cms\include\Ignore.cs",
                @"cms\test\Test.cs",
                @"cms\test\ignore.cs",
                @"cms\test\subfolder\test.cs",
                @"web\data.cs",
                @"web\test\test1.cs",
                @"web\test\test2.cs",
                @"web\test\README",
                @"web\test\subfolder\test.cs",
                @"data.cs",
                @"README"
            };

            var ignoreList = new IgnoreList(_basePath + @"\test.gitignore");

            var ignoredFiles = files.Where(f => ignoreList.IsMatch(f));

            Assert.IsTrue(ignoredFiles.Count() == 4);
        }

        [TearDown]
        public void TearDown()
        {

        }
    }
}
