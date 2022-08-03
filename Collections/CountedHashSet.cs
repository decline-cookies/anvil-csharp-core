using System.Collections.Generic;

namespace Anvil.CSharp.Collections
{
    /// <summary>
    /// A hash set that maintains a balance of add/remove calls and only removes the item when <see cref="Remove"/> has
    /// been called as many times as <see cref="Add"/>.
    /// </summary>
    /// <typeparam name="T">The type of items to store.</typeparam>
    public class CountedHashSet<T>
    {
        private readonly Dictionary<T, int> m_Items;

        /// <summary>
        /// A collection of items that have been added to the set.
        /// </summary>
        public Dictionary<T, int>.KeyCollection Items
        {
            get => m_Items.Keys;
        }

        /// <summary>
        /// Creates a new instance of <see cref="CountedHashSet{T}"/>.
        /// </summary>
        public CountedHashSet()
        {
            m_Items = new Dictionary<T, int>();
        }

        /// <summary>
        /// Add an item to the set or increase the count if it already exists.
        /// </summary>
        /// <param name="item">The item to add.</param>
        /// <returns>True if the item is new to the set.</returns>
        public bool Add(T item)
        {
            if (m_Items.TryAdd(item, 1))
            {
                return true;
            }

            m_Items[item]++;
            return false;
        }

        /// <summary>
        /// Remove an item from the set or decrease the count if it already exists.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item is removed from the set or did not previously exist in the set.</returns>
        public bool Remove(T item)
        {
            int count = m_Items[item];
            count--;

            if (count <= 0)
            {
                System.Diagnostics.Debug.Assert(count == 0);
                m_Items.Remove(item);
                return true;
            }

            m_Items[item] = count;
            return false;
        }

        /// <summary>
        /// Clear all items from the set regardless of count.
        /// </summary>
        public void Clear()
        {
            m_Items.Clear();
        }

    }
}