using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Command
{
    public abstract class AbstractCommand : AbstractAnvilDisposable
    {
        public event Action<AbstractCommand> OnComplete;

        protected override void DisposeSelf()
        {
            OnComplete = null;
            base.DisposeSelf();
        }

        public virtual void Execute()
        {
            
        }

        protected void CompleteCommand()
        {
            OnComplete?.Invoke(this);
        }
    }
}

