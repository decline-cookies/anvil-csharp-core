﻿using System;

namespace Anvil.CSharp.Core
{
    /// <summary>
    /// The base class for anything disposable in the Anvil Framework.
    /// Adds some convenience flow and functionality for <see cref="IAnvilDisposable"/> implementations.
    /// </summary>
    public abstract class AbstractAnvilDisposable : IAnvilDisposable
    {
        /// <summary>
        /// <inheritdoc cref="IAnvilDisposable.IsDisposed"/>
        /// </summary>
        public bool IsDisposed { get; private set; }
        
        /// <summary>
        /// <inheritdoc cref="IAnvilDisposable.IsDisposing"/>
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
