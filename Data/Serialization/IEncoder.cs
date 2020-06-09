using System;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    public interface IEncoder : IDisposable
    {
        string Encode(object obj, EncodeOptions options = EncodeOptions.None);
    }
}

