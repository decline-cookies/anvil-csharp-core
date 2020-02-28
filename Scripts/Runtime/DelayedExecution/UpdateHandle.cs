using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.DelayedExecution
{
    public class UpdateHandle<T> : AnvilAbstractDisposable where T:AbstractUpdateSource
    {
        private AbstractUpdateSource m_UpdateSource;

        private AbstractUpdateSource UpdateSource
        {
            get
            {
                if (m_UpdateSource == null)
                {
                    Type sourceType = typeof(T);
                    m_UpdateSource = UpdateHandleSystem.GetUpdateSource(sourceType);
                }

                return m_UpdateSource;
            }
        }
        
        
        private UpdateSourcePipe m_UpdatePipe;

        private UpdateSourcePipe UpdatePipe
        {
            get
            {
                if (m_UpdatePipe == null)
                {
                    m_UpdatePipe = new UpdateSourcePipe(UpdateSource);
                }

                return m_UpdatePipe;
            }
        }

        public event Action OnUpdate
        {
            add => UpdatePipe.OnUpdate += value;
            remove => UpdatePipe.OnUpdate -= value;
        }

        protected override void DisposeSelf()
        {
            if (m_UpdatePipe != null)
            {
                m_UpdatePipe.Dispose();
                m_UpdatePipe = null;
            }
            m_UpdateSource = null;
            
            base.DisposeSelf();
        }
    }
}

