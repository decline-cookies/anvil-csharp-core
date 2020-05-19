using System;
using System.Collections.Generic;
using System.Diagnostics;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// An object that encapsulates logic to allow for code to execute over time. Provides a simple event to hook into
    /// for an "Update Loop" and allows the calling of specific functions later on in the future
    /// via <see cref="CallAfterHandle"/>
    /// </summary>
    public class UpdateHandle : AbstractAnvilDisposable
    {
        /// <summary>
        /// Use with <see cref="UpdateHandle.CallAfter"/> to allow a CallAfterHandle to repeat indefinitely.
        /// </summary>
        public const uint CALL_AFTER_INFINITE_CALL_LIMIT = 0;
        /// <summary>
        /// Use with <see cref="UpdateHandle.CallAfter"/> to have a CallAfterHandle fire it's callback just once.
        /// </summary>
        public const uint CALL_AFTER_DEFAULT_CALL_LIMIT = 1;

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
        private readonly List<CallAfterHandle> m_UpdateIterator = new List<CallAfterHandle>();

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
                m_UpdateSource.OnUpdate -= UpdateSource_OnUpdate;
                m_UpdateSource = null;
            }

            foreach (CallAfterHandle callAfterHandle in m_CallAfterHandles.Values)
            {
                callAfterHandle.OnDisposing -= CallAfterHandle_OnDisposing;
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
                UpdateSource.OnUpdate += UpdateSource_OnUpdate;
                m_IsUpdateSourceHookEnabled = true;
            }
            else if (m_IsUpdateSourceHookEnabled && (m_UpdateListeners.Count == 0 && m_CallAfterHandles.Count == 0))
            {
                UpdateSource.OnUpdate -= UpdateSource_OnUpdate;
                m_IsUpdateSourceHookEnabled = false;
            }
        }

        private void UpdateSource_OnUpdate()
        {
            if (m_CallAfterHandles.Count > 0)
            {
                //Take a snapshot of CallAfterHandles valid for this frame to iterate through since m_CallAfterHandles
                //could have new CallAfterHandles added or existing ones removed as part of the stack from their Update.
                m_UpdateIterator.AddRange(m_CallAfterHandles.Values);
                foreach (CallAfterHandle callAfterHandle in m_UpdateIterator)
                {
                    callAfterHandle.Update();
                }
                m_UpdateIterator.Clear();
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
        /// Calls a specific function later on in the future via <see cref="CallAfterHandle"/>.
        /// CallAfterHandles are managed by the UpdateHandle and will be disposed if the UpdateHandle is disposed.
        /// </summary>
        /// <remarks>
        /// CallAfterHandles operate on floats but can easily be used to call after a certain amount of "frames".
        /// Simply pass in whole numbers (1.0f, 2.0f, etc) and use a <see cref="DeltaProvider"/> function that returns
        /// whole numbers to represent "frames".
        /// </remarks>
        /// <param name="targetTime">The amount of time to wait until firing the callback function.</param>
        /// <param name="callback">The callback function to fire</param>
        /// <param name="deltaTimeProvider">A <see cref="DeltaProvider"/> function to allow the
        /// <see cref="CallAfterHandle"/></param> to calculate the amount of time that has passed each time this
        /// <see cref="UpdateHandle"/>'s Update event fires.
        /// <param name="repeatCount">The amount of times this <see cref="CallAfterHandle"/> should repeat. Defaults to
        /// <see cref="CALL_AFTER_DEFAULT_CALL_LIMIT"/></param>
        /// <returns>A reference to the <see cref="CallAfterHandle"/> to store for use later. (Complete, Dispose)</returns>
        public CallAfterHandle CallAfter(float targetTime, Action callback, DeltaProvider deltaTimeProvider, uint repeatCount = CALL_AFTER_DEFAULT_CALL_LIMIT)
        {
            CallAfterHandle callAfterHandle = new CallAfterHandle(GetNextCallAfterHandleID(),
                callback,
                targetTime,
                deltaTimeProvider,
                repeatCount);

            FinalizeHandle(callAfterHandle);

            return callAfterHandle;
        }

        private void FinalizeHandle(CallAfterHandle callAfterHandle)
        {
            callAfterHandle.OnDisposing += CallAfterHandle_OnDisposing;
            m_CallAfterHandles.Add(callAfterHandle.ID, callAfterHandle);
            ValidateUpdateSourceHook();
        }

        private void CallAfterHandle_OnDisposing(CallAfterHandle callAfterHandle)
        {
            Debug.Assert(!m_CallAfterHandles.ContainsKey(callAfterHandle.ID), $"Tried to remove Call After Handle with ID {callAfterHandle.ID} but it didn't exist in the lookup!");

            m_CallAfterHandles.Remove(callAfterHandle.ID);
            ValidateUpdateSourceHook();
        }

    }
}

