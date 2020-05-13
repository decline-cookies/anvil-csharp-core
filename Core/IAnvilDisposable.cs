using System;

namespace Anvil.CSharp.Core
{
    /// <summary>
    /// Interface to allow for a Disposable <see cref="IDisposable"/> object that also implements two state booleans.
    /// One for if the object has been disposed and one for if it is currently being disposed.
    /// <see cref="AbstractAnvilDisposable"/>
    /// </summary>
    public interface IAnvilDisposable : IDisposable
    {
        /// <summary>
        /// Allows an instance to be queried to know if <see cref="Dispose"/> has been called yet or not and if
        /// the instance has been completely disposed. All <see cref="DisposeSelf"/> functions down the inheritance
        /// chain have been called.
        /// </summary>
        bool IsDisposed { get; }
        
        /// <summary>
        /// Allows an instance to be queried to know if <see cref="Dispose"/> has been called yet or not and if
        /// the instance is currently disposing.
        /// </summary>
        bool IsDisposing { get; }
    }
}

