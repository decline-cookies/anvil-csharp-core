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
        /// Gets the <see cref="AbstractContentController"/> bound to this content.
        /// </summary>
        AbstractContentController Controller { get; }

        /// <summary>
        /// Binds the content to a <see cref="AbstractContentController"/>.
        /// </summary>
        /// <param name="controller">The controller to bind to this content</param>
        /// <remarks>
        /// The controller will only be set once. Implementations should assert this.
        /// <see cref="AbstractContentController"/> calls this immediately after receiving reference to this content.
        /// </remarks>
        protected internal void BindTo(AbstractContentController controller);
    }
}
