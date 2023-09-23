using System.Collections.Generic;

namespace Anvil.CSharp.Collections
{
    /// <summary>
    /// A collection of extensions for working with <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class EnumerableExtension
    {
        /// <summary>
        /// Convert a collection of elements to a string representation calling <see cref="object.ToString()"/> on each one.
        /// SampleOutput: "[element1,element2,element3]"
        /// </summary>
        /// <param name="collection">The collection of elements.</param>
        /// <typeparam name="TElement">The type of element.</typeparam>
        /// <returns>A string representation of the collection.</returns>
        public static string ToElementString<TElement>(this IEnumerable<TElement> collection)
        {
            return $"[{string.Join(",", collection)}]";
        }
    }
}