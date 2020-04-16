using System;

namespace Anvil.CSharp.Content
{
    public interface IContent : IDisposable
    {
        AbstractContentController Controller { get; set;}
        bool IsContentDisposing { get; }
    }
}

