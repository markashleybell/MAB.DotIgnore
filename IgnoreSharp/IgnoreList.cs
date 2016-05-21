using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace IgnoreSharp
{
    public class IgnoreList
    {
        private List<IgnoreRule> _rules = new List<IgnoreRule>();

        public ReadOnlyCollection<IgnoreRule> Rules { get { return _rules.AsReadOnly(); } }

        public IgnoreList(IEnumerable<string> rules)
        {
            AddRules(rules);
        }

        public IgnoreList(string ignoreFilePath)
        {
            AddRules(File.ReadAllLines(ignoreFilePath));
        }

        public void AddRule(string rule)
        {
            AddRules(new string[] { rule });
        }

        public void AddRules(string ignoreFilePath)
        {
            _rules.AddRange(CleanRules(File.ReadAllLines(ignoreFilePath)).Select(line => new IgnoreRule(line)));
        }

        public void AddRules(IEnumerable<string> rules)
        {
            _rules.AddRange(CleanRules(rules).Select(line => new IgnoreRule(line)));
        }

        public void RemoveRule(string rule)
        {
            _rules.RemoveAll(r => r.OriginalPattern == rule.Trim());
        }

        private IEnumerable<string> CleanRules(IEnumerable<string> rules)
        {
            // Exclude all comment or whitespace lines
            return rules.Select(line => line.Trim())
                        .Where(line => line.Length > 0 && !line.StartsWith("#", StringComparison.OrdinalIgnoreCase));
        }

        public bool IsMatch(string input)
        {
            return IsMatch(input, null);
        }

        public bool IsMatch(string input, List<string> log)
        {
            // .gitignore files use Unix paths (with a forward slash separator), so make sure our input also uses forward slashes
            input = input.Replace(Path.DirectorySeparatorChar.ToString(), "/").Trim();

            // This pattern modified from https://github.com/henon/GitSharp/blob/master/GitSharp/IgnoreRules.cs
            var ignore = false;

            foreach (var rule in _rules)
            {
                if (rule.Exclude != ignore)
                {
                    ignore = rule.IsMatch(input, true);

                    if (log != null)
                    {
                        log.Add(string.Format("{0} by {1}", (rule.Exclude ? "Ignored" : "Included"), rule.OriginalPattern));
                    }
                }
            }

            return ignore;
        }

        public IgnoreList Clone()
        {
            return new IgnoreList(_rules.Select(x => x.Pattern));
        }
    }
}
