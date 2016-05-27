using NUnit.Framework;
using System.IO;
using System.Linq;

namespace MAB.DotIgnore.Tests
{
    [TestFixture]
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
        public void Copy_Non_Ignored_Solution_Files()
        {
            var sourceFolder = Directory.GetParent(TestContext.CurrentContext.TestDirec‌​tory).Parent.Parent.FullName;
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
