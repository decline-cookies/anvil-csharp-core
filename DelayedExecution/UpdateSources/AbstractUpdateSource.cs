using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// The base class for adding a new source to provide Update events from.
    /// </summary>
    public abstract class AbstractUpdateSource : AbstractAnvilDisposable
    {
        /// <summary>
        /// Dispatched whenever an Update event is emitted from the source.
        /// </summary>
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

