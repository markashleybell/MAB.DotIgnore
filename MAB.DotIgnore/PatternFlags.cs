using System;

namespace MAB.DotIgnore
{
    /// <summary>
    /// Informational flags telling us about a parsed pattern
    /// </summary>
    [Flags]
    public enum PatternFlags 
    {
        /// <summary>
        /// Pattern is a basic match pattern with no wildcards and no leading/trailing slashes
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Pattern contains '*', '[', and/or '?'
        /// </summary>
        WILD = 1,
        /// <summary>
        /// Pattern contains '**'
        /// </summary>
        WILD2 = 2,
        /// <summary>
        /// Pattern starts with '**'
        /// </summary>
        WILD2_PREFIX = 4,
        /// <summary>
        /// Pattern starts with '/'
        /// </summary>
        ABSOLUTE_PATH = 8,
        /// <summary>
        /// Pattern should match only directories
        /// </summary>
        DIRECTORY = 16
    }
}
