using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public abstract class AbstractCallLaterHandle : AnvilAbstractDisposable, ICallLaterHandle
    {
        public event Action<ICallLaterHandle> OnDisposing;
        
        private Action m_Callback;
        private readonly IUpdateHandle m_UpdateHandle;

        public uint ID { get; internal set; }

        protected AbstractCallLaterHandle(Action callback, IUpdateHandle updateHandle)
        {
            m_Callback = callback;
            m_UpdateHandle = updateHandle;
        }

        protected override void DisposeSelf()
        {
            m_UpdateHandle.OnUpdate -= HandleOnUpdate;
            m_Callback = null;
            
            OnDisposing?.Invoke(this);
            OnDisposing = null;
            
            base.DisposeSelf();
        }

        internal void Start()
        {
            m_UpdateHandle.OnUpdate += HandleOnUpdate;
        }

        public void Cancel()
        {
            Dispose();
        }

        public void Complete()
        {
            m_Callback?.Invoke();
            Dispose();
        }

        protected abstract void HandleOnUpdate();
    }
}

