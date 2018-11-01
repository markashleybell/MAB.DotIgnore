using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MAB.DotIgnore
{
    internal static class Matcher
    {
        public static bool IsMatch(string pattern, string path, bool caseSensitive = true)
        {
            var literalsToEscapeInRegex = new[] { ".", "$", "{", "}", "(", "|", ")", "+" };

            var charClassSubstitutions = new Dictionary<string, string> {
                { "[:alnum:]", @"a-zA-Z0-9" },
                { "[:alpha:]", @"a-zA-Z" },
                { "[:blank:]", @" \t" },
                { "[:cntrl:]", @"\x00-\x1F\x7F" },
                { "[:digit:]", @"0-9" },
                { "[:graph:]", @"\x21-\x7E" },
                { "[:lower:]", @"a-z" },
                { "[:print:]", @"\x20-\x7E" },
                // Note that this has twice the amount of backslashes before the escaped backslash
                // This is because we later replace \\ with \ when building the regex pattern
                { "[:punct:]", @"!""\#$%&'()*+,\-./:;<=>?@\[\\\]^_`{|}~" },
                { "[:space:]", @" \t\r\n\v\f" },
                { "[:upper:]", @"A-Z" },
                { "[:xdigit:]", @"A-Fa-f0-9" }
            };

            var charClasses = charClassSubstitutions.Keys.ToArray();

            var patternCharClasses = Regex.Matches(pattern, @"\[\:[a-z]+\:\]").Cast<Match>().Select(m => m.Groups[0].Value);

            if (patternCharClasses.Any(pcc => !charClasses.Any(cc => cc == pcc)))
            {
                // Malformed character class
                return false;
            }

            var rx = new StringBuilder(pattern);

            foreach(var literal in literalsToEscapeInRegex)
                rx.Replace(literal, @"\" + literal);

            foreach(var k in charClasses)
                rx.Replace(k, charClassSubstitutions[k]);

            rx.Replace("!", "^");

            rx.Insert(0, "^");
            rx.Append("$");

            var rxs = rx.ToString();

            // Remove single backslashes before alphanumeric chars (escaping these should have no effect)
            rxs = Regex.Replace(rxs, @"(?<!\\)\\([a-zA-Z0-9])", "$1");

            // Replace non-escaped star and question mark chars with equivalent regex patterns
            rxs = Regex.Replace(rxs, @"(?<!\\)\*", ".*");
            rxs = Regex.Replace(rxs, @"(?<!\\)\?", ".");

            try
            {
                return !caseSensitive
                    ? Regex.IsMatch(path, rxs, RegexOptions.IgnoreCase)
                    : Regex.IsMatch(path, rxs);
            }
            catch
            {
                return false;
            }
        }
    }
}
