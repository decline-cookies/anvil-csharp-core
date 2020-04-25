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
        /// Allows an instance to be queried to know if <see cref="Dispose"/> has been called yet or not.
        /// </summary>
        public bool IsDisposed { get; private set; }
        
        /// <summary>
        /// <inheritdoc cref="IDisposable.Dispose"/>
        /// Will early return if <see cref="IsDisposed"/> is true.
        /// Calls the virtual method <see cref="DisposeSelf"/> for inherited classes to override.
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            DisposeSelf();
        }
        
        /// <summary>
        /// Override to implement specific Dispose logic.
        /// </summary>
        protected virtual void DisposeSelf()
        {
            
        }
    } 
}

