using TinyJSON;

namespace Anvil.CSharp.Data
{
    public interface IJSONParser
    {
        int Priority { get; }
        string Encode(object data, EncodeOptions options);
        T Decode<T>(string jsonString);
        void MakeInto<T>(Variant data, out T item);
    }
}

