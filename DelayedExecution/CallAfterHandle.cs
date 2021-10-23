using System;
using System.Diagnostics;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Encapsulates a function that will be called later at some point in the future.
    /// <see cref="UpdateHandle.CallAfter"/> for usage.
    /// </summary>
    public class CallAfterHandle : AbstractAnvilBase
    {
        /// <summary>
        /// Dispatched whenever this CallAfterHandle is disposing so that the owning <see cref="UpdateHandle"/>
        /// can remove it from management.
        /// </summary>
        public event Action<CallAfterHandle> OnDisposing;

        internal readonly uint ID;

        private readonly float m_Target;
        private readonly uint m_CallLimit;

        private float m_Elapsed;
        private uint m_CallCount;

        private DeltaProvider m_DeltaProvider;
        private Action m_Callback;


        internal CallAfterHandle(
            uint id,
            Action callback,
            float target,
            DeltaProvider deltaProvider,
            uint callLimit = UpdateHandle.CALL_AFTER_DEFAULT_CALL_LIMIT)
        {
            ID = id;
            m_Callback = callback;
            m_Target = target;
            m_DeltaProvider = deltaProvider;
            m_CallLimit = callLimit;
            m_Elapsed = 0.0f;
            m_CallCount = 0;
        }

        protected override void DisposeSelf()
        {
            m_DeltaProvider = null;
            m_Callback = null;

            OnDisposing?.Invoke(this);
            OnDisposing = null;

            base.DisposeSelf();
        }

        /// <summary>
        /// Immediately completes the <see cref="CallAfterHandle"/> and invokes the callback function.
        /// If the CallAfterHandle should repeat, it will continue to do so. Complete only completes the current
        /// iteration.
        /// </summary>
        public void ForceComplete()
        {
            m_Elapsed = m_Target;
            Complete();
        }

        private void Complete()
        {
            m_CallCount++;
            m_Callback?.Invoke();

            Debug.Assert(m_CallCount <= m_CallLimit || m_CallLimit == UpdateHandle.CALL_AFTER_INFINITE_CALL_LIMIT, "Unless call limit is infinite call count should never exceed call limit");
            if (m_CallCount == m_CallLimit)
            {
                Dispose();
            }
            else
            {
                //Reset elapsed again accounting for any overages that might have happened.
                m_Elapsed -= m_Target;
            }
        }

        internal void Update()
        {
            Debug.Assert(m_DeltaProvider != null, $"Tried to run update on {this} but {nameof(m_DeltaProvider)} was null!");

            m_Elapsed += m_DeltaProvider();
            if (m_Elapsed < m_Target)
            {
                return;
            }

            Complete();
        }
    }
}

