using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace MAB.DotIgnore.Tests
{
    [TestFixture]
    public class WildMatchTests
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
        }

        [SetUp]
        public void SetUp()
        {

        }

        [Test]
        public void No_PATHNAME_Match_Slash_With_Single_Star()
        {
            var match = WildMatch.IsMatch("ab*c", "ab/c", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.MATCH);
        }

        [Test]
        public void Dont_Match_Slash_With_Question_Mark_Wildcard()
        {
            var match = WildMatch.IsMatch("ab?c", "ab/c", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.NOMATCH);
        }

        [Test]
        public void Single_Star_Match_Path_Within_Slashes()
        {
            var match = WildMatch.IsMatch("ab/*/ef", "ab/cd/ef", MatchFlags.PATHNAME);
            Assert.IsTrue(match == WildMatch.MATCH);
        }

        [Test]
        public void Single_Star_Dont_Match_Path_Without_Slash()
        {
            var match = WildMatch.IsMatch("ab/*/ef", "ab/cdef", MatchFlags.PATHNAME);
            Assert.IsTrue(match == WildMatch.NOMATCH);
        }

        [Test]
        public void Single_Star_No_PATHNAME_Set_Match_Slash()
        {
            var match = WildMatch.IsMatch("ab/**", "ab/cd/ef", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.MATCH);
        }

        [Test]
        public void Abort_Unclosed_Character_Range()
        {
            var match = WildMatch.IsMatch("abc.t[", "abc.txt", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.ABORT_ALL);
        }

        [Test]
        public void Abort_Unclosed_Character_Range_Negated()
        {
            var match = WildMatch.IsMatch("abc.t[^", "abc.txt", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.ABORT_ALL);
        }

        [Test]
        public void Abort_Unclosed_Character_Range_Escaped()
        {
            var match = WildMatch.IsMatch("abc.t[\\", "abc.txt", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.ABORT_ALL);
        }

        [Test]
        public void Match_Escaped_Character_Range()
        {
            var match = WildMatch.IsMatch("abc.t[\\[x]t", "abc.t[t", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.MATCH);
        }

        [Test]
        public void Handle_Unclosed_Character_Class()
        {
            var match = WildMatch.IsMatch("abc.t[:x]t", "abc.txt", MatchFlags.NONE);
            Assert.IsTrue(match == WildMatch.MATCH);
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
