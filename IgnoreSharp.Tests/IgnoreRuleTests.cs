using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace IgnoreSharp.Tests
{
    [TestFixture]
    public class IgnoreRuleTests
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
            // Should return original glob pattern > modified glob pattern > translated regex
            Assert.IsTrue(rule.ToString() == "sub2/**.txt > SUB2/**.TXT > SUB2/.*\\.TXT");
        }

        [Test]
        public void Match_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("test.txt");
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Match_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("/test.txt");
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Match_Directory_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("test/");
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsFalse(rule.IsMatch("/test", false));
        }

        [Test]
        public void Match_Directory_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("/test/");
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsFalse(rule.IsMatch("/test", false));
        }

        [Test]
        public void Negated_Match_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("!test.txt");
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Negated_Match_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("!/test.txt");
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Negated_Match_Directory_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("!test/");
            Assert.IsFalse(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsTrue(rule.IsMatch("/test", false));
        }

        [Test]
        public void Negated_Match_Directory_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("!/test/");
            Assert.IsFalse(rule.IsMatch("/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file called 'test' with no extension
            Assert.IsTrue(rule.IsMatch("/test", false));
        }

        [Test]
        public void Match_Global_Wildcards()
        {
            var rule = new IgnoreRule("*.txt");
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Negated_Match_Global_Wildcards()
        {
            var rule = new IgnoreRule("!*.txt");
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Match_Global_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("**.txt");
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Match_Relative_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("sub2/**.txt");
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", true));
        }

        [Test]
        public void Match_Absolute_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("/sub1/**.txt");
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub0/sub1/test.txt", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
        }

        [Test]
        public void Match_Trailing_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("sub1/**");
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.jpg", false));
            // Should match directory as well
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
        }

        [Test]
        public void Match_Directory_Global_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("**test/");
            Assert.IsTrue(rule.IsMatch("/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
        }

        [Test]
        public void Match_Directory_Relative_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("sub1/**/test/");
            Assert.IsFalse(rule.IsMatch("/test", true));
            Assert.IsFalse(rule.IsMatch("/sub1/test", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test", false));
        }

        [Test]
        public void Match_Directory_Absolute_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("/sub1/**.txt/");
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", true));
            Assert.IsFalse(rule.IsMatch("/sub0/sub1/test.txt", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
        }

        [Test]
        public void Match_Directory_Trailing_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("sub1/**/");
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", true));
            Assert.IsTrue(rule.IsMatch("/sub1/test.jpg", true));
            // Should not match file
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
        }

        [Test]
        public void Negated_Match_Global_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!**.txt");
            Assert.IsFalse(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsFalse(rule.IsMatch("/test.txt", true));
        }

        [Test]
        public void Negated_Match_Relative_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!sub2/**.txt");
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            // Should match directory as well
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", true));
        }

        [Test]
        public void Negated_Match_Absolute_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!/sub1/**.txt");
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            Assert.IsTrue(rule.IsMatch("/sub0/sub1/test.txt", false));
            // Should match directory as well
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", true));
        }

        [Test]
        public void Negated_Match_Trailing_Star_Star_Wildcard()
        {
            var rule = new IgnoreRule("!sub1/**");
            Assert.IsTrue(rule.IsMatch("/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/sub2/test.txt", false));
            Assert.IsFalse(rule.IsMatch("/sub1/test.jpg", false));
            // Should match directory as well
            Assert.IsFalse(rule.IsMatch("/sub1/test.txt", true));
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
