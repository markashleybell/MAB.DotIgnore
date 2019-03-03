using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace profile
{
    public static class TestData
    {
        public static readonly string[] Files =
            ReadLines("profile.data.filelist.txt")
                .Select(l => l.Trim('"').Replace(@"C:\", "").Replace(@"\", "/"))
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

        public static readonly string[] Patterns =
            ReadLines("profile.data..ignores").ToArray();

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
    }
}
