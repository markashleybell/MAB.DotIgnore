using System;
using System.Collections.Generic;
using System.Linq;
using MAB.DotIgnore;

namespace profile
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var fileList = TestData.Files;
            var ignoreList = new IgnoreList(TestData.Patterns);

            var results = new List<bool>();

            foreach (var f in fileList)
            {
                var isIgnored = ignoreList.IsIgnored(f, pathIsDirectory: false);

                results.Add(isIgnored);
            }

            Console.WriteLine(results.Count);
        }
    }
}
