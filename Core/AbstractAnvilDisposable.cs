using System;

namespace Anvil.CSharp.Core
{
    /// <summary>
    /// The base class for anything disposable in the Anvil Framework.
    /// Adds some convenience flow and functionality for <see cref="IDisposable"/> implementations.
    /// </summary>
    public abstract class AbstractAnvilDisposable : IDisposable
    {
        /// <summary>
        /// Allows an instance to be queried to know if <see cref="Dispose"/> has been called yet or not and if
        /// the instance has been completely disposed. All <see cref="DisposeSelf"/> functions down the inheritance
        /// chain have been called.
        /// </summary>
        public bool IsDisposed { get; private set; }
        
        /// <summary>
        /// Allows an instance to be queried to know if <see cref="Dispose"/> has been called yet or not and if
        /// the instance is currently disposing.
        /// </summary>
        public bool IsDisposing { get; private set; }
        
        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// Will early return if <see cref="IsDisposed"/> or <see cref="IsDisposing"/> is true.
        /// Calls the virtual method <see cref="DisposeSelf"/> for inherited classes to override.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposing || IsDisposed)
            {
                return;
            }

            IsDisposing = true;
            DisposeSelf();
            IsDisposing = false;
            IsDisposed = true;
        }
        
        /// <summary>
        /// Override to implement specific Dispose logic.
        /// </summary>
        protected virtual void DisposeSelf()
        {
            
        }
    } 
}

