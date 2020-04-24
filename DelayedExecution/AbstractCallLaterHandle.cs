using System;
using System.Collections.Generic;
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
        private UpdateHandle m_UpdateHandle;
        
        /// <summary>
        /// An ID to represent the Call Later Handle.
        /// Used with <see cref="UpdateHandle.Cancel"/> to cancel before a Call Later Handle has fired.
        /// </summary>
        public uint ID { get; internal set; }
        
        internal UpdateHandle UpdateHandle
        {
            get => m_UpdateHandle;
            set
            {
                m_UpdateHandle = value;
                ValidateUpdateHandleSourceType(m_UpdateHandle.UpdateSourceType);
                m_UpdateHandle.OnUpdate += HandleOnUpdate;
            }
        }

        protected AbstractCallLaterHandle(Action callback)
        {
            m_Callback = callback;
        }

        protected override void DisposeSelf()
        {
            m_UpdateHandle.OnUpdate -= HandleOnUpdate;
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

        private void ValidateUpdateHandleSourceType(Type updateHandleSourceType)
        {
            List<Type> validUpdateSourceTypes = GetValidUpdateSourceTypes();
            if (!validUpdateSourceTypes.Contains(updateHandleSourceType))
            {
                //throw new Exception($"Trying to do a Call Later with {this} using an Update Handle configured with Update Source {updateHandleSourceType} but it isn't in the valid update source types!");
            }
        }
        
        /// <summary>
        /// Override to implement specific logic for how the Call Later Handle should respond to
        /// an <see cref="UpdateHandle.OnUpdate"/> event.
        /// </summary>
        protected abstract void HandleOnUpdate();
        
        /// <summary>
        /// Override to provide a list of valid <see cref="AbstractUpdateSource"/> types that this Call Later Handle
        /// can use.
        /// </summary>
        /// <returns></returns>
        protected abstract List<Type> GetValidUpdateSourceTypes();
    }
}

