using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace IgnoreSharp
{
    public class IgnoreList
    {
        private IEnumerable<IgnoreRule> _rules;

        public IgnoreList(string ignoreFilePath)
        {
            // Read all lines which aren't comments or whitespace
            var lines = File.ReadAllLines(ignoreFilePath)
                            .Select(line => line.Trim())
                            .Where(line => line.Length > 0 && !line.StartsWith("#"))
                            .ToList();

            _rules = lines.Select(line => {
                var isExclude = !line.StartsWith("!");
                return new IgnoreRule {
                    Pattern = line,
                    Exclude = isExclude,
                    DirectoryOnly = line.EndsWith("/"),  // !line.Contains(".")
                    Glob = new Glob(isExclude ? line : line.TrimStart('!'))
                };
            });
        }

        public bool IsMatch(string input)
        {
            // .gitignore files use Unix paths (with a forward slash separator), so make sure our input also uses forward slashes
            input = input.Replace(Path.DirectorySeparatorChar.ToString(), "/").Trim();

            // This pattern modified from https://github.com/henon/GitSharp/blob/master/GitSharp/IgnoreRules.cs
            var ignore = false;

            foreach (var rule in _rules)
            {
                if (rule.Exclude != ignore && rule.Glob.IsMatch(input))
                {
                    ignore = rule.Exclude;
                }
            }

            return ignore;
        }
    }
}
