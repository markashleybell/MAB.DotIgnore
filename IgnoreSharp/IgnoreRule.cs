using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgnoreSharp
{
    public class IgnoreRule
    {
        public string Pattern { get; set; }
        public Glob Glob { get; set; }
        public bool Exclude { get; set; }
        public bool DirectoryOnly { get; set; }
    }
}
