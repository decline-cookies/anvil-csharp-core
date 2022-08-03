using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Collections
{
    /// <summary>
    /// Extension methods for use with <see cref="ICollection{T}"/> instances.
    /// </summary>
    public static class CollectionExtension
    {
        /// <summary>
        /// Dispose all elements of a <see cref="ICollection{T}"/> and then clear the collection.
        /// </summary>
        /// <param name="collection">The <see cref="ICollection{T}"/> to operate on.</param>
        /// <typeparam name="T">The element type</typeparam>
        /// <remarks>
        /// This method will only clear the <see cref="collection"/> if <see cref="ICollection{T}.IsReadOnly"/> is false.
        /// </remarks>
        public static void DisposeAllAndTryClear<T>(this ICollection<T> collection) where T : IDisposable
        {
            foreach (T item in collection)
            {
                item.Dispose();
            }

            if (!collection.IsReadOnly)
            {
                collection.Clear();
            }
        }
    }
}