using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Collections
{
    public static class DictionaryExtension
    {
        /// <summary>
        /// Dispose all elements of a <see cref="Dictionary{TKey,TValue}"/> and then clear the dictionary.
        /// </summary>
        /// <param name="dictionary">The <see cref="Dictionary{TKey,TValue}"/> to operate on.</param>
        /// <typeparam name="TKey">The key type</typeparam>
        /// <typeparam name="TValue">The value type</typeparam>
        public static void DisposeAllValuesAndClear<TKey, TValue>(this Dictionary<TKey, TValue> dictionary) where TValue : IDisposable
        {
            foreach (TValue value in dictionary.Values)
            {
                value.Dispose();
            }

            dictionary.Clear();
        }
    }
}