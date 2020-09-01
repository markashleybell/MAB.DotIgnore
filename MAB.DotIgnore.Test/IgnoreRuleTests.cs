using System;
using System.IO;
using NUnit.Framework;

namespace MAB.DotIgnore.Tests
{
    [TestFixture(Category = "IgnoreRule Tests")]
    public class IgnoreRuleTests
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
        public void Null_Pattern_Throws_Exception() => Assert.Throws<ArgumentException>(() => new IgnoreRule(null));

        [Test]
        public void Null_Path_Throws_Exception()
        {
            var rule = new IgnoreRule("test.txt");
            Assert.Throws<ArgumentException>(() => rule.IsMatch(null, false));
        }

        [Test]
        public void Empty_Pattern_Throws_Exception() => Assert.Throws<ArgumentException>(() => new IgnoreRule("  "));

        [Test]
        public void Empty_Path_Throws_Exception()
        {
            var rule = new IgnoreRule("test.txt");
            Assert.Throws<ArgumentException>(() => rule.IsMatch("  ", false));
        }

        [Test]
        public void ToString_Returns_Correct_Pattern_Info()
        {
            var rule = new IgnoreRule("sub2/**.txt");

            // Should return original glob pattern > modified glob pattern
            Assert.IsTrue(rule.ToString() == "sub2/**.txt");
        }

        [Test]
        public void Case_Sensitive_Match()
        {
            var rule = new IgnoreRule("a/**/teSt.tXt");
            Assert.IsTrue(rule.IsMatch("a/b/teSt.tXt", true));
            Assert.IsFalse(rule.IsMatch("a/b/test.txt", true));
            Assert.IsFalse(rule.IsMatch("a/b/TEST.TXT", true));
        }

