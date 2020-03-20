using System;

namespace Anvil.CSharp.DelayedExecution
{
    public interface ICallLaterHandle
    {
        uint ID
        {
            get;
        }

        event Action<ICallLaterHandle> OnDisposing;
        
        //TODO: Pause/Resume?
        void Cancel();
        void Complete();
    }
}

