using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace IgnoreSharp
{
    /// <summary>
    /// Modified from the Glob class in the Unity Application Block
    /// https://github.com/unitycontainer/unity/blob/4fc7c789aecf4415db2688753dabe18421296222/source/Unity.Interception/Src/Utilities/Glob.cs
    /// </summary>
    public class Glob
    {
        private readonly Regex _pattern;

        /// <summary>
        /// Constructs a new <see cref="Glob"/> instance that matches the given pattern.
        /// </summary>
        /// <remarks>
        /// The pattern match is case sensitive by default.
        /// </remarks>
        /// <param name="pattern">Pattern to use. See <see cref="Glob"/> summary for
        /// details of the pattern.</param>
        public Glob(string pattern) : this(pattern, true) { }

        /// <summary>
        /// Constructs a new <see cref="Glob"/> instance that matches the given pattern.
        /// </summary>
        /// <param name="pattern">The pattern to use. See <see cref="Glob"/> summary for
        /// details of the patterns supported.</param>
        /// <param name="caseSensitive">If true, perform a case sensitive match. 
        /// If false, perform a case insensitive comparison.</param>
        public Glob(string pattern, bool caseSensitive)
        {
            _pattern = GlobPatternToRegex(pattern, caseSensitive);
        }

        /// <summary>
        /// Checks to see if the given string matches the pattern.
        /// </summary>
        /// <param name="s">String to check.</param>
        /// <returns>True if it matches, false if it doesn't.</returns>
        public bool IsMatch(string s)
        {
            return _pattern.IsMatch(s);
        }

        private static Regex GlobPatternToRegex(string pattern, bool caseSensitive)
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

            RegexOptions options = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            return new Regex(regexPattern.ToString(), options);
        }
    }
}
