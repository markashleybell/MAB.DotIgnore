using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace MAB.DotIgnore.Tests
{
    [TestFixture(Category = "IgnoreList Tests")]
    public class IgnoreListTests
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
        public void Load_Rules_From_File()
        {
            var ignoreList = new IgnoreList(_basePath + "/loadfromfile.gitignore");
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
            Assert.IsTrue(ignoreList.IsIgnored("README1.txt", true));
            Assert.IsTrue(ignoreList.IsIgnored("README2.txt", true));
            Assert.IsTrue(ignoreList.IsIgnored("README3.txt", true));
            Assert.IsTrue(ignoreList.IsIgnored("README4.txt", true));
        }

        [Test]
        public void Ignore_After_Dynamic_Remove()
        {
            var ignoreList = new IgnoreList(new List<string> { "README1.txt", "README2.txt", "README3.txt", "README4.txt" });
            ignoreList.RemoveRule("README2.txt");
            Assert.IsTrue(ignoreList.IsIgnored("README1.txt", true));
            Assert.IsFalse(ignoreList.IsIgnored("README2.txt", true));
            Assert.IsTrue(ignoreList.IsIgnored("README3.txt", true));
            Assert.IsTrue(ignoreList.IsIgnored("README4.txt", true));
        }

        [Test]
        public void Add_Rules_From_File()
        {
            var ignoreList = new IgnoreList(new string[] { "README.txt" });
            ignoreList.AddRules(_basePath + "/loadfromfile.gitignore");
            Assert.IsTrue(ignoreList.IsIgnored("README.txt", true));
            Assert.IsFalse(ignoreList.IsIgnored("test.jpg", true));
            Assert.IsTrue(ignoreList.IsIgnored("test.cs", true));
        }

        [Test]
        public void Clone()
        {
            var original = new IgnoreList(new string[] { "README1.txt", "README2.txt" });
            var clone = original.Clone();
            Assert.IsTrue(clone.IsIgnored("README1.txt", true));
            Assert.IsTrue(clone.IsIgnored("README2.txt", true));
            Assert.IsFalse(clone.IsIgnored("README3.txt", true));
        }

        [Test]
        public void Clone_Keeps_Match_Flags()
        {
            var original = new IgnoreList(new string[] { "README1.txt", "README2.txt" }, MatchFlags.PATHNAME | MatchFlags.CASEFOLD);
            original.AddRule("README3.txt", MatchFlags.NONE);
            var clone = original.Clone();
            Assert.IsTrue(clone.Rules[0].MatchFlags == (MatchFlags.PATHNAME | MatchFlags.CASEFOLD));
            Assert.IsTrue(clone.Rules[1].MatchFlags == (MatchFlags.PATHNAME | MatchFlags.CASEFOLD));

            // Should use PATHNAME because we assign this internally if it's not set
            Assert.IsTrue(clone.Rules[2].MatchFlags == MatchFlags.PATHNAME);
        }

        [Test]
        public void Respect_Rule_Overrides()
        {
            var ignoreList = new IgnoreList(new string[] { "*.txt", "!sub1/*.txt", "sub1/README2.txt" });
            var matched = new List<IgnoreRule>();
            Assert.IsTrue(ignoreList.IsIgnored("README1.txt", true));
            Assert.IsFalse(ignoreList.IsIgnored("sub1/README1.txt", true));
            Assert.IsTrue(ignoreList.IsIgnored("sub1/README2.txt", true));
        }

        [Test]
        public void FileInfo_Match()
        {
            var directory = new DirectoryInfo(_basePath);
            var file = directory.GetFiles("*.txt")[0];
            var list = new IgnoreList(new string[] { "test.txt" });
            Assert.IsTrue(list.IsIgnored(file));
        }

        [Test]
        public void DirectoryInfo_Match()
        {
            var directory = new DirectoryInfo(_basePath + "/test");
            var list = new IgnoreList(new string[] { "test" });
            Assert.IsTrue(list.IsIgnored(directory));
        }

        [Test]
        public void Constructor_Flags_Respected()
        {
            var directory = new DirectoryInfo(_basePath + "/TEST");

            // Case sensitive list, should not match
            var list1 = new IgnoreList(new string[] { "test" });

            // Case insensitive, should match
            var list2 = new IgnoreList(new string[] { "test" }, MatchFlags.CASEFOLD);
            Assert.IsFalse(list1.IsIgnored(directory));
            Assert.IsTrue(list2.IsIgnored(directory));
        }

        [Test]
        public void Add_Rule_Flags_Respected()
        {
            var directory = new DirectoryInfo(_basePath + "/TEST");
            var list1 = new IgnoreList(new string[] { "x" });
            var list2 = new IgnoreList(new string[] { "x" });
            list1.AddRule("test");
            list2.AddRule("test", MatchFlags.CASEFOLD);
            Assert.IsFalse(list1.IsIgnored(directory));
            Assert.IsTrue(list2.IsIgnored(directory));
        }

        [Test]
        public void Ignored_Directory_Ignores_All_Children()
        {
            var list = new IgnoreList(new string[] { "ignored/", "!ignored/one/two/" });
            Assert.IsTrue(list.IsIgnored("ignored/test.txt", false));
            Assert.IsTrue(list.IsIgnored("ignored/one/test.txt", false));
            Assert.IsTrue(list.IsIgnored("ignored/two/three.txt", false));
            Assert.IsTrue(list.IsIgnored("ignored/one/two/test.txt", false));
            Assert.IsTrue(list.IsIgnored("ignored/one/two/three/test.txt", false));
            Assert.IsFalse(list.IsIgnored("notignored/test.txt", false));
            Assert.IsFalse(list.IsIgnored("notignored/one/test.txt", false));
            Assert.IsFalse(list.IsIgnored("notignored/one/two/test.txt", false));
        }

        [Test]
        public void Handle_Non_Relative_Path()
        {
            var list = new IgnoreList(new string[] { "ignored/" });

            Assert.IsTrue(list.IsIgnored("/ignored/test/test.txt", false));
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
