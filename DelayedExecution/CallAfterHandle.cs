using System;
using System.Diagnostics;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Encapsulates a function that will be called later at some point in the future.
    /// <see cref="UpdateHandle.CallAfter"/> for usage.
    /// </summary>
    public class CallAfterHandle : AbstractAnvilDisposable
    {
        /// <summary>
        /// Dispatched whenever this CallAfterHandle is disposing so that the owning <see cref="UpdateHandle"/>
        /// can remove it from management.
        /// </summary>
        public event Action<CallAfterHandle> OnDisposing;

        internal readonly uint ID;

        private readonly float m_Target;
        private readonly int m_RepeatTarget;

        private float m_Elapsed;
        private int m_RepeatCount;

        private DeltaProvider m_DeltaProvider;
        private Action m_Callback;


        internal CallAfterHandle(
            uint id,
            Action callback,
            float target,
            DeltaProvider deltaProvider,
            int repeatTarget = UpdateHandle.CALL_AFTER_DEFAULT_REPEAT_LIMIT)
        {
            ID = id;
            m_Callback = callback;
            m_Target = target;
            m_DeltaProvider = deltaProvider;
            m_RepeatTarget = repeatTarget;
            m_Elapsed = 0.0f;
            m_RepeatCount = 0;
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
        /// <param name="shouldAlsoDispose">If true, the <see cref="CallAfterHandle"/> will be disposed after
        /// invoking the callback function. This will happen regardless if this CallAfterHandle should not
        /// repeat or if at the end of the amount of times it should repeat.</param>
        public void Complete(bool shouldAlsoDispose = false)
        {
            m_RepeatCount++;
            m_Callback?.Invoke();

            if (m_RepeatCount < m_RepeatTarget && !shouldAlsoDispose)
            {
                if (m_Elapsed >= m_Target)
                {
                    //Reset elapsed again accounting for any overages that might have happened.
                    m_Elapsed -= m_Target;
                }
                else
                {
                    //Special case where Complete was manually called before we actually got to that point in time.
                    //In this case if we subtracted the m_Target again we would have to wait longer than expected.
                    m_Elapsed = 0.0f;
                }
            }
            else
            {
                Dispose();
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

