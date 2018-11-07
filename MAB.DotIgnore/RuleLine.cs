namespace MAB.DotIgnore
{
    /// <summary>
    /// Represents a line specifying a rule in an ignore file.
    /// </summary>
    internal class RuleLine
    {
        /// <summary>
        /// Gets or sets the line number.
        /// </summary>
        public int? LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the ignore rule pattern.
        /// </summary>
        public string Pattern { get; set; }
    }
}
