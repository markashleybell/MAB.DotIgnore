using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MAB.DotIgnore
{
    /// <summary>
    /// A rule which can be used to determine whether a file path should be ignored
    /// </summary>
    public class IgnoreRule
    {
        private int _wildcardIndex;

        private StringComparison sc = StringComparison.Ordinal;

        /// <summary>
        /// The original pattern string passed into the constructor
        /// </summary>
        public string OriginalPattern { get; private set; }
        /// <summary>
        /// The pre-processed pattern string after basic parsing
        /// </summary>
        public string Pattern { get; private set; }
        /// <summary>
        /// The <see cref="MatchFlags"/> set for this rule
        /// </summary>
        public MatchFlags MatchFlags { get; private set; }
        /// <summary>
        /// The <see cref="PatternFlags"/> set for the parsed rule pattern
        /// </summary>
        public PatternFlags PatternFlags { get; private set; }

        /// <summary>
        /// Create an individual ignore rule for the specified pattern
        /// </summary>
        /// <param name="pattern">A glob pattern specifying file(s) this rule should ignore</param>
        public IgnoreRule(string pattern)
        {
            if(Utils.IsNullOrWhiteSpace(pattern))
                throw new ArgumentNullException(nameof(pattern));
            
            // Keep track of the original pattern before modifications (for display purposes)
            OriginalPattern = pattern;
            Pattern = pattern;
            MatchFlags = MatchFlags.NONE;
            
            // First, let's figure out some things about the pattern and set flags to pass to our match function
            PatternFlags = PatternFlags.NONE;
    
            // If the pattern starts with an exclamation mark, it's a negation pattern
            // Once we know that, we can remove the exclamation mark (so the pattern behaves just like any other),
            // then just negate the match result when we return it
            if(Pattern.StartsWith("!", sc))
            { 
                PatternFlags |= PatternFlags.NEGATION;
                Pattern = Pattern.Substring(1);
            }

            // If the pattern starts with a forward slash, it should only match an absolute path
            if (Pattern.StartsWith("/", sc))
            { 
                PatternFlags |= PatternFlags.ABSOLUTE_PATH;
                Pattern = Pattern.Substring(1);
            }
    
            // If the pattern ends with a forward slash, it should only match a directory
            // Again though, once we know that we can remove the slash to normalise the pattern
            if (Pattern.EndsWith("/", sc))
            {
                PatternFlags |= PatternFlags.DIRECTORY;
                Pattern = Pattern.Substring(0, Pattern.Length - 1);
            }

            _wildcardIndex = Pattern.IndexOfAny(new char[] { '*','[','?' });
            
            // If CASEFOLD is set, string comparisons should ignore case too
            if (MatchFlags.HasFlag(MatchFlags.CASEFOLD))
                sc = StringComparison.OrdinalIgnoreCase;

            // TODO: Currently, we are just setting PATHNAME for every rule, because it seems to match the original behaviour
            // See here for a clue: https://github.com/git/git/blob/c2c5f6b1e479f2c38e0e01345350620944e3527f/dir.c#L99

            // If PATHNAME is set, single asterisks should not match slashes
            MatchFlags |= MatchFlags.PATHNAME;
        }

        // PATTERN FORMAT
        // 
        // - A blank line matches no files, so it can serve as a separator for readability.
        // 
        // - A line starting with # serves as a comment. Put a backslash ("\") in front of the first 
        //   hash for patterns that begin with a hash.
        // 
        // - Trailing spaces are ignored unless they are quoted with backslash("\").
        // 
        // - An optional prefix "!" which negates the pattern; any matching file excluded by a previous 
        //   pattern will become included again. It is not possible to re-include a file if a parent 
        //   directory of that file is excluded. Git doesn’t list excluded directories for performance 
        //   reasons, so any patterns on contained files have no effect, no matter where they are defined.
        //   Put a backslash("\") in front of the first "!" for patterns that begin with a literal "!", 
        //   for example: "\!important!.txt".
        // 
        // - If the pattern ends with a slash, it is removed for the purpose of the following description, 
        //   but it would only find a match with a directory. In other words, foo/ will match a directory 
        //   foo and paths underneath it, but will not match a regular file or a symbolic link foo (this is 
        //   consistent with the way how pathspec works in general in Git).
        // 
        // - If the pattern does not contain a slash /, Git treats it as a shell glob pattern and checks 
        //   for a match against the pathname relative to the location of the.gitignore file (relative to
        //   the toplevel of the work tree if not from a.gitignore file).
        // 
        // - Otherwise, Git treats the pattern as a shell glob suitable for consumption by fnmatch(3) with 
        //   the FNM_PATHNAME flag: wildcards in the pattern will not match a / in the pathname.
        //   For example, "Documentation/*.html" matches "Documentation/git.html" but not 
        //   "Documentation/ppc/ppc.html" or "tools/perf/Documentation/perf.html".
        // 
        // - A leading slash matches the beginning of the pathname.
        //   For example, "/*.c" matches "cat-file.c" but not "mozilla-sha1/sha1.c".
        // 
        // Two consecutive asterisks("**") in patterns matched against full pathname may have special meaning:
        // 
        // - A leading "**" followed by a slash means match in all directories.For example, "**/foo" 
        //   matches file or directory "foo" anywhere, the same as pattern "foo". "**/foo/bar" matches 
        //   file or directory "bar" anywhere that is directly under directory "foo".
        // 
        // - A trailing "/**" matches everything inside.For example, "abc/**" matches all files inside 
        //   directory "abc", relative to the location of the.gitignore file, with infinite depth.
        // 
        // - A slash followed by two consecutive asterisks then a slash matches zero or more directories.
        //   For example, "a/**/b" matches "a/b", "a/x/b", "a/x/y/b" and so on.
        // 
        // - Other consecutive asterisks are considered invalid.

        /// <summary>
        /// Check if a file path matches the rule glob pattern
        /// </summary>
        /// <param name="file">FileInfo representing the file to check</param>
        public bool IsMatch(FileInfo file)
        {
            return IsMatch(file.FullName, false);
        }

        /// <summary>
        /// Check if a directory path matches the rule glob pattern
        /// </summary>
        /// <param name="directory">DirectoryInfo representing the directory to check</param>
        public bool IsMatch(DirectoryInfo directory)
        {
            return IsMatch(directory.FullName, true);
        }

        /// <summary>
        /// Check if a path matches the rule glob pattern
        /// </summary>
        /// <param name="path">String representing the path to check</param>
        /// <param name="pathIsDirectory">Should be set True if the path represents a directory, False if it represents a file</param>
        public bool IsMatch(string path, bool pathIsDirectory)
        {
            // If the pattern or the path are null empty, there is nothing to match
            if(Utils.IsNullOrWhiteSpace(path))
                throw new ArgumentNullException(nameof(path));

            // .gitignore files use Unix paths (with a forward slash separator), so make sure our input also uses forward slashes
            path = NormalisePath(path);

            path = path.TrimStart('/');

            // Shortcut return if the pattern is directory-only and the path isn't a directory
            // This has to be determined by the OS (at least that's the only reliable way), 
            // so we pass that information in as a boolean so the consuming code can provide it
            if (PatternFlags.HasFlag(PatternFlags.DIRECTORY) && pathIsDirectory == false)
                return false;

            // If the pattern is an absolute path pattern, the path must start with the part of the pattern
            // before any wildcards occur. If it doesn't, we can just return a negative match
            var patternBeforeFirstWildcard = _wildcardIndex != -1 ? Pattern.Substring(0, _wildcardIndex) : Pattern;
    
            if (PatternFlags.HasFlag(PatternFlags.ABSOLUTE_PATH) && !path.StartsWith(patternBeforeFirstWildcard, sc))
                return false;

            // If the pattern is *not* an absolute path pattern and there are no wildcards in the pattern, 
            // then we know that the path must actually end with the pattern in order to be a match
            if (!PatternFlags.HasFlag(PatternFlags.ABSOLUTE_PATH) && _wildcardIndex == -1)
                return path.EndsWith(Pattern, sc);

            // If we got this far, we can't figure out the match with simple string matching, so use our wildmatch implementation
            
            // If the pattern does not contain any slashes it should match *any* occurence, *anywhere* within the path 
            // (e.g. '*.jpg' should match 'a.jpg', 'a/b.jpg', 'a/b/c.jpg'), so try matching before each slash
            if (!Pattern.Contains("/") && path.Contains("/"))
            {
                string[] segments = path.Split('/');

                foreach(var segment in segments)
                {
                    if(WildMatch.IsMatch(Pattern, segment, MatchFlags) == WildMatch.MATCH)
                        return true;
                }

                return false;
            }

            // If the *path* doesn't contain any slashes, we should skip over the conditional above
            return WildMatch.IsMatch(Pattern, path, MatchFlags) == WildMatch.MATCH;
        }

        /// <summary>
        /// Return a string representation showing the original pattern, 
        /// the pre-processed pattern and the regular expression pattern
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} > {1}", OriginalPattern, Pattern);
        }

        private string NormalisePath(string path)
        {
            return path.Replace(":", "").Replace(Path.DirectorySeparatorChar.ToString(), "/").Trim();
        }
    }
}
