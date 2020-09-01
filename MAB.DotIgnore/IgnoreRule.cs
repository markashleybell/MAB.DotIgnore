using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MAB.DotIgnore
{
    /// <summary>
    /// A rule which can be used to determine whether a file path should be ignored.
    /// </summary>
    public class IgnoreRule
    {
        private static readonly char[] _wildcardChars = new char[] { '*', '[', '?' };

        private readonly int _wildcardIndex;
        private readonly Regex _rx;
        private readonly StringComparison _sc = StringComparison.Ordinal;

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreRule"/> class.
        /// </summary>
        /// <param name="pattern">A glob pattern specifying file(s) this rule should ignore.</param>
        /// <param name="flags">Optional flags determining pattern matching behaviour.</param>
        /// <param name="lineNumber">Optional line number for logging purposes.</param>
        public IgnoreRule(string pattern, MatchFlags flags = MatchFlags.PATHNAME, int? lineNumber = null)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));
            }

            LineNumber = lineNumber;

            // Keep track of the original pattern before modifications (for display purposes)
            OriginalPattern = pattern;
            Pattern = pattern;
            MatchFlags = flags;

            // First, let's figure out some things about the pattern and set flags to pass to our match function
            PatternFlags = PatternFlags.NONE;

            // If the pattern starts with an exclamation mark, it's a negation pattern
            // Once we know that, we can remove the exclamation mark (so the pattern behaves
            // just like any other), then just negate the match result when we return it
            if (Pattern.StartsWithCI("!"))
            {
                PatternFlags |= PatternFlags.NEGATION;
                Pattern = Pattern.Substring(1);
            }

            // If the pattern starts with a forward slash, it should only match an absolute path
            if (Pattern.StartsWithCI("/"))
            {
                PatternFlags |= PatternFlags.ABSOLUTE_PATH;
                Pattern = Pattern.Substring(1);
            }

            // If the pattern ends with a forward slash, it should only match a directory
            // Again though, once we know that we can remove the slash to normalise the pattern
            if (Pattern.EndsWithCI("/"))
            {
                PatternFlags |= PatternFlags.DIRECTORY;
                Pattern = Pattern.Substring(0, Pattern.Length - 1);
            }

            _wildcardIndex = Pattern.IndexOfAny(_wildcardChars);

            // If CASEFOLD is set, string comparisons should ignore case too
            if ((MatchFlags & MatchFlags.CASEFOLD) != 0)
            {
                _sc = StringComparison.OrdinalIgnoreCase;
            }

            // TODO: Currently, we are just setting PATHNAME for every rule
            // This is because it seems to match the original behaviour:
            // https://github.com/git/git/blob/c2c5f6b1e479f2c38e0e01345350620944e3527f/dir.c#L99

            // If PATHNAME is set, single asterisks should not match slashes
            if ((MatchFlags & MatchFlags.PATHNAME) == 0)
            {
                MatchFlags |= MatchFlags.PATHNAME;
            }

            var rxPattern = Matcher.ToRegex(Pattern);

            // If rxPattern is null, an invalid pattern was passed to ToRegex, so it cannot match
            if (!string.IsNullOrEmpty(rxPattern))
            {
                var rxOptions = RegexOptions.Compiled;

                if ((MatchFlags & MatchFlags.CASEFOLD) != 0)
                {
                    rxOptions |= RegexOptions.IgnoreCase;
                }

                _rx = new Regex(rxPattern, rxOptions);
            }
        }

        /// <summary>
        /// Gets the <see cref="MatchFlags"/> set for this rule.
        /// </summary>
        public MatchFlags MatchFlags { get; }

        /// <summary>
        /// Gets the original pattern string passed into the constructor.
        /// </summary>
        public string OriginalPattern { get; }

        /// <summary>
        /// Gets the pre-processed pattern string after basic parsing.
        /// </summary>
        public string Pattern { get; }

        /// <summary>
        /// Gets the <see cref="PatternFlags"/> set for the parsed rule pattern.
        /// </summary>
        public PatternFlags PatternFlags { get; }

        /// <summary>
        /// Gets or sets the line number of the pattern if it was loaded from a file.
        /// </summary>
        internal int? LineNumber { get; set; }

        /// <summary>
        /// Check if a file path matches the rule pattern.
        /// </summary>
        /// <param name="file">FileInfo representing the file to check.</param>
        /// <returns>True if the file path matches the rule pattern.</returns>
        public bool IsMatch(FileInfo file) =>
            IsMatch(file.FullName, false);

        /// <summary>
        /// Check if a directory path matches the rule pattern.
        /// </summary>
        /// <param name="directory">DirectoryInfo representing the directory to check.</param>
        /// <returns>True if the directory path matches the rule pattern.</returns>
        public bool IsMatch(DirectoryInfo directory) =>
            IsMatch(directory.FullName, true);

        /// <summary>
        /// Check if a path matches the rule pattern.
        /// </summary>
        /// <param name="path">String representing the path to check.</param>
        /// <param name="pathIsDirectory">Should be set True if the path represents
        /// a directory, False if it represents a file.</param>
        /// <returns>True if the file or directory path matches the rule pattern.</returns>
        public bool IsMatch(string path, bool pathIsDirectory)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            }

            // .gitignore files use Unix paths (with a forward slash separator),
            // so make sure our input also uses forward slashes
            path = path.NormalisePath().TrimStart('/');

            // Shortcut return if the pattern is directory-only and the path isn't a directory
            // This has to be determined by the OS (at least that's the only reliable way),
            // so we pass that information in as a boolean so the consuming code can provide it
            if ((PatternFlags & PatternFlags.DIRECTORY) != 0 && !pathIsDirectory)
            {
                return false;
            }

            // If the pattern is an absolute path pattern, the path must start with the part of the pattern
            // before any wildcards occur. If it doesn't, we can just return a negative match
            var patternBeforeFirstWildcard = _wildcardIndex != -1
                ? Pattern.Substring(0, _wildcardIndex)
                : Pattern;

            if ((PatternFlags & PatternFlags.ABSOLUTE_PATH) != 0
                && !path.StartsWith(patternBeforeFirstWildcard, _sc))
            {
                return false;
            }

            // If we got this far, we can't figure out the match with simple
            // string matching, so use our regex match function

            // If the *pattern* does not contain any slashes, it should match *any*
            // occurence, *anywhere* within the path (e.g. '*.jpg' should match
            // 'a.jpg', 'a/b.jpg', 'a/b/c.jpg'), so try matching before each slash
            if (!Pattern.Contains("/") && path.Contains("/"))
            {
                return path.Split('/').Any(segment => Matcher.TryMatch(_rx, segment));
            }

            // If the *path* doesn't contain any slashes, we should skip over the conditional above
            return Matcher.TryMatch(_rx, path);
        }

        /// <summary>
        /// Gets a string representation showing the original pattern
        /// (plus the line number if present).
        /// </summary>
        /// <returns>The original pattern, plus the line number if present.</returns>
        public override string ToString()
        {
            var lineNumber = LineNumber.HasValue
                ? $" (line {LineNumber.Value})"
                : string.Empty;

            return $"{OriginalPattern}{lineNumber}";
        }
    }
}
