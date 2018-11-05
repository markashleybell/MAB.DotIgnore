using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MAB.DotIgnore.Tests
{
    internal class GitTest
    {
        public int ID { get; set; }
        public string Pattern { get; set; }
        public string Path { get; set; }
        public bool ExpectGlobMatch { get; set; }
        public bool ExpectGlobMatchCI { get; set; }
        public bool ExpectPathMatch { get; set; }
        public bool ExpectPathMatchCI { get; set; }
    }

    [TestFixture(Category = "Integration Tests")]
    public class IntegrationTests
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
        public void Compare_Matcher_Output_With_C_Function()
        {
            var tests = File.ReadAllLines(_basePath + @"\git-tests\tests-current-fixed.txt")
                .Where(l => !l.StartsWith("#") && !string.IsNullOrWhiteSpace(l))
                .Select(l => l.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                .Select((l, i) => {
                    var path = l[5].Trim('\'', '"');
                    // This weirdness is because a few of the test patterns actually contain a space
                    // character, so we tack the extra element created by the split onto the end
                    var pattern = l[6].Trim('\'', '"') + (l.Length == 8 ? " " + l[7].Trim('\'', '"') : "");

                    // Manually hack around the fact that test 82 passes in a space as the path, which makes 
                    // both the pattern and path differ from the test file and causes the test to fail 
                    if (i == 82)
                    {
                        path = " ";
                        pattern = pattern.TrimStart(' ');
                    }

                    return new GitTest {
                        ID = i,
                        Pattern = pattern,
                        Path = path,
                        ExpectGlobMatch = l[1] == "1",
                        ExpectGlobMatchCI = l[2] == "1",
                        ExpectPathMatch = l[3] == "1",
                        ExpectPathMatchCI = l[4] == "1"
                    };
                })
                .ToList();


            tests.ForEach(t => {
                Console.WriteLine(string.Format("{0} {1}", t.Path, t.Pattern));
                var referenceReturnValue = ReferenceMatchPattern(t.Pattern, t.Path, false);
                var referenceResult = referenceReturnValue == 0;
                var testResult = Matcher.IsMatch(t.Pattern, t.Path);
                Assert.AreEqual(referenceResult, testResult);
            });
        }

        private Process CreateProcess(string executableFilename, string arguments, string workingDirectory)
        {
            return new Process {
                EnableRaisingEvents = true,
                StartInfo = new ProcessStartInfo {
                    FileName = executableFilename,
                    Arguments = arguments,
                    WorkingDirectory = workingDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
        }

        private int ReferenceMatchPattern(string pattern, string text, bool caseFold)
        {
            var workingDirectory = _basePath + @"\git-wildmatch";
    
            var log = new List<string>();
    
            var arguments = pattern + " " + text + " " + (caseFold ? 1 : 0);
    
            using (var build = CreateProcess(workingDirectory + @"\wm.exe", arguments, workingDirectory))
            {
                build.Start();

                build.OutputDataReceived += (sender, e) => log.Add("0> " + e.Data);
                build.BeginOutputReadLine();

                build.ErrorDataReceived += (sender, e) => log.Add("1> " + e.Data);
                build.BeginErrorReadLine();

                build.WaitForExit();
        
                return build.ExitCode;
            }
        }
        
        [Test]
        public void Copy_Non_Ignored_Solution_Files()
        {
            var sourceFolder = Directory.GetParent(TestContext.CurrentContext.TestDirec‌​tory).Parent.Parent.Parent.FullName;
            var destinationFolder = _basePath + @"\copy";

            if(Directory.Exists(destinationFolder))
                Directory.Delete(destinationFolder, true);

            Directory.CreateDirectory(destinationFolder);

            var source = new DirectoryInfo(sourceFolder);
            var destination = new DirectoryInfo(destinationFolder);

            // Load the solution .gitignore file
            var ignores = new IgnoreList(sourceFolder + @"\.gitignore");
            // Add an additional rule to ignore the .git folder
            ignores.AddRule(".git/");

            CopyWithIgnores(source, destination, ignores);

            // Do some very minimal checks and then just do some manual checking of the copy folder
            Assert.IsTrue(File.Exists(destinationFolder + @"\MAB.DotIgnore\MAB.DotIgnore.csproj"));
            Assert.IsTrue(File.Exists(destinationFolder + @"\MAB.DotIgnore.Test\MAB.DotIgnore.Test.csproj"));
            Assert.IsFalse(Directory.Exists(destinationFolder + @"\MAB.DotIgnore\bin"));
        }

        public static void CopyWithIgnores(DirectoryInfo source, DirectoryInfo target, IgnoreList ignores)
        {
            foreach (DirectoryInfo dir in source.GetDirectories().Where(d => !ignores.IsIgnored(d)))
                CopyWithIgnores(dir, target.CreateSubdirectory(dir.Name), ignores);

            foreach (FileInfo file in source.GetFiles().Where(f => !ignores.IsIgnored(f)))
                file.CopyTo(Path.Combine(target.FullName, file.Name));
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
