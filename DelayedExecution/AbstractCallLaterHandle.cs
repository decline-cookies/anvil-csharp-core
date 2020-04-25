using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public abstract class AbstractCallLaterHandle : AbstractAnvilDisposable
    {
        public event Action<AbstractCallLaterHandle> OnDisposing;
        
        private Action m_Callback;
        private UpdateHandle m_UpdateHandle;

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

        public void Cancel()
        {
            Dispose();
        }

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

        protected abstract void HandleOnUpdate();
        protected abstract List<Type> GetValidUpdateSourceTypes();
    }
}

