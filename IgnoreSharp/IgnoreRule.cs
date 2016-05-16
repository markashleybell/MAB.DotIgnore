using System;
using System.Text;
using System.Text.RegularExpressions;

namespace IgnoreSharp
{
    public class IgnoreRule
    {
        private string _pattern;
        private MatchFlags _matchFlags;
        private PatternFlags _patternFlags;

        public string Pattern { get { return _pattern; } }
        public MatchFlags MatchFlags { get { return _matchFlags; } }
        public PatternFlags PatternFlags { get { return _patternFlags; } }

        public IgnoreRule(string pattern, MatchFlags matchFlags = MatchFlags.IGNORE_CASE | MatchFlags.PATHNAME)
        {
            if(Utils.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException("pattern");

            _pattern = pattern;
            _matchFlags = matchFlags;
        }

        //    PATTERN FORMAT
        //    
        //    - A blank line matches no files, so it can serve as a separator for readability.
        //    
        //    - A line starting with # serves as a comment. Put a backslash ("\") in front of the first 
        //      hash for patterns that begin with a hash.
        //    
        //    - Trailing spaces are ignored unless they are quoted with backslash("\").
        //    
        //    - An optional prefix "!" which negates the pattern; any matching file excluded by a previous 
        //      pattern will become included again. It is not possible to re-include a file if a parent 
        //      directory of that file is excluded. Git doesn’t list excluded directories for performance 
        //      reasons, so any patterns on contained files have no effect, no matter where they are defined.
        //      Put a backslash("\") in front of the first "!" for patterns that begin with a literal "!", 
        //      for example: "\!important!.txt".
        //    
        //    - If the pattern ends with a slash, it is removed for the purpose of the following description, 
        //      but it would only find a match with a directory. In other words, foo/ will match a directory 
        //      foo and paths underneath it, but will not match a regular file or a symbolic link foo (this is 
        //      consistent with the way how pathspec works in general in Git).
        //    
        //    - If the pattern does not contain a slash /, Git treats it as a shell glob pattern and checks 
        //      for a match against the pathname relative to the location of the.gitignore file (relative to
        //      the toplevel of the work tree if not from a.gitignore file).
        //    
        //    - Otherwise, Git treats the pattern as a shell glob suitable for consumption by fnmatch(3) with 
        //      the FNM_PATHNAME flag: wildcards in the pattern will not match a / in the pathname.
        //      For example, "Documentation/*.html" matches "Documentation/git.html" but not 
        //      "Documentation/ppc/ppc.html" or "tools/perf/Documentation/perf.html".
        //    
        //    - A leading slash matches the beginning of the pathname.
        //      For example, "/*.c" matches "cat-file.c" but not "mozilla-sha1/sha1.c".
        //    
        //    Two consecutive asterisks("**") in patterns matched against full pathname may have special meaning:
        //    
        //    - A leading "**" followed by a slash means match in all directories.For example, "**/foo" 
        //      matches file or directory "foo" anywhere, the same as pattern "foo". "**/foo/bar" matches 
        //      file or directory "bar" anywhere that is directly under directory "foo".
        //    
        //    - A trailing "/**" matches everything inside.For example, "abc/**" matches all files inside 
        //      directory "abc", relative to the location of the.gitignore file, with infinite depth.
        //    
        //    - A slash followed by two consecutive asterisks then a slash matches zero or more directories.
        //      For example, "a/**/b" matches "a/b", "a/x/b", "a/x/y/b" and so on.
        //    
        //    - Other consecutive asterisks are considered invalid.

        public bool IsMatch(string path, bool pathIsDirectory)
        {
            // .gitignore files use Unix paths (with a forward slash separator), so make sure our input also uses forward slashes
            // path = path.Replace(Path.DirectorySeparatorChar.ToString(), "/").Trim();

            // If the pattern or the path are null empty, there is nothing to match
            if(Utils.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException("path");

            // First, let's figure out some things about the pattern and set flags to pass to our match function
    
            _patternFlags = PatternFlags.NONE;
    
            // If the pattern starts with an exclamation mark, it's a negation pattern
            // Once we know that, we can remove the exclamation mark (so the pattern behaves just like any other),
            // then just negate the match result when we return it
            var negation = _pattern.StartsWith("!");

            if (negation)
                _pattern = _pattern.Substring(1);

            // If the pattern starts with a forward slash, it should only match an absolute path
            if (_pattern.StartsWith("/"))
                _patternFlags |= PatternFlags.ABSOLUTE_PATH;
    
            // If the pattern ends with a forward slash, it should only match a directory
            // Again though, once we know that we can remove the slash to normalise the pattern
            if (_pattern.EndsWith("/"))
            {
                _patternFlags |= PatternFlags.DIRECTORY;
                _pattern = _pattern.Substring(0, _pattern.Length - 1);
            }

            var wildcardIndex = _pattern.IndexOfAny(new char[] { '*','[','?' });
    
            // Set some more pattern flags depending on which glob wildcards appear in the pattern, and where
            if (wildcardIndex != -1)
            {
                _patternFlags |= PatternFlags.WILD;
    
                if (_pattern.Contains("**"))
                {
                    _patternFlags |= PatternFlags.WILD2;
            
                    if (_pattern.StartsWith("**"))
                        _patternFlags |= PatternFlags.WILD2_PREFIX;
                }
            }
    
            //pattern.Dump();
            //_patternFlags.Dump();

            // If we're passing IGNORE_CASE (equivalent to WM_CASEFOLD in wildmatch.c)
            // then uppercase both the pattern and the path
            if (_matchFlags.HasFlag(MatchFlags.IGNORE_CASE))
            {
                _pattern = _pattern.ToUpperInvariant();
                path = path.ToUpperInvariant();
            }

            // If PATHNAME is set, a single asterisk should not match forward slashes
            // If not, a single asterisk in the pattern becomes equivalent to **
            var singleAsteriskMatchesSlashes = !_matchFlags.HasFlag(MatchFlags.PATHNAME);
    
            // If the pattern is an absolute path pattern, the path must start with the part of the pattern
            // before any wildcards occur. If it doesn't, we can just return a negative match
            var patternBeforeFirstWildcard = wildcardIndex != -1 ? _pattern.Substring(0, wildcardIndex) : _pattern;
    
            if (_patternFlags.HasFlag(PatternFlags.ABSOLUTE_PATH) && !path.StartsWith(patternBeforeFirstWildcard))
                return !negation;

            // If the pattern is *not* an absolute path pattern and there are no wildcards in the pattern, 
            // then we know that the path must actually end with the pattern in order to be a match
            if (!_patternFlags.HasFlag(PatternFlags.ABSOLUTE_PATH) && wildcardIndex == -1)
            {
                var endMatch = path.EndsWith(_pattern);
                return negation != endMatch;
            }
    
            // Shortcut return if the pattern is a directory-only pattern and the path isn't a directory
            // This has to be determined by the OS (at least that's the only reliable way), so we pass
            // that information in as a boolean so the consumer can provide it
            if (_patternFlags.HasFlag(PatternFlags.DIRECTORY) && !pathIsDirectory)
                return negation ? true : false;

            var match = Match(_pattern.ToCharArray(), 0, path.ToCharArray(), 0, singleAsteriskMatchesSlashes);
    
            var isMatch = negation != match;
    
            //(isMatch ? "MATCHED" : "NOT MATCHED").Dump();
            //"--------------".Dump();
    
            return isMatch;
        }

        public bool Match(char[] pattern, int patternIndex, char[] text, int textIndex, bool singleAsteriskMatchesSlashes)
        {
            // Iterate over the characters of the pattern and the text in parallel
            for (int p = patternIndex, t = textIndex; p < pattern.Length; p++, t++)
            {
                char pchar = pattern[p];
                char tchar = text[t];

                switch (pchar)
                {
                    case '\\':
                        // Literal match with following character
                        pchar = pattern[++p];
                        goto default;
                    default:
                        // If the text character doesn't match, 
                        if (tchar != pchar)
                            return false;
                        continue;
                    case '*':
                        continue;
                }
            }
    
            return true;
        }

        public override string ToString()
        {
            return string.Format("{0}", Pattern);
        }
    }
}
