using System;
using System.Diagnostics;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// The base class for adding a new source to provide Update events from.
    /// </summary>
    public abstract class AbstractUpdateSource : AbstractAnvilBase
    {
        /// <summary>
        /// Dispatched whenever an Update event is emitted from the source.
        /// </summary>
        public event Action OnUpdate;

        /// <summary>
        /// Indicates whether the source is currently executing its OnUpdate phase.
        /// </summary>
        public bool IsUpdating { get; private set; }

        protected AbstractUpdateSource() { }

        protected override void DisposeSelf()
        {
            UpdateHandleSourcesManager.RemoveUpdateSource(this);
            OnUpdate = null;

            base.DisposeSelf();
        }

        protected void DispatchOnUpdateEvent()
        {
            Debug.Assert(!IsUpdating);
            IsUpdating = true;

            OnUpdate?.Invoke();

            Debug.Assert(IsUpdating);
            IsUpdating = false;
        }
    }
}
