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
        /// Pattern is a basic pattern
        /// </summary>
        NONE = 0,
        /// <summary>
        /// Pattern starts with '!'
        /// </summary>
        NEGATION = 1,
        /// <summary>
        /// Pattern starts with '/'
        /// </summary>
        ABSOLUTE_PATH = 2,
        /// <summary>
        /// Pattern should match only directories
        /// </summary>
        DIRECTORY = 4
    }
}