        [Test]
        public void Case_Insensitive_Match()
        {
            // Use a glob pattern to get past the shortcuts and exercise WildMatch
            var rule = new IgnoreRule("a/**/teSt.tXt", MatchFlags.CASEFOLD);
            Assert.IsTrue(rule.IsMatch("a/b/teSt.tXt", true));
            Assert.IsTrue(rule.IsMatch("a/b/test.txt", true));
            Assert.IsTrue(rule.IsMatch("a/b/TEST.TXT", true));
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
            var directory = new DirectoryInfo(_basePath + "/test");

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

        [Test]
        public void Match_Question_Mark_Wildcard()
        {
            var rule = new IgnoreRule("?at.txt");
            Assert.IsTrue(rule.IsMatch("/cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/mat.txt", true));
            Assert.IsFalse(rule.IsMatch("/hot.txt", true));
        }

        [Test]
        public void Match_Escape_Char()
        {
            var rule = new IgnoreRule(@"h\?t.txt");
            Assert.IsTrue(rule.IsMatch("/h?t.txt", true));
            Assert.IsFalse(rule.IsMatch("/hat.txt", true));
        }

        [Test]
        public void Match_Trailing_Star()
        {
            var rule = new IgnoreRule(@"a/b/*");
            Assert.IsTrue(rule.IsMatch("a/b/c", true));
            Assert.IsFalse(rule.IsMatch("a/b/c/d", true));
        }

        [Test]
        public void Match_Character_Choice_Case_Sensitive()
        {
            var rule = new IgnoreRule(@"[ChM]at.txt");
            Assert.IsTrue(rule.IsMatch("/Cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/Mat.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
            Assert.IsFalse(rule.IsMatch("/Hat.txt", true));
            Assert.IsFalse(rule.IsMatch("/mat.txt", true));
        }

        [Test]
        public void Match_Character_Choice_Case_Insensitive()
        {
            var rule = new IgnoreRule(@"[chm]at.txt", MatchFlags.CASEFOLD);
            Assert.IsTrue(rule.IsMatch("/Cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/Mat.txt", true));
            Assert.IsTrue(rule.IsMatch("/cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/Hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/mat.txt", true));
        }

        [Test]
        public void Match_Character_Range_Case_Sensitive()
        {
            var rule = new IgnoreRule(@"[A-De-z]at.txt");
            Assert.IsTrue(rule.IsMatch("/Cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/mat.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
            Assert.IsFalse(rule.IsMatch("/Hat.txt", true));
            Assert.IsFalse(rule.IsMatch("/Mat.txt", true));
        }

        [Test]
        public void Match_Character_Range_Case_Insensitive()
        {
            var rule = new IgnoreRule(@"[A-De-z]at.txt", MatchFlags.CASEFOLD);
            Assert.IsTrue(rule.IsMatch("/Cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/mat.txt", true));
            Assert.IsTrue(rule.IsMatch("/cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/Hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/Mat.txt", true));
        }

        [Test]
        public void Negated_Match_Character_Range()
        {
            var rule = new IgnoreRule(@"[^C-Dc-d]at.txt", MatchFlags.CASEFOLD);
            Assert.IsFalse(rule.IsMatch("/Cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/mat.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/Hat.txt", true));
            Assert.IsTrue(rule.IsMatch("/Mat.txt", true));
        }

        [Test]
        public void Match_Character_Class_Alnum()
        {
            var rule = new IgnoreRule(@"[[:alnum:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/3at.txt", true));
            Assert.IsFalse(rule.IsMatch("/~at.txt", true));
        }

        [Test]
        public void Match_Character_Class_Alpha()
        {
            var rule = new IgnoreRule(@"[[:alpha:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/cat.txt", true));
            Assert.IsFalse(rule.IsMatch("/1at.txt", true));
        }

        [Test]
        public void Match_Character_Class_Blank()
        {
            var rule = new IgnoreRule(@"[[:blank:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/ at.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
        }

        [Test]
        public void Match_Character_Class_Cntrl()
        {
            var rule = new IgnoreRule(@"[[:cntrl:]]at.txt");
            var controlChar = Convert.ToChar(0x00);
            Assert.IsTrue(rule.IsMatch(controlChar + "at.txt", true));
        }

        [Test]
        public void Match_Character_Class_Digit()
        {
            var rule = new IgnoreRule(@"[[:digit:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/2at.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
        }

        [Test]
        public void Match_Character_Class_Lower()
        {
            var rule = new IgnoreRule(@"[[:lower:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/cat.txt", true));
            Assert.IsFalse(rule.IsMatch("/Cat.txt", true));
        }

        [Test]
        public void Match_Character_Class_Punct()
        {
            var rule = new IgnoreRule(@"[[:punct:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/.at.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
        }

        [Test]
        public void Match_Character_Class_Space()
        {
            var rule = new IgnoreRule(@"[[:space:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/ at.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
        }

        [Test]
        public void Match_Character_Class_Upper()
        {
            var rule = new IgnoreRule(@"[[:upper:]]at.txt");
            Assert.IsTrue(rule.IsMatch("/Cat.txt", true));
            Assert.IsFalse(rule.IsMatch("/cat.txt", true));
        }

        [Test]
        public void Match_Character_Class_Upper_Casefold()
        {
            var rule = new IgnoreRule(@"[[:upper:]]at.txt", MatchFlags.CASEFOLD);
            Assert.IsTrue(rule.IsMatch("/Cat.txt", true));
            Assert.IsTrue(rule.IsMatch("/cat.txt", true));
        }

        [Test]
        public void Match_Character_Class_XDigit()
        {
            var rule = new IgnoreRule(@"1[[:xdigit:]].txt");
            Assert.IsTrue(rule.IsMatch("/1F.txt", true));
            Assert.IsFalse(rule.IsMatch("/1G.txt", true));
        }

        [Test]
        public void Match_Malformed_Character_Class()
        {
            var rule = new IgnoreRule(@"1[[:malformed:]].txt");
            Assert.IsFalse(rule.IsMatch("/1F.txt", true));
            Assert.IsFalse(rule.IsMatch("/1G.txt", true));
        }

        [Test]
        public void Match_Literal_After_Star_Case_Sensitive()
        {
            var rule = new IgnoreRule(@"A/b/*Cd.txt");
            Assert.IsTrue(rule.IsMatch("/A/b/dCd.txt", true));
            Assert.IsFalse(rule.IsMatch("/a/b/dcD.txt", true));
        }

        [Test]
        public void Match_Literal_After_Star_Case_Insensitive()
        {
            var rule = new IgnoreRule(@"A/b/*Cd.txt", MatchFlags.CASEFOLD);
            Assert.IsTrue(rule.IsMatch("/A/b/dCd.txt", true));
            Assert.IsTrue(rule.IsMatch("/a/b/dcD.txt", true));
        }

        [Test]
        public void Abort_Invalid_Pattern()
        {
            var rule = new IgnoreRule(@"a/te**.txt");
            Assert.IsFalse(rule.IsMatch("a/test.txt", false));
        }

        [Test]
        public void Dont_Match_Same_Endings()
        {
            var rule = new IgnoreRule(@"ignored/");
            Assert.IsTrue(rule.IsMatch("ignored", true));
            Assert.IsFalse(rule.IsMatch("notignored", true));
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
