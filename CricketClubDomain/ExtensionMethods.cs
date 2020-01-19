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

        public static TValue GetValueOrInitializeDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, defaultValue);
            }
            return dictionary[key];

        }

        public static T[] Add<T>(this T[] array, T value)
        {
            
            var list = array?.ToList() ?? new List<T>();
            list.Add(value);
            return list.ToArray();
        }

        public static bool IsNullOrEmpty<T>(this IEnumerable<T> value)
        {
            return value == null || value.Any();
        }
        
        public static IEnumerable<T> AsEnumerable<T>(this Tuple<T, T> value)
        {
            yield return value.Item1;
            yield return value.Item2;
        }
    }
}
