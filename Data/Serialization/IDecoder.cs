using System;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    public interface IDecoder : IDisposable
    {
        Variant Decode(string jsonString);
    }
}

