using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Anvil.CSharp.Linq
{
    /// <summary>
    /// A set of convenience LINQ extensions
    /// </summary>
    public static class LINQExtension
    {
        /// <summary>
        /// Allows a synchronous query to be cancelled by a <see cref="CancellationToken"/>.
        /// Typically used when a long running, LINQ (not PLINQ) query is executed in a cancellable context.
        /// (Ex: <see cref="System.Threading.Tasks.Task"/>)
        /// </summary>
        /// <remarks>
        /// Take care where this is placed in a query. It is most effective in a section of
        /// the iteration that is lazy evaluated and visited often.
        /// `AsCancellable()` can be specified multiple times in a query. The only overhead is additional
        /// <see cref="CancellationToken.IsCancellationRequested"/> checks. In some situations this can help
        /// improve the responsiveness of a cancellation request.
        /// </remarks>
        /// <example>
        /// This is a poor placement of AsCancellable. The `ToArray()` instruction causes the myCollection to
        /// be iterated completely before getting into the expensive `Count()` condition. The `Count()` condition
        /// is where most of the time will be spent but will not respond to cancellation requests since the instructions up to `Count()` have already been evaluated.
        /// myCollection
        ///     .AsCancellable()
        ///     .ToArray()
        ///     .Count(someExpensiveCondition);
        ///
        /// Ideally, remove `ToArray()` so that `AsCancellable()` is getting evaluated on each iteration.
        /// myCollection
        ///     .AsCancellable()
        ///     .Count(someExpensiveCondition);
        ///
        /// Alternatively, `AsCancelled()` can be moved below the `ToArray()` so that it is evaluated on each iteration.
        /// myCollection
        ///     .ToArray()
        ///     .AsCancellable()
        ///     .Count(someExpensiveCondition);
        /// </example>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to iterate.</param>
        /// <param name="token">The cancellation token to monitor.</param>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> whose next iteration aborts after
        /// <see cref="CancellationToken.IsCancellationRequested"/> is set to <see langword="true"/>.
        /// </returns>
        public static IEnumerable<TSource> AsCancellable<TSource>(this IEnumerable<TSource> source, CancellationToken token)
        {
            foreach (TSource item in source)
            {
                if (token.IsCancellationRequested)
                {
                    token.ThrowIfCancellationRequested();
                    yield break;
                }

                yield return item;
            }
        }

        /// <summary>
        /// Immediately executes an action on every item in a query.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="source">A sequence of values to iterate.</param>
        /// <param name="action">The action to perform on each query item</param>
        public static void ForEach<TSource>(this IEnumerable<TSource> source, Action<TSource> action)
        {
            foreach (TSource item in source)
            {
                action(item);
            }
        }

        /// <summary>
        /// Checks whether a collection contains duplicate elements using the default equality check.
        /// </summary>
        /// <param name="source">The collection to check.</param>
        /// <typeparam name="TSource">The element type in the collection.</typeparam>
        /// <returns>true if there are duplicate entries in the collection.</returns>
        public static bool ContainsDuplicates<TSource>(this IEnumerable<TSource> source)
        {
            return source.Distinct().Count() != source.Count();
        }
    }
}