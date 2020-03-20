using System;

namespace Anvil.CSharp.DelayedExecution
{
    public interface IUpdateHandle
    {
        event Action OnUpdate;
    }
}

