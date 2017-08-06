using System;
using System.Collections.Generic;

namespace CIRCBot
{
    /// <summary>
    /// Extensions methods for Dictionaries.
    /// </summary>
    static class DictionaryExtensions
    {
        /// <summary>
        /// Add multiple Keys with the same Value to the dictionary.
        /// </summary>
        /// <typeparam name="T">Type of the Key</typeparam>
        /// <typeparam name="T2">Type of the Value</typeparam>
        /// <param name="dict"></param>
        /// <param name="value">Value for the dictionary entries</param>
        /// <param name="keys">Array of Keys for the dictionary entries</param>
        public static void AddKeys<T, T2>(this Dictionary<T2, T> dict, T value, params T2[] keys)
        {
            foreach (T2 key in keys)
            {
                dict.Add(key, @value);
            }
        }
    }
}
