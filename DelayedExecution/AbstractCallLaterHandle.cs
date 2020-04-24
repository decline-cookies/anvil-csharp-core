using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// The base class for calling a function after some amount of time in the future.
    /// </summary>
    public abstract class AbstractCallLaterHandle : AbstractAnvilDisposable
    {
        /// <summary>
        /// Dispatched whenever this Call Later Handle is disposing.
        /// </summary>
        public event Action<AbstractCallLaterHandle> OnDisposing;
        
        private Action m_Callback;

        /// <summary>
        /// An ID to represent the Call Later Handle.
        /// </summary>
        public uint ID { get; internal set; }

        protected AbstractCallLaterHandle(Action callback)
        {
            m_Callback = callback;
        }

        protected override void DisposeSelf()
        {
            m_Callback = null;
            
            OnDisposing?.Invoke(this);
            OnDisposing = null;
            
            base.DisposeSelf();
        }

        /// <summary>
        /// Completes the Call Later Handle immediately and fires the callback.
        /// Call Later Handle is disposed immediately after firing the callback.
        /// </summary>
        public void Complete()
        {
            m_Callback?.Invoke();
            Dispose();
        }

        internal void Update()
        {
            HandleOnUpdate();
        }

        protected abstract void HandleOnUpdate();
    }
}

