using System;

namespace MAB.DotIgnore
{
    /// <summary>
    /// A set of flags determining the behaviour of ignore rule matches
    /// </summary>
    [Flags]
    public enum MatchFlags
    {
        /// <summary>
        /// Patterns are case-sensitive, single asterisks in patterns match path slashes
        /// </summary>
        NONE = 0,
        /// <summary>
        /// If set, pattern matches are case-insensitive
        /// </summary>
        CASEFOLD = 1,
        /// <summary>
        /// If set, single asterisks in patterns should not match path slashes
        /// </summary>
        PATHNAME = 2    
    }
}
