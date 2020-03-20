using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public class UpdateHandle : AnvilAbstractDisposable, IUpdateHandle
    {
        public static UpdateHandle Create<T>() where T:AbstractUpdateSource
        {
            UpdateHandle updateHandle = new UpdateHandle(typeof(T));
            return updateHandle;
        }
        
        private const uint CALL_LATER_HANDLE_INITIAL_ID = 0;
        private uint m_CallLaterHandleCurrentID = CALL_LATER_HANDLE_INITIAL_ID;
        
        private readonly Dictionary<uint, ICallLaterHandle> m_CallLaterHandles = new Dictionary<uint, ICallLaterHandle>();
        
        private event Action m_OnUpdate;

        private readonly Type m_UpdateSourceType;
        private AbstractUpdateSource m_UpdateSource;
        private int m_OnUpdateCount;
        private bool m_IsUpdateSourceHookEnabled;

        private AbstractUpdateSource UpdateSource
        {
            get
            {
                if (m_UpdateSource == null)
                {
                    m_UpdateSource = UpdateHandleSourcesManager.GetOrCreateUpdateSource(m_UpdateSourceType);
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

        private UpdateHandle(Type updateSourceType)
        {
            m_UpdateSourceType = updateSourceType;
        }

        protected override void DisposeSelf()
        {
            m_OnUpdate = null;
            if (m_UpdateSource != null)
            {
                m_UpdateSource.OnUpdate -= HandleOnUpdate;
                m_UpdateSource = null;
            }

            foreach (ICallLaterHandle callLaterHandle in m_CallLaterHandles.Values)
            {
                callLaterHandle.Cancel();
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
        
        private uint GetNextCallLaterHandleID()
        {
            uint id = m_CallLaterHandleCurrentID;
            m_CallLaterHandleCurrentID++;
            return id;
        }

        public ICallLaterHandle CallLater(ICallLaterHandle callLaterHandle)
        {
            // callLaterHandle.ID = GetNextCallLaterHandleID();
            callLaterHandle.OnDisposing += HandleOnCallLaterHandleDisposing;
            m_CallLaterHandles.Add(callLaterHandle.ID, callLaterHandle);
            // callLaterHandle.Start();
            return callLaterHandle;
        }

        private void HandleOnCallLaterHandleDisposing(ICallLaterHandle callLaterHandle)
        {
            if (!m_CallLaterHandles.ContainsKey(callLaterHandle.ID))
            {
                throw new Exception($"Tried to remove Call Later Handle with ID {callLaterHandle.ID} but it didn't exist in the lookup!");
            }

            m_CallLaterHandles.Remove(callLaterHandle.ID);
        }
        
    }
}

