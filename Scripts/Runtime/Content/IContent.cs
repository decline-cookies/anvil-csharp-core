using System;

namespace Anvil.CSharp.Content
{
    /// <summary>
    /// An interface for Content to correspond to an <see cref="AbstractContentController"/> since the base class
    /// for a given piece of Content could vary.
    /// </summary>
    public interface IContent : IDisposable
    {
        /// <summary>
        /// Dispatched when the Content is disposing so that corresponding <see cref="AbstractContentController"/>s can
        /// dispose themselves nicely as well.
        /// </summary>
        event Action OnContentDisposing;
    }
}

