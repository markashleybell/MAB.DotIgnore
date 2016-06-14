using NUnit.Framework;
using System;
using System.IO;

namespace MAB.DotIgnore.Tests
{
    [TestFixture]
    public class IgnoreRuleTests
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
        public void Null_Pattern_Throws_Exception()
        {
            Assert.Throws<ArgumentNullException>(() => new IgnoreRule(null));
        }

        [Test]
        public void Null_Path_Throws_Exception()
        {
            var rule = new IgnoreRule("test.txt");
            Assert.Throws<ArgumentNullException>(() => rule.IsMatch(null, false));
        }

        [Test]
        public void ToString_Returns_Correct_Pattern_Info()
        {
            var rule = new IgnoreRule("sub2/**.txt");
            // Should return original glob pattern > modified glob pattern
            Assert.IsTrue(rule.ToString() == "sub2/**.txt > sub2/**.txt");
        }

        [Test]
        public void Case_Sensitive_Match()
        {
            var rule = new IgnoreRule("test.txt");
            Assert.IsTrue(rule.IsMatch("test.txt", true));
            Assert.IsFalse(rule.IsMatch("TEST.TXT", true));
        }

        [Test]
        public void Windows_Paths_Converted()
        {
            var rule = new IgnoreRule("/c/textfiles/test.txt");
            Assert.IsTrue(rule.IsMatch(@"c:\textfiles\test.txt", true));
        }

        [Test]
        public void FileInfo_Match()
        {
            var directory = new DirectoryInfo(_basePath);
            var file = directory.GetFiles("*.txt")[0];

            var rule = new IgnoreRule("test.txt");
            Assert.IsTrue(rule.IsMatch(file));
        }

        [Test]
        public void DirectoryInfo_Match()
        {
            var directory = new DirectoryInfo(_basePath + @"\test");

            var rule = new IgnoreRule("test");
            Assert.IsTrue(rule.IsMatch(directory));
        }

        [Test]
        public void Match_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("test.txt");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/test.txt2", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
            Assert.IsFalse(rule.IsMatch("/test.txt2", true));
        }

        [Test]
        public void Match_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("/test.txt");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/test.txt2", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
            Assert.IsFalse(rule.IsMatch("/test.txt2", true));
        }

        [Test]
        public void Match_Directory_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("test/");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/test2", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsFalse(rule.IsMatch("/test", false));
        }

        [Test]
        public void Match_Directory_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("/test/");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/test2", true));
            Assert.IsFalse(rule.IsMatch("/sub1/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsFalse(rule.IsMatch("/test", false));
        }

        [Test]
        public void Negated_Match_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("!test.txt");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/test.txt2", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
            Assert.IsFalse(rule.IsMatch("/test.txt2", true));
        }

        [Test]
        public void Negated_Match_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("!/test.txt");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/test.txt2", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
            Assert.IsFalse(rule.IsMatch("/test.txt2", true));
        }

        [Test]
        public void Negated_Match_Directory_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("!test/");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/test2", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsFalse(rule.IsMatch("/test", false));
        }

        [Test]
        public void Negated_Match_Directory_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("!/test/");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/test2", true));
            Assert.IsFalse(rule.IsMatch("/sub1/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsFalse(rule.IsMatch("/test", false));
        }

        [Test]
        public void Match_Global_Wildcards()
        {
            var rule = new IgnoreRule("*.txt");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/test.txt2", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
            Assert.IsFalse(rule.IsMatch("/test.txt2", true));
        }

        [Test]
        public void Negated_Match_Global_Wildcards()
        {
            var rule = new IgnoreRule("!*.txt");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/test.txt2", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
            Assert.IsFalse(rule.IsMatch("/test.txt2", true));
        }

        [Test]
        public void Match_Leading_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("**/test");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/test2", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", false));
        }

        [Test]
        public void Match_Trailing_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("sub1/**");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.jpg", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
        }

        [Test]
        public void Match_Directory_Relative_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("sub1/**/test/");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsFalse(rule.IsMatch("/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test2", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test", false));
        }

        [Test]
        public void Match_Directory_Trailing_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("sub1/**/");
            Assert.IsFalse(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test.jpg", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
        }

        [Test]
        public void Negated_Match_Leading_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!**/test");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/test2", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test2", false));
        }

        [Test]
        public void Negated_Match_Trailing_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!sub1/**");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.jpg", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
        }

        [Test]
        public void Negated_Match_Directory_Relative_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!sub1/**/test/");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsFalse(rule.IsMatch("/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test2", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test", false));
        }

        [Test]
        public void Negated_Match_Directory_Trailing_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!sub1/**/");
            Assert.IsTrue(rule.PatternFlags.HasFlag(PatternFlags.NEGATION));
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test.jpg", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
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
