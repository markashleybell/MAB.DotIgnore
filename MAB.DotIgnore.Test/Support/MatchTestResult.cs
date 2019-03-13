namespace MAB.DotIgnore.Test.Support
{
    internal class MatchTestResult
    {
        public int LineNumber { get; set; }

        public string Path { get; set; }

        public string Pattern { get; set; }

        public string Regex { get; set; }

        public bool Result { get; set; }

        public bool ResultCI { get; set; }
    }
}
