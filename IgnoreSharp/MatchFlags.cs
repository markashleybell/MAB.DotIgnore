using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgnoreSharp
{
    [Flags]
    public enum MatchFlags
    {
        NONE = 0,
        IGNORE_CASE = 1,
        PATHNAME = 2
    }
}
