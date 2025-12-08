using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Collections
{
    /// <summary>
    /// Extension methods for use with <see cref="IList{T}"/> and <see cref="List{T}"/> instances.
    /// </summary>
    public static class ListExtension
    {
        /// <summary>
        /// Uniquely adds a collection of items to an <see cref="IList{T}"/>.
        /// Useful for when LINQ can't be used or the underlying list instance must remain the same.
        /// </summary>
        /// <param name="list">The list to add to.</param>
        /// <param name="items">The items to attempt to add.</param>
        public static void AddUnique<T>(this IList<T> list, IEnumerable<T> items)
        {
            foreach (T item in items)
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
        }

        /// <summary>
        /// Shuffle the elements in a list using the Fisher-Yates algorithm.
        /// </summary>
        /// <param name="collection">The collection to shuffle the elements of</param>
        /// <param name="randomProvider">Produces a random integer between arg1 (inclusive) and arg2 (exclusive)</param>
        public static void Shuffle<T>(this IList<T> collection, Func<int, int, int> randomProvider)
        {
            int length = collection.Count;
            for (int i = length-1; i > 0; i--)
            {
                int randomIndex =  randomProvider(0, i+1);
                (collection[i], collection[randomIndex]) = (collection[randomIndex], collection[i]);
            }
        }
    }
}
