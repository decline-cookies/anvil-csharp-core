using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Content
{
    public abstract class AbstractContent : AbstractAnvilDisposable, IContent
    {
        public event Action OnContentDisposing;
        
        private bool m_IsContentDisposing;

        protected override void DisposeSelf()
        {
            if (m_IsContentDisposing)
            {
                return;
            }
            m_IsContentDisposing = true;
            
            OnContentDisposing?.Invoke();
            OnContentDisposing = null;

            base.DisposeSelf();
        }
    }
}

