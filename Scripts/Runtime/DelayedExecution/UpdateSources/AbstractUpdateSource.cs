using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public abstract class AbstractUpdateSource : AbstractAnvilDisposable
    {
        public event Action OnUpdate;

        protected AbstractUpdateSource()
        {
            Init();
        }

        protected override void DisposeSelf()
        {
            OnUpdate = null;
            base.DisposeSelf();
        }
        
        protected abstract void Init();
        
        protected void DispatchOnUpdateEvent()
        {
            OnUpdate?.Invoke();
        }
    }
}

