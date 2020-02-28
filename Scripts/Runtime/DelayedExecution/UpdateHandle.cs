using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public class UpdateHandle<T> : AnvilAbstractDisposable where T:AbstractUpdateSource
    {
        private event Action m_OnUpdate;
        
        private AbstractUpdateSource m_UpdateSource;
        private int m_OnUpdateCount;
        private bool m_IsUpdateSourceHookEnabled;

        private AbstractUpdateSource UpdateSource
        {
            get
            {
                if (m_UpdateSource == null)
                {
                    Type sourceType = typeof(T);
                    m_UpdateSource = UpdateHandleSystem.GetUpdateSource(sourceType);
                }

                return m_UpdateSource;
            }
        }

        public event Action OnUpdate
        {
            add
            {
                m_OnUpdate += value;
                m_OnUpdateCount++;
                ValidateUpdateSourceHook();
            }
            remove
            {
                //Don't want multiple unsubscribes to mess with counting
                if (m_OnUpdateCount <= 0)
                {
                    return;
                }
                m_OnUpdate -= value;
                m_OnUpdateCount--;
                ValidateUpdateSourceHook();
            }
        }

        protected override void DisposeSelf()
        {
            m_OnUpdate = null;
            if (m_UpdateSource != null)
            {
                m_UpdateSource.OnUpdate -= HandleOnUpdate;
                m_UpdateSource = null;
            }
            
            base.DisposeSelf();
        }
        
        private void ValidateUpdateSourceHook()
        {
            if (m_OnUpdateCount > 0 && !m_IsUpdateSourceHookEnabled)
            {
                UpdateSource.OnUpdate += HandleOnUpdate;
                m_IsUpdateSourceHookEnabled = true;
            }
            else if (m_OnUpdateCount <= 0 && m_IsUpdateSourceHookEnabled)
            {
                UpdateSource.OnUpdate -= HandleOnUpdate;
                m_IsUpdateSourceHookEnabled = false;
            }
        }
        
        private void HandleOnUpdate()
        {
            m_OnUpdate?.Invoke();
        }
    }
}

