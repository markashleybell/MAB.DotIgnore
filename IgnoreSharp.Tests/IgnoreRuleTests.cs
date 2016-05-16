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
        public void Match_File_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("XXXXX");
            Assert.IsTrue(rule.IsMatch("YYYYY", false));
        }

        [Test]
        public void Match_File_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("XXXXX");
            Assert.IsTrue(rule.IsMatch("YYYYY", false));
        }

        [Test]
        public void Match_Directory_Relative_No_Wildcards()
        {
            var rule = new IgnoreRule("XXXXX");
            Assert.IsTrue(rule.IsMatch("YYYYY", true));
        }

        [Test]
        public void Match_Directory_Absolute_No_Wildcards()
        {
            var rule = new IgnoreRule("XXXXX");
            Assert.IsTrue(rule.IsMatch("YYYYY", true));
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
