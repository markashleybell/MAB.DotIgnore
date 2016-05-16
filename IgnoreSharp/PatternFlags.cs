using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IgnoreSharp
{
    [Flags]
    public enum PatternFlags 
    {
        NONE = 0,
        WILD = 1, // Pattern contains '*', '[', and/or '?'
        WILD2 = 2, // Pattern contains '**'
        WILD2_PREFIX = 4, // Pattern starts with '**'
        ABSOLUTE_PATH = 8, // Pattern starts with '/'
        DIRECTORY = 16 // Pattern should match only directories
    }
}
