using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MAB.DotIgnore
{
    public static class Utils
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value == null) return true;
 
            for(int i = 0; i < value.Length; i++)
            {
                if(!char.IsWhiteSpace(value[i])) return false;
            }
 
            return true;
        }

        public static bool HasFlag(this Enum variable, Enum value)
        {
            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
    }
}
