namespace MAB.DotIgnore
{
    /// <summary>
    /// Represents a line specifying a rule in an ignore file.
    /// </summary>
    internal struct RuleLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RuleLine"/> struct.
        /// </summary>
        /// <param name="lineNumber">The line number.</param>
        /// <param name="pattern">The ignore rule pattern.</param>
        public RuleLine(int? lineNumber, string pattern)
        {
            LineNumber = lineNumber;
            Pattern = pattern;
        }

        /// <summary>
        /// Gets the line number.
        /// </summary>
        public int? LineNumber { get; }

        /// <summary>
        /// Gets the ignore rule pattern.
        /// </summary>
        public string Pattern { get; }
    }
}
