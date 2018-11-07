using System;

namespace MAB.DotIgnore
{
    /// <summary>
    /// Informational flags telling us about a parsed pattern.
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
#pragma warning disable CA1707 // Identifiers should not contain underscores
        ABSOLUTE_PATH = 2,
#pragma warning restore CA1707 // Identifiers should not contain underscores

        /// <summary>
        /// Pattern should match only directories
        /// </summary>
        DIRECTORY = 4,
    }
}
