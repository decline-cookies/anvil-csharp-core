using System;
using System.Collections.Generic;
using System.Diagnostics;
using Anvil.CSharp.Core;
using Anvil.CSharp.Data;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// An object that encapsulates logic to allow for code to execute over time. Provides a simple event to hook into
    /// for an "Update Loop" and allows the calling of specific functions later on in the future
    /// via <see cref="CallAfterHandle"/>
    /// </summary>
    public class UpdateHandle : AbstractAnvilBase
    {
        /// <summary>
        /// Use with <see cref="UpdateHandle.CallAfter"/> to allow a CallAfterHandle to repeat indefinitely.
        /// </summary>
        public const uint CALL_AFTER_INFINITE_CALL_LIMIT = 0;
        /// <summary>
        /// Use with <see cref="UpdateHandle.CallAfter"/> to have a CallAfterHandle fire it's callback just once.
        /// </summary>
        public const uint CALL_AFTER_DEFAULT_CALL_LIMIT = 1;

        /// <summary>
        /// A <see cref="DeltaProvider"/> that provides a fixed delta of 1.
        /// </summary>
        public static readonly DeltaProvider FixedDeltaProvider = () => 1f;

        /// <summary>
        /// Convenience method for creation of an UpdateHandle
        /// </summary>
        /// <typeparam name="T">The type of <see cref="AbstractUpdateSource"/> to use.</typeparam>
        /// <returns>The instance of the Update Handle</returns>
        public static UpdateHandle Create<T>() where T : AbstractUpdateSource
        {
            UpdateHandle updateHandle = new UpdateHandle(typeof(T));
            return updateHandle;
        }

        private readonly IDProvider m_IDProvider = new IDProvider();
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

        /// <summary>
        /// Indicates whether the handle's <see cref="UpdateSource"/> is currently executing its OnUpdate phase.
        /// </summary>
        public bool IsSourceUpdating
        {
            get => m_UpdateSource != null && m_UpdateSource.IsUpdating;
        }
        /// <summary>
        /// Indicates whether the handle is currently executing its OnUpdate phase.
        /// </summary>
        public bool IsUpdating { get; private set; }

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
            Debug.Assert(!IsUpdating);
            IsUpdating = true;

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

            Debug.Assert(IsUpdating);
            IsUpdating = false;
        }

        /// <summary>
        /// Calls the provided callback after an arbitrary delta (usually time) via a <see cref="CallAfterHandle"/>.
        /// Delta is measured on each <see cref="OnUpdate"/> tick and the callback is called if the aggregated delta is >= the targetDelta.
        /// CallAfterHandles are managed by the UpdateHandle and will be disposed if the UpdateHandle is disposed.
        /// </summary>
        /// <remarks>
        /// CallAfterHandles usually operate on delta time but can easily be used to call after any type of delta.
        /// Example: <see cref="CallAfterUpdates(int, Action, uint)"/> is a convenience method that calls after a
        /// number of updates (usually frames).
        /// </remarks>
        /// <param name="targetDelta">The amount of delta (usually time) to wait until firing the callback function.</param>
        /// <param name="callback">The callback function to fire</param>
        /// <param name="deltaProvider">A <see cref="DeltaProvider"/> function to allow the <see cref="CallAfterHandle"/>
        ///  to calculate the amount of delta that has passed each between <see cref="OnUpdate"/> calls from this
        /// <see cref="UpdateHandle"/>.</param>
        /// <param name="callLimit">The amount of times this <see cref="CallAfterHandle"/> should be called. Defaults to
        /// <see cref="CALL_AFTER_DEFAULT_CALL_LIMIT"/></param>
        /// <returns>A reference to the <see cref="CallAfterHandle"/> to store for use later. (Complete, Dispose)</returns>
        public CallAfterHandle CallAfter(float targetDelta, Action callback, DeltaProvider deltaProvider, uint callLimit = CALL_AFTER_DEFAULT_CALL_LIMIT)
        {
            CallAfterHandle callAfterHandle = new CallAfterHandle(
                m_IDProvider.GetNextID(),
                callback,
                targetDelta,
                deltaProvider,
                callLimit
                );

            FinalizeHandle(callAfterHandle);

            return callAfterHandle;
        }

        /// <summary>
        /// Calls the provided callback after a number of <see cref="OnUpdate"/> ticks.
        /// This is a convenience wrapper for <see cref="CallAfter(float, Action, DeltaProvider, uint)"/>
        /// CallAfterHandles are managed by the UpdateHandle and will be disposed if the UpdateHandle is disposed.
        /// </summary>
        /// <param name="updateCount">The number of <see cref="OnUpdate"/> ticks before the callback is fired.</param>
        /// <param name="callback">The callback function to fire.</param>
        /// <param name="callLimit">The amount of times this <see cref="CallAfterHandle"/> should be called. Defaults to
        /// <see cref="CALL_AFTER_DEFAULT_CALL_LIMIT"/></param>
        /// <returns>A reference to the <see cref="CallAfterHandle"/> to store for use later. (Complete, Dispose)</returns>
        public CallAfterHandle CallAfterUpdates(int updateCount, Action callback, uint callLimit = CALL_AFTER_DEFAULT_CALL_LIMIT)
        {
            return CallAfter(updateCount, callback, FixedDeltaProvider, callLimit);
        }

        private void FinalizeHandle(CallAfterHandle callAfterHandle)
        {
            callAfterHandle.OnDisposing += CallAfterHandle_OnDisposing;
            m_CallAfterHandles.Add(callAfterHandle.ID, callAfterHandle);
            ValidateUpdateSourceHook();
        }

        private void CallAfterHandle_OnDisposing(CallAfterHandle callAfterHandle)
        {
            Debug.Assert(m_CallAfterHandles.ContainsKey(callAfterHandle.ID), $"Tried to remove Call After Handle with ID {callAfterHandle.ID} but it didn't exist in the lookup!");

            m_CallAfterHandles.Remove(callAfterHandle.ID);
            ValidateUpdateSourceHook();
        }

    }
}