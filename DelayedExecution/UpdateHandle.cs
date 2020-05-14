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
        public const int CALL_AFTER_INFINITE_REPEAT_LIMIT = 0;
        public const int CALL_AFTER_DEFAULT_REPEAT_LIMIT = 1;

        private const uint CALL_AFTER_HANDLE_INITIAL_ID = 0;



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


        private uint m_CallAfterHandleCurrentID = CALL_AFTER_HANDLE_INITIAL_ID;

        private readonly Dictionary<uint, CallAfterHandle> m_CallAfterHandles = new Dictionary<uint, CallAfterHandle>();
        private readonly List<Action> m_UpdateListeners = new List<Action>();

        private event Action m_OnUpdate;

        private readonly Type m_UpdateSourceType;
        private AbstractUpdateSource m_UpdateSource;

        private bool m_IsUpdateSourceHookEnabled;

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
                m_UpdateListeners.Add(value);
                ValidateUpdateSourceHook();
            }
            remove
            {
                m_UpdateListeners.Remove(value);
                m_OnUpdate -= value;
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

            foreach (CallAfterHandle callAfterHandle in m_CallAfterHandles.Values)
            {
                callAfterHandle.OnDisposing -= HandleOnCallAfterHandleDisposing;
                callAfterHandle.Dispose();
            }

            m_CallAfterHandles.Clear();
            m_UpdateListeners.Clear();

            base.DisposeSelf();
        }

        private void ValidateUpdateSourceHook()
        {
            if (!m_IsUpdateSourceHookEnabled && (m_UpdateListeners.Count > 0 || m_CallAfterHandles.Count > 0))
            {
                UpdateSource.OnUpdate += HandleOnUpdate;
                m_IsUpdateSourceHookEnabled = true;
            }
            else if (m_IsUpdateSourceHookEnabled && (m_UpdateListeners.Count == 0 && m_CallAfterHandles.Count == 0))
            {
                UpdateSource.OnUpdate -= HandleOnUpdate;
                m_IsUpdateSourceHookEnabled = false;
            }
        }

        private void HandleOnUpdate()
        {
            foreach (CallAfterHandle callAfterHandle in m_CallAfterHandles.Values)
            {
                callAfterHandle.Update();
            }
            m_OnUpdate?.Invoke();
        }

        private uint GetNextCallAfterHandleID()
        {
            uint id = m_CallAfterHandleCurrentID;
            m_CallAfterHandleCurrentID++;

            return id;
        }

        /// <summary>
        /// Calls a specific function later on in the future. Depends on the passed in <see cref="AbstractCallLaterHandle"/>
        /// Call Later Handles are managed by the Update Handle and will be disposed if the Update Handle is disposed.
        /// </summary>
        /// <param name="callLaterHandle">The <see cref="AbstractCallLaterHandle"/> to use.</param>
        /// <returns>A reference to the <see cref="AbstractCallLaterHandle"/> to store for use later (Cancel, Complete).</returns>
        public CallAfterHandle CallAfter(float targetTime, Action callback, DeltaProvider deltaTimeProvider, int repeatCount = CALL_AFTER_DEFAULT_REPEAT_LIMIT)
        {
            CallAfterHandle callAfterHandle = new CallAfterHandle(GetNextCallAfterHandleID(),
                callback,
                targetTime,
                deltaTimeProvider,
                repeatCount);

            FinalizeCallAfterHandle(callAfterHandle);

            return callAfterHandle;
        }

        public CallAfterHandle CallAfter(int targetFrames, Action callback, DeltaProvider deltaFramesProvider, int repeatCount = CALL_AFTER_DEFAULT_REPEAT_LIMIT)
        {
            return CallAfter((float)targetFrames, callback, deltaFramesProvider, repeatCount);
        }

        private void FinalizeCallAfterHandle(CallAfterHandle callAfterHandle)
        {
            callAfterHandle.OnDisposing += HandleOnCallAfterHandleDisposing;
            m_CallAfterHandles.Add(callAfterHandle.ID, callAfterHandle);
            ValidateUpdateSourceHook();
        }

        private void HandleOnCallAfterHandleDisposing(CallAfterHandle callAfterHandle)
        {
            if (!m_CallAfterHandles.ContainsKey(callAfterHandle.ID))
            {
                throw new Exception($"Tried to remove Call After Handle with ID {callAfterHandle.ID} but it didn't exist in the lookup!");
            }

            m_CallAfterHandles.Remove(callAfterHandle.ID);
            ValidateUpdateSourceHook();
        }

    }
}

