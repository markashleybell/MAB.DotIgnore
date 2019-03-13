namespace MAB.DotIgnore.Test.Support
{
    internal class GitTest
    {
        public bool ExpectGlobMatch { get; set; }

        public bool ExpectGlobMatchCI { get; set; }

        public bool ExpectPathMatch { get; set; }

        public bool ExpectPathMatchCI { get; set; }

        public int LineNumber { get; set; }

        public string Path { get; set; }

        public string Pattern { get; set; }
    }
}
