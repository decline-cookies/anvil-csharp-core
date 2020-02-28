using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public class UpdateSourcePipe : AnvilAbstractDisposable
    {
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

        private event Action m_OnUpdate;
        
        private AbstractUpdateSource m_UpdateSource;
        private int m_OnUpdateCount;
        private bool m_IsUpdateSourceHookEnabled;
        public UpdateSourcePipe(AbstractUpdateSource updateSource)
        {
            m_UpdateSource = updateSource;
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
                m_UpdateSource.OnUpdate += HandleOnUpdate;
                m_IsUpdateSourceHookEnabled = true;
            }
            else if (m_OnUpdateCount <= 0 && m_IsUpdateSourceHookEnabled)
            {
                m_UpdateSource.OnUpdate -= HandleOnUpdate;
                m_IsUpdateSourceHookEnabled = false;
            }
        }

        private void HandleOnUpdate()
        {
            m_OnUpdate?.Invoke();
        }
    }
}
