using System;

namespace MAB.DotIgnore
{
    // Re-implement a couple of convenience functions introduced in .NET 4, so we can support 3.5

    internal static class Utils
    {
        internal static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null) return true;
 
            for(int i = 0; i < value.Length; i++)
            {
                if(!char.IsWhiteSpace(value[i])) return false;
            }
 
            return true;
        }

        internal static bool HasFlag(this Enum variable, Enum value)
        {
            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
    }
}
