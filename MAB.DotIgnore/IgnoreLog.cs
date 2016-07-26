using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.DotIgnore
{
    public class IgnoreLog : Dictionary<string, List<string>>
    {
        public override string ToString()
        {
            var nl = Environment.NewLine;
            var prefix = nl + "    ";
            return string.Join(nl + nl, base.Keys.Select(k => k + prefix + string.Join(prefix, base[k].ToArray())).ToArray());
        }
    }
}
