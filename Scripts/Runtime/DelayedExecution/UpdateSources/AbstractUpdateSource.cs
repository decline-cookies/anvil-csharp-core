using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public abstract class AbstractUpdateSource : AnvilAbstractDisposable
    {
        public event Action OnUpdate;

        protected AbstractUpdateSource()
        {
            
        }

        protected override void DisposeSelf()
        {
            OnUpdate = null;
            base.DisposeSelf();
        }

        public void RegisterUpdateSource()
        {
            Initialize();
            UpdateHandleSystem.RegisterUpdateSource(this);
        }
        protected abstract void Initialize();
        
        protected void DispatchOnUpdateEvent()
        {
            OnUpdate?.Invoke();
        }
    }
}

