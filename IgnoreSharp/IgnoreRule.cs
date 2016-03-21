using System;
using System.Text;
using System.Text.RegularExpressions;

namespace IgnoreSharp
{
    public class IgnoreRule
    {
        public string Pattern { get; private set; }
        public bool Exclude { get; private set; }
        public bool DirectoryOnly { get; private set; }
        public Regex RegularExpression { get; private set; }

        public IgnoreRule(string pattern)
        {
            var isExclude = !pattern.StartsWith("!", StringComparison.OrdinalIgnoreCase);

            Pattern = pattern;
            Exclude = isExclude;
            DirectoryOnly = pattern.EndsWith("/", StringComparison.OrdinalIgnoreCase);
            RegularExpression = GlobPatternToRegex(isExclude ? pattern : pattern.TrimStart('!'), DirectoryOnly);
        }

        public bool IsMatch(string s)
        {
            return RegularExpression.IsMatch(s);
        }

        // Modified from the Glob class in the Unity Application Block
        // https://github.com/unitycontainer/unity/blob/4fc7c789aecf4415db2688753dabe18421296222/source/Unity.Interception/Src/Utilities/Glob.cs
        private Regex GlobPatternToRegex(string pattern, bool directoryOnly)
        {
            string[] globLiterals = { "\\", ".", "$", "^", "{", "(", "|", ")", "+" };

            foreach (string globLiteral in globLiterals)
            {
                pattern.Replace(globLiteral, @"\" + globLiteral);
            }

            if (!pattern.Contains("/"))
            {
                // If there are no path separators in the rule, treat asterisks the same way as double asterisks
                pattern = Regex.Replace(pattern, @"\*+", ".*");
            }
            else
            { 
                // Two consecutive asterisks should be equivalent to .*
                // Replace any occurrences with a placeholder, so that we can match single asterisks differently with the next pass
                pattern = Regex.Replace(pattern, @"\*\*", ".{@}");
                // Now replace single asterisks - should match everything except forward slashes
                pattern = Regex.Replace(pattern, @"\*", "[^/]+");
                // Now single asterisks have been dealt with, replace any double-asterisk placeholders with the correct regex pattern
                pattern = Regex.Replace(pattern, @"\.\{\@\}", ".*");
            }

            pattern = Regex.Replace(pattern, @"\?", ".");

            // Match start of line
            pattern = "^" + pattern;

            // If this is a directory ignore rule, we want to match anything which *starts* with the rule as well
            // as literal matches, so only add the end of line token if this is a file rule
            if(!directoryOnly)
                pattern = pattern + "$";

            // TODO: Is this supposed to be case-sensitive? 
            // Glob on Unix is case-sensitive, but running tests on Windows it seems that the casing of .gitignore rules makes no difference...
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }
    }
}
