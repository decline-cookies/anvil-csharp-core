using System;
using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Find the first index in the enumerable matching the given predicate. Returns -1 if no matches are found.
        /// </summary>
        /// <param name="enumerable">The enumerable to search.</param>
        /// <param name="predicate">The condition to match.</param>
        /// <typeparam name="TElement">The type of the element.</typeparam>
        /// <returns>The first index matching the given predicate, or -1 if none is found.</returns>
        public static int FindIndex<TElement>(this IEnumerable<TElement> enumerable, Predicate<TElement> predicate)
        {
            return enumerable
                .Select((element, i) => (element, i))
                .Where(x => predicate(x.element))
                .Select(x => x.i)
                .DefaultIfEmpty(-1)
                .First();
        }

        /// <summary>
        /// Find the first index in the enumerable equal to the given object. Returns -1 if no matches are found.
        /// </summary>
        /// <param name="enumerable">The enumerable to search.</param>
        /// <param name="element">The element to find.</param>
        /// <typeparam name="TElement">The type of the element</typeparam>
        /// <returns>The first index equal to the given object, or -1 if none is found.</returns>
        public static int IndexOf<TElement>(this IEnumerable<TElement> enumerable, TElement element)
        {
            // NOTE: EqualityComparer<T>.Default is used instead of Object.Equals, because it checks for an
            // IEquatable<T> override in cases where Object.Equals has not been overridden
            return enumerable
                .Select((el, i) => (el, i))
                .Where(x => EqualityComparer<TElement>.Default.Equals(x.el, element))
                .Select(x => x.i)
                .DefaultIfEmpty(-1)
                .First();
        }
    }
}
