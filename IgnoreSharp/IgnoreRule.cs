using System;
using System.Text;
using System.Text.RegularExpressions;

namespace IgnoreSharp
{
    public class IgnoreRule
    {
        private bool _negation;
        private bool _singleAsteriskMatchesSlashes;
        private int _wildcardIndex;

        private StringComparison sc = StringComparison.OrdinalIgnoreCase;

        public string Pattern { get; private set; }
        public MatchFlags MatchFlags { get; private set; }
        public PatternFlags PatternFlags { get; private set; }

        public IgnoreRule(string pattern, MatchFlags matchFlags = MatchFlags.IGNORE_CASE | MatchFlags.PATHNAME)
        {
            if(Utils.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException(nameof(pattern));
            
            Pattern = pattern;
            MatchFlags = matchFlags;

            // If PATHNAME is set, a single asterisk should not match forward slashes
            // If not, a single asterisk in the pattern becomes equivalent to **
            _singleAsteriskMatchesSlashes = !MatchFlags.HasFlag(MatchFlags.PATHNAME);

            // First, let's figure out some things about the pattern and set flags to pass to our match function
            PatternFlags = PatternFlags.NONE;
    
            // If the pattern starts with an exclamation mark, it's a negation pattern
            // Once we know that, we can remove the exclamation mark (so the pattern behaves just like any other),
            // then just negate the match result when we return it
            _negation = Pattern.StartsWith("!", sc);

            if (_negation)
                Pattern = Pattern.Substring(1);

            // If the pattern starts with a forward slash, it should only match an absolute path
            if (Pattern.StartsWith("/", sc))
                PatternFlags |= PatternFlags.ABSOLUTE_PATH;
    
            // If the pattern ends with a forward slash, it should only match a directory
            // Again though, once we know that we can remove the slash to normalise the pattern
            if (Pattern.EndsWith("/", sc))
            {
                PatternFlags |= PatternFlags.DIRECTORY;
                Pattern = Pattern.Substring(0, Pattern.Length - 1);
            }

            _wildcardIndex = Pattern.IndexOfAny(new char[] { '*','[','?' });
    
            // Set some more pattern flags depending on which glob wildcards appear in the pattern, and where
            if (_wildcardIndex != -1)
            {
                PatternFlags |= PatternFlags.WILD;
    
                if (Pattern.Contains("**"))
                {
                    PatternFlags |= PatternFlags.WILD2;
            
                    if (Pattern.StartsWith("**", sc))
                        PatternFlags |= PatternFlags.WILD2_PREFIX;
                }
            }

            // If we're passing IGNORE_CASE, uppercase the pattern
            if (MatchFlags.HasFlag(MatchFlags.IGNORE_CASE))
                Pattern = Pattern.ToUpperInvariant();
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
                throw new ArgumentNullException(nameof(path));
            
            // Shortcut return if the pattern is directory-only and the path isn't a directory
            // This has to be determined by the OS (at least that's the only reliable way), 
            // so we pass that information in as a boolean so the consuming code can provide it
            if (PatternFlags.HasFlag(PatternFlags.DIRECTORY) && pathIsDirectory == false)
                return _negation != false;

            // If we're passing IGNORE_CASE, uppercase the pattern
            if (MatchFlags.HasFlag(MatchFlags.IGNORE_CASE))
                path = path.ToUpperInvariant();

            // If the pattern is an absolute path pattern, the path must start with the part of the pattern
            // before any wildcards occur. If it doesn't, we can just return a negative match
            var patternBeforeFirstWildcard = _wildcardIndex != -1 ? Pattern.Substring(0, _wildcardIndex) : Pattern;
    
            if (PatternFlags.HasFlag(PatternFlags.ABSOLUTE_PATH) && !path.StartsWith(patternBeforeFirstWildcard, sc))
                return _negation != false;

            // If the pattern is *not* an absolute path pattern and there are no wildcards in the pattern, 
            // then we know that the path must actually end with the pattern in order to be a match
            if (!PatternFlags.HasFlag(PatternFlags.ABSOLUTE_PATH) && _wildcardIndex == -1)
                return _negation != path.EndsWith(Pattern, sc);
    
            var match = Match(Pattern.ToCharArray(), 0, path.ToCharArray(), 0, _singleAsteriskMatchesSlashes);
    
            var isMatch = _negation != match;
    
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
