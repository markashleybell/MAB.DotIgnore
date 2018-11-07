using System;
using System.Collections.Generic;
using System.Linq;

namespace MAB.DotIgnore
{
    /// <summary>
    /// Keeps track of which rules matched which path (including overrides etc).
    /// </summary>
#pragma warning disable CA1710 // Identifiers should have correct suffix
#pragma warning disable CA2237 // Mark ISerializable types with serializable
    public class IgnoreLog : Dictionary<string, List<string>>
#pragma warning restore CA2237 // Mark ISerializable types with serializable
#pragma warning restore CA1710 // Identifiers should have correct suffix
    {
        /// <summary>
        /// Returns a "pretty printed" string representation of the rule match log.
        /// </summary>
        /// <returns>String representation of the match log.</returns>
        public override string ToString()
        {
            var nl = Environment.NewLine;
            var prefix = nl + "    ";
            return string.Join(nl + nl, Keys.Select(k => k + prefix + string.Join(prefix, base[k].ToArray())).ToArray());
        }
    }
}
