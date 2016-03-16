using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IgnoreSharp
{
    public class IgnoreList
    {
        private IEnumerable<IgnoreRule> _rules;

        public IgnoreList(IEnumerable<string> rules)
        {
            InitialiseRules(CleanRules(rules));
        }

        public IgnoreList(string ignoreFilePath)
        {
            InitialiseRules(CleanRules(File.ReadAllLines(ignoreFilePath)));
        }

        private IEnumerable<string> CleanRules(IEnumerable<string> rules)
        {
            // Exclude all comment or whitespace lines
            return rules.Select(line => line.Trim())
                        .Where(line => line.Length > 0 && !line.StartsWith("#", StringComparison.OrdinalIgnoreCase));
        }

        private void InitialiseRules(IEnumerable<string> rules)
        {
            _rules = rules.Select(line => new IgnoreRule(line));
        }

        public bool IsMatch(string input)
        {
            // .gitignore files use Unix paths (with a forward slash separator), so make sure our input also uses forward slashes
            input = input.Replace(Path.DirectorySeparatorChar.ToString(), "/").Trim();

            // This pattern modified from https://github.com/henon/GitSharp/blob/master/GitSharp/IgnoreRules.cs
            var ignore = false;

            foreach (var rule in _rules)
            {
                if (rule.Exclude != ignore && rule.IsMatch(input))
                {
                    ignore = rule.Exclude;
                }
            }

            return ignore;
        }
    }
}
