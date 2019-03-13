using System;
using System.IO;

namespace MAB.DotIgnore
{
    /// <summary>
    /// Utility functions.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Converts a path to a UNIX-style path (with forward-slash directory separators).
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>A UNIX-style path string.</returns>
        internal static string NormalisePath(this string path) =>
            path.Replace(":", string.Empty)
                .Replace(char.ToString(Path.DirectorySeparatorChar), "/")
                .Trim();

        /// <summary>
        /// Determines whether the beginning of this string instance matches the specified string.
        /// </summary>
        /// <param name="s">The string instance.</param>
        /// <param name="value">The string to compare.</param>
        /// <returns>true if value matches the beginning of this string; otherwise, false.</returns>
        internal static bool StartsWithCI(this string s, string value) =>
            s.StartsWith(value, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether the end of this string instance matches the specified string.
        /// </summary>
        /// <param name="s">The string instance.</param>
        /// <param name="value">The string to compare.</param>
        /// <returns>true if value matches the end of this string; otherwise, false.</returns>
        internal static bool EndsWithCI(this string s, string value) =>
            s.EndsWith(value, StringComparison.OrdinalIgnoreCase);
    }
}
