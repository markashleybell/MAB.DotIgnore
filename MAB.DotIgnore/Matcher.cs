﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MAB.DotIgnore
{
    public static class Matcher
    {
        private static readonly Regex CharClassRx = new Regex(@"\[:(?>[a-z]*?):\]", RegexOptions.Compiled);

        private static readonly Regex InvalidStarStarRx = new Regex(@"\*\*[^/\s]|[^/\s]\*\*", RegexOptions.Compiled);

        private static readonly Regex EscapedAlphaNumRx = new Regex(@"(?<!\\)\\([a-zA-Z0-9])", RegexOptions.Compiled);

        private static readonly string[] LiteralsToEscapeInRegex = new[] { ".", "$", "{", "}", "(", "|", ")", "+" };

        // Match POSIX char classes with .NET unicode equivalents
        // https://www.regular-expressions.info/posixbrackets.html
        private static readonly Dictionary<string, string> CharClassSubstitutions = 
            new Dictionary<string, string> {
                { "[:alnum:]", @"a-zA-Z0-9" },
                { "[:alpha:]", @"a-zA-Z" },
                { "[:blank:]", @"\p{Zs}\t" },
                { "[:cntrl:]", @"\p{Cc}" },
                { "[:digit:]", @"\d" },
                { "[:graph:]", @"^\p{Z}\p{C}" },
                { "[:lower:]", @"a-z" },
                { "[:print:]", @"\p{C}" },
                { "[:punct:]", @"\p{P}" },
                { "[:space:]", @"\s" },
                { "[:upper:]", @"A-Z" },
                { "[:xdigit:]", @"A-Fa-f0-9" }
            };

        public static bool TryMatch(Regex rx, string path)
        {
            if (rx == null)
            {
                return false;
            }

            try
            {
                return rx.IsMatch(path);
            }
            catch
            {
                return false;
            }
        }

        // https://git-scm.com/docs/gitignore#_pattern_format

        // FNM_PATHNAME
        // If this flag is set, match a slash in string only with a slash in pattern 
        // and not by an asterisk (*) or a question mark (?) metacharacter, nor by a 
        // bracket expression ([]) containing a slash.

        public static string GetRegex(string pattern, bool caseSensitive = true)
        {
            // Double-star is only valid:
            // - at the beginning of a pattern, immediately followed by a slash ('**/c')
            // - at the end of a pattern, immediately preceded by a slash ('a/**')
            // - anywhere in the pattern with a slash immediately before and after ('a/**/c')
            if (InvalidStarStarRx.IsMatch(pattern))
            {
                return null;
            }

            var charClasses = CharClassSubstitutions.Keys.ToArray();

            var patternCharClasses = CharClassRx.Matches(pattern).Cast<Match>().Select(m => m.Groups[0].Value);

            if (patternCharClasses.Any(pcc => !charClasses.Any(cc => cc == pcc)))
            {
                // Malformed character class
                return null;
            }

            // Remove single backslashes before alphanumeric chars 
            // (escaping these in a glob pattern should have no effect)
            pattern = EscapedAlphaNumRx.Replace(pattern, "$1");

            var rx = new StringBuilder(pattern);

            foreach (var literal in LiteralsToEscapeInRegex)
            {
                rx.Replace(literal, @"\" + literal);
            }

            foreach (var k in charClasses)
            {
                rx.Replace(k, CharClassSubstitutions[k]);
            }

            rx.Replace("!", "^");
            rx.Replace("**", "[:STARSTAR:]");
            rx.Replace("*", "[:STAR:]");
            rx.Replace("?", "[:QM:]");

            rx.Insert(0, "^");
            rx.Append("$");

            // Character class patterns shouldn't match slashes, so we prefix them with 
            // negative lookaheads. This is rather harder than it seems, because class 
            // patterns can also contain unescaped square brackets...

            // TODO: is this only true if PATHMATCH isn't specified?

            var rx2 = new StringBuilder(NonPathMatchCharClasses(rx.ToString()));

            // Non-escaped question mark should match any single char except slash
            rx2.Replace(@"\[:QM:]", @"\?");
            rx2.Replace(@"[:QM:]", @"[^/]");

            // Replace star patterns with equivalent regex patterns
            rx2.Replace(@"\[:STAR:]", @"\*");
            rx2.Replace(@"[:STAR:]", @"[^/]*");

            return Regex.Replace(rx2.ToString(), @"(?>\[:STARSTAR:\]/?)+", ".*");
        }

        private static string NonPathMatchCharClasses(string p)
        {
            var o = new StringBuilder();
            var inBrackets = false;

            for (var i = 0; i < p.Length;)
            {
                var escaped = i != 0 && p[i - 1] == '\\';

                if (p[i] == '[' && !escaped)
                {
                    if (inBrackets)
                    {
                        o.Append(@"\");
                    }
                    else
                    {
                        if (i < p.Length && p[i + 1] != ':')
                        {
                            o.Append(@"(?!/)");
                        }

                        inBrackets = true;
                    }
                }
                else if (p[i] == ']' && !escaped)
                {
                    if (inBrackets && p.IndexOf(']', i + 1) >= p.IndexOf('[', i + 1))
                    {
                        inBrackets = false;
                    }
                }

                o.Append(p[i++]);
            }

            return o.ToString();
        }
    }
}
