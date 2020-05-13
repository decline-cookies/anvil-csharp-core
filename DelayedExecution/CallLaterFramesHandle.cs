using System;

namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// The base class for calling a function after some amount of time in the future.
    /// </summary>
    public class CallLaterFramesHandle : AbstractCallLaterHandle
    {
        private readonly int m_TargetFrames;
        private int m_ElapsedFrames;
        private DeltaFramesProvider m_DeltaFramesProvider;
        public CallLaterFramesHandle(uint id, Action callback, int targetFrames, DeltaFramesProvider deltaFramesProvider) : base(id, callback)
        {
            m_TargetFrames = targetFrames;
            m_DeltaFramesProvider = deltaFramesProvider;
        }
        
        protected override void DisposeSelf()
        {
            m_DeltaFramesProvider = null;
            base.DisposeSelf();
        }

        internal override void Update()
        {
            if (m_DeltaFramesProvider == null)
            {
                Dispose();
                return;
            }
            
            m_ElapsedFrames += m_DeltaFramesProvider();
            if (m_ElapsedFrames >= m_TargetFrames)
            {
                Complete();
            }
        }
    }
}

