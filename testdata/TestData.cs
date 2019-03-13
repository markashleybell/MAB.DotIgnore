using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BenchmarkDotNet.Jobs;

namespace testdata
{
    public static class TestData
    {
        public static readonly string[] Files =
            ReadLines("testdata.data.filelist.txt")
                .Select(l => Regex.Replace(l.Trim('"'), @"[A-Z]\:\\", "").Replace(@"\", "/"))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

        public static readonly string[] Patterns =
            ReadLines("testdata.data..ignores").ToArray();

        public static IEnumerable<string> ReadLines(string resource)
        {
            using (var s = typeof(TestData).GetTypeInfo().Assembly.GetManifestResourceStream(resource))
            using (var sr = new StreamReader(s))
            {
                var line = string.Empty;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        public static RunMode ParseRunMode(string arg)
        {
            switch (arg.ToUpperInvariant())
            {
                case "S":
                    return RunMode.Short;
                case "M":
                    return RunMode.Medium;
                case "L":
                    return RunMode.Long;
                default:
                    throw new ArgumentException("Run length must be s|m|l", nameof(arg));
            }
        }
    }
}
