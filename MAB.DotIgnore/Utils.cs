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
        internal static string NormalisePath(string path) =>
            path.Replace(":", string.Empty).Replace(char.ToString(Path.DirectorySeparatorChar), "/").Trim();
    }
}
