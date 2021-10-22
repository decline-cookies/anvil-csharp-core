using System.Collections.Generic;
using System.Threading;

public static class LINQExtension
{
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
