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
        public void BASIC_WILDCARD()
        {
            var rule = new IgnoreRule("*.txt");
            Assert.IsTrue(rule.IsMatch("/test.txt", true));
            //Assert.IsTrue(rule.IsMatch("/sub1/test.txt", true));
            //Assert.IsTrue(rule.IsMatch("/sub1/sub2/test.txt", true));
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
