using NUnit.Framework;
using System;

namespace MAB.DotIgnore.Tests
{
    [TestFixture]
    public class UtilsTests
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
        public void StringIsNullOrWhiteSpace_Null()
        {
            Assert.IsTrue(Utils.IsNullOrWhiteSpace(null));
        }

        [Test]
        public void StringIsNullOrWhiteSpace_Empty()
        {
            Assert.IsTrue(Utils.IsNullOrWhiteSpace(""));
        }

        [Test]
        public void StringIsNullOrWhiteSpace_White_Space()
        {
            Assert.IsTrue(Utils.IsNullOrWhiteSpace("   "));
        }

        [Test]
        public void StringIsNullOrWhiteSpace_String()
        {
            Assert.IsFalse(Utils.IsNullOrWhiteSpace("ABC"));
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
