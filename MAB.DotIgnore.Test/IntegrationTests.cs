using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MAB.DotIgnore.Test.Support;
using NUnit.Framework;

namespace MAB.DotIgnore.Tests
{
    [TestFixture(Category = "Integration Tests")]
    public class IntegrationTests
    {
        private string _basePath;

        public static void CopyWithIgnores(DirectoryInfo source, DirectoryInfo target, IgnoreList ignores)
        {
            foreach (var dir in source.GetDirectories().Where(d => !ignores.IsIgnored(d)))
            {
                CopyWithIgnores(dir, target.CreateSubdirectory(dir.Name), ignores);
            }

            foreach (var file in source.GetFiles().Where(f => !ignores.IsIgnored(f)))
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }

        public static string TrimQuotes(string s) => s.Trim('\'', '"');

        [Test]
        public void Compare_Matcher_Results_With_Wildmatch_Expectations()
        {
            var testLineRx = new Regex(@"^match ([01]) ([01]) ([01]) ([01]) ('.+?'|.+?) ('.+?'|.+?)$", RegexOptions.IgnoreCase);

            var tests = File.ReadAllLines(_basePath + "/git-tests/tests-current-fixed.txt")
                .Select((s, i) => (content: s, number: i))
                .Where(line => !line.content.StartsWith("#", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(line.content))
                .Select(line => (match: testLineRx.Match(line.content), lineNo: line.number))
                .Select(test => new GitTest {
                    LineNumber = test.lineNo,
                    Pattern = test.match.Groups[6].Value,
                    Path = test.match.Groups[5].Value,
                    ExpectGlobMatch = test.match.Groups[1].Value == "1",
                    ExpectGlobMatchCI = test.match.Groups[2].Value == "1",
                    ExpectPathMatch = test.match.Groups[3].Value == "1",
                    ExpectPathMatchCI = test.match.Groups[4].Value == "1",
                });

            var expected = tests.Select(t => {
                var pattern = TrimQuotes(t.Pattern);
                var path = TrimQuotes(t.Path);

                return new MatchTestResult {
                    LineNumber = t.LineNumber,
                    Pattern = pattern,
                    Path = path,
                    Regex = Matcher.ToRegex(pattern),
                    Result = t.ExpectGlobMatch,
                    ResultCI = t.ExpectGlobMatchCI,
                };
            });

            var actual = tests.Select(t => {
                var pattern = TrimQuotes(t.Pattern);
                var path = TrimQuotes(t.Path);

                var rxPattern = Matcher.ToRegex(pattern);

                Regex rx = null;
                Regex rxCI = null;

                try
                {
                    rx = new Regex(rxPattern);
                    rxCI = new Regex(rxPattern, RegexOptions.IgnoreCase);
                }
                catch
                {
                }

                return new MatchTestResult {
                    LineNumber = t.LineNumber,
                    Pattern = pattern,
                    Path = path,
                    Regex = rxPattern,
                    Result = Matcher.TryMatch(rx, path),
                    ResultCI = Matcher.TryMatch(rxCI, path),
                };
            });

            var failed = actual
                .Where(a => {
                    var ex = expected.Single(e => e.LineNumber == a.LineNumber);
                    return a.Result != ex.Result || a.ResultCI != ex.ResultCI;
                })
                .Select(a => new {
                    a.LineNumber,
                    a.Pattern,
                    a.Path,
                    a.Regex,
                    Expected = !a.Result,
                    Actual = a.Result,
                    ExpectedCI = !a.ResultCI,
                    ActualCI = a.ResultCI,
                });

            Assert.That(failed.Count() == 0);

            foreach (var t in failed)
            {
                Console.WriteLine($"{t.Path} {t.Pattern} {t.Regex}");
            }
        }

        [Test]
        public void Copy_Non_Ignored_Solution_Files()
        {
            var sourceFolder = Directory.GetParent(TestContext.CurrentContext.TestDirectory).Parent.Parent.Parent.FullName;
            var destinationFolder = _basePath + "/copy";

            if (Directory.Exists(destinationFolder))
            {
                Directory.Delete(destinationFolder, true);
            }

            Directory.CreateDirectory(destinationFolder);

            var source = new DirectoryInfo(sourceFolder);
            var destination = new DirectoryInfo(destinationFolder);

            // Load the solution .gitignore file
            var ignores = new IgnoreList(sourceFolder + "/.gitignore");

            // Add an additional rule to ignore the .git folder
            ignores.AddRule(".git/");

            CopyWithIgnores(source, destination, ignores);

            // Do some very minimal checks and then just do some manual checking of the copy folder
            Assert.IsTrue(File.Exists(destinationFolder + "/MAB.DotIgnore/MAB.DotIgnore.csproj"));
            Assert.IsTrue(File.Exists(destinationFolder + "/MAB.DotIgnore.Test/MAB.DotIgnore.Test.csproj"));
            Assert.IsFalse(Directory.Exists(destinationFolder + "/MAB.DotIgnore/bin"));
        }

        [OneTimeSetUp]
        public void OneTimeSetUp() =>
            _basePath = TestContext.CurrentContext.TestDirectory + "/test_content";

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
        }

        [SetUp]
        public void SetUp()
        {
        }

        [TearDown]
        public void TearDown()
        {
        }
    }
}
