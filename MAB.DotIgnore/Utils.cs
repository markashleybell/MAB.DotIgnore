using System;
using System.IO;

namespace MAB.DotIgnore
{
    /// <summary>
    /// Re-implements a couple of convenience functions introduced in .NET 4, so we can support 3.5.
    /// </summary>
    internal static class Utils
    {
        /// <summary>
        /// Check if a string consists solely of white space characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string consists solely of white space characters.</returns>
        internal static bool IsWhiteSpace(string value)
        {
            if (value == null)
            {
                return false;
            }

            for (var i = 0; i < value.Length; i++)
            {
                if (!char.IsWhiteSpace(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Check if a string is null or consists solely of white space characters.
        /// </summary>
        /// <param name="value">The string to check.</param>
        /// <returns>True if the string is null or consists solely of white space characters.</returns>
        internal static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null)
            {
                return true;
            }

            return IsWhiteSpace(value);
        }

        /// <summary>
        /// Check if an enum variable value has the specified flag.
        /// </summary>
        /// <param name="variable">The variable value to check.</param>
        /// <param name="value">The flag value to check for.</param>
        /// <returns>True if the variable value contains the specified flag.</returns>
        internal static bool HasFlag(this Enum variable, Enum value)
        {
            var num = Convert.ToUInt64(value, null);

            return (Convert.ToUInt64(variable, null) & num) == num;
        }

        /// <summary>
        /// Converts a path to a UNIX-style path (with forward-slash directory separators).
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>A UNIX-style path string.</returns>
        internal static string NormalisePath(string path) =>
            path.Replace(":", string.Empty).Replace(char.ToString(Path.DirectorySeparatorChar), "/").Trim();
    }
}
