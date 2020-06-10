using System.Collections.Generic;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    public interface IJSONParser
    {
        int Priority { get; }
        string Encode(object data, EncodeOptions options);
        T Decode<T>(string jsonString);
        void MakeInto<T>(Variant data, out T item);

        void SupportTypeForAOT<T>();

        void SupportListTypeForAOT<TList, T>()
            where TList : IList<T>, new();

        void SupportDictionaryTypeForAOT<TDictionary, TKey, TValue>()
            where TDictionary : IDictionary<TKey, TValue>, new();

        void SupportArrayTypeForAOT<TArray, T>();
        void SupportMultiRankArrayTypeForAOT<TArray, T>();
    }
}

