using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CricketClubDomain
{
    public static class ExtensionMethods
    {
        public static string ToNiceString<T>(this T[] array)
        {
            return "[" + string.Join(" | ", array.Select(t=>t.ToString()).ToArray()) + "]";
        }
    }
}
