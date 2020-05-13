using System;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// The base class for calling a function after some amount of time in the future.
    /// </summary>
    public class CallLaterTimeHandle : AbstractCallLaterHandle
    {
        private readonly float m_TargetTime;
        private float m_ElapsedTime;
        private DeltaTimeProvider m_DeltaTimeProvider;
        public CallLaterTimeHandle(uint id, Action callback, float targetTime, DeltaTimeProvider deltaTimeProvider) : base(id, callback)
        {
            m_TargetTime = targetTime;
            m_DeltaTimeProvider = deltaTimeProvider;
        }
        
        protected override void DisposeSelf()
        {
            m_DeltaTimeProvider = null;
            base.DisposeSelf();
        }

        internal override void Update()
        {
            if (m_DeltaTimeProvider == null)
            {
                Dispose();
                return;
            }
            
            m_ElapsedTime += m_DeltaTimeProvider();
            if (m_ElapsedTime >= m_TargetTime)
            {
                Complete();
            }
        }
    }
}

