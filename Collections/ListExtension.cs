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
            foreach (var item in items)
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
        }
    }
}