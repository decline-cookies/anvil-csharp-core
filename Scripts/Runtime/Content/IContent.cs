using System;

namespace Anvil.CSharp.Content
{
    public interface IContent : IDisposable
    {
        event Action OnContentDisposing;
    }
}

