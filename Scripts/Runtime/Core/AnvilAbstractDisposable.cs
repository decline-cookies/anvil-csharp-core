using System;

namespace Anvil.CSharp.Core
{
    public abstract class AnvilAbstractDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            DisposeSelf();
        }

        protected virtual void DisposeSelf()
        {
            
        }
    } 
}

