using MAB.DotIgnore;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Benchmark
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string workingDirectory = @"C:\Src\MAB.DotIgnore\tools\benchmark";

            // This gives us an array of ~1500 file paths
            List<string> fileList = File.ReadAllLines($@"{workingDirectory}\filelist.txt")
                .Select(l => l.Trim('"').Replace(@"C:\", "").Replace(@"\", "/"))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToList();

            IgnoreList ignoreList = new IgnoreList($@"{workingDirectory}\.ignores");

            void action() => fileList.ForEach(f => ignoreList.IsIgnored(f, pathIsDirectory: false));

            for (int i = 0; i < 20; i++)
            {
                action();
            }
        }
    }
}
