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
        /// Gets/Sets the <see cref="AbstractContentController"/> to correspond to this content. 
        /// </summary>
        AbstractContentController ContentController { get; }
    }
}

