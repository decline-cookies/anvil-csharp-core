using System;
using System.Collections.Generic;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// An object that encapsulates logic to allow for code to execute over time. Provides a simple event to hook into
    /// for an "Update Loop" and allows the calling of specific functions later on in the future via
    /// <see cref="CallLater"/>
    /// </summary>
    public class UpdateHandle : AbstractAnvilDisposable
    {
        private const uint CALL_LATER_HANDLE_INITIAL_ID = 0;
        
        /// <summary>
        /// Convenience method for creation of an UpdateHandle
        /// </summary>
        /// <typeparam name="T">The type of <see cref="AbstractUpdateSource"/> to use.</typeparam>
        /// <returns>The instance of the Update Handle</returns>
        public static UpdateHandle Create<T>() where T:AbstractUpdateSource
        {
            UpdateHandle updateHandle = new UpdateHandle(typeof(T));
            return updateHandle;
        }
        
        
        private uint m_CallLaterHandleCurrentID = CALL_LATER_HANDLE_INITIAL_ID;

        private readonly Dictionary<uint, AbstractCallLaterHandle> m_CallLaterHandles = new Dictionary<uint, AbstractCallLaterHandle>();
        
        private event Action m_OnUpdate;

        private readonly Type m_UpdateSourceType;
        private AbstractUpdateSource m_UpdateSource;
        private int m_OnUpdateCount;
        private bool m_IsUpdateSourceHookEnabled;

        internal Type UpdateSourceType => m_UpdateSourceType;

        private AbstractUpdateSource UpdateSource
        {
            get
            {
                return m_UpdateSource ?? (m_UpdateSource = UpdateHandleSourcesManager.GetOrCreateUpdateSource(m_UpdateSourceType));
            }
        }

        /// <summary>
        /// Dispatches whenever the <see cref="AbstractUpdateSource"/> provides an Update event. This is dependent on
        /// the <see cref="AbstractUpdateSource"/>
        /// </summary>
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

            foreach (AbstractCallLaterHandle callLaterHandle in m_CallLaterHandles.Values)
            {
                callLaterHandle.OnDisposing -= HandleOnCallLaterHandleDisposing;
                callLaterHandle.Cancel();
            }
            m_CallLaterHandles.Clear();
            
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
        
        /// <summary>
        /// Calls a specific function later on in the future. Depends on the passed in <see cref="AbstractCallLaterHandle"/>
        /// Call Later Handles are managed by the Update Handle and will be disposed if the Update Handle is disposed.
        /// </summary>
        /// <param name="callLaterHandle">The <see cref="AbstractCallLaterHandle"/> to use.</param>
        /// <returns>A reference to the <see cref="AbstractCallLaterHandle"/> to store for use later (Cancel, Complete).</returns>
        public AbstractCallLaterHandle CallLater(AbstractCallLaterHandle callLaterHandle)
        {
            callLaterHandle.ID = GetNextCallLaterHandleID();
            callLaterHandle.OnDisposing += HandleOnCallLaterHandleDisposing;
            m_CallLaterHandles.Add(callLaterHandle.ID, callLaterHandle);
            callLaterHandle.UpdateHandle = this;
            return callLaterHandle;
        }

        private void HandleOnCallLaterHandleDisposing(AbstractCallLaterHandle callLaterHandle)
        {
            if (!m_CallLaterHandles.ContainsKey(callLaterHandle.ID))
            {
                throw new Exception($"Tried to remove Call Later Handle with ID {callLaterHandle.ID} but it didn't exist in the lookup!");
            }

            m_CallLaterHandles.Remove(callLaterHandle.ID);
        }
        
    }
}

