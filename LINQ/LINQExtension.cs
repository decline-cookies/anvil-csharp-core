using System.Collections.Generic;
using System.Threading;

/// <summary>
/// A set of convenience LINQ extensions
/// </summary>
public static class LINQExtension
{
    /// <summary>
    /// Allows a syncronous query to be cancelled by a <see cref="CancellationToken"/>.
    /// Typically used when a long running, non-PLINQ query is executed in a cancellable context.
    /// (Ex: <see cref="System.Threading.Tasks.Task"/>)
    /// </summary>
    /// <remarks>
    /// Take care where this is placed in a query. It is most effective in a section of 
    /// the iteration that is lazy evaluated and visited often.
    /// </remarks>
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
}
