using System;
using System.Collections.Generic;
using System.Linq;
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
            var isExclude = !pattern.StartsWith("!");

            Pattern = pattern;
            Exclude = isExclude;
            DirectoryOnly = pattern.EndsWith("/");  // !line.Contains(".")
            RegularExpression = GlobPatternToRegex(isExclude ? pattern : pattern.TrimStart('!'));
        }

        public bool IsMatch(string s)
        {
            return RegularExpression.IsMatch(s);
        }

        // Modified from the Glob class in the Unity Application Block
        // https://github.com/unitycontainer/unity/blob/4fc7c789aecf4415db2688753dabe18421296222/source/Unity.Interception/Src/Utilities/Glob.cs
        private Regex GlobPatternToRegex(string pattern)
        {
            StringBuilder regexPattern = new StringBuilder(pattern);

            string[] globLiterals = new string[] { "\\", ".", "$", "^", "{", "(", "|", ")", "+" };
            foreach (string globLiteral in globLiterals)
            {
                regexPattern.Replace(globLiteral, @"\" + globLiteral);
            }
            regexPattern.Replace("*", ".*");
            regexPattern.Replace("?", ".");

            //regexPattern.Insert(0, "^");
            //regexPattern.Append("$");

            // TODO: Is this supposed to be case-sensitive? 
            // Glob on Unix is case-sensitive, but running tests on Windows it seems that the casing of .gitignore rules makes no difference...
            return new Regex(regexPattern.ToString(), RegexOptions.IgnoreCase);
        }
    }
}
