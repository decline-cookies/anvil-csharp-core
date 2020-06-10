using System.Collections.Generic;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    public static class JSON
    {
        public static void OverrideParser(IJSONParser instance)
        {
            if (instance.Priority > s_Parser.Priority)
            {
                s_Parser = instance;
            }
        }

        private static IJSONParser s_Parser = new TinyJSONParser<Encoder, Decoder>();

        public static string Encode(object data, EncodeOptions options = EncodeOptions.None)
        {
            return s_Parser.Encode(data, options);
        }

        public static T Decode<T>(string jsonString)
        {
            return s_Parser.Decode<T>(jsonString);
        }

        public static void MakeInto<T>(Variant data, out T item)
        {
            s_Parser.MakeInto(data, out item);
        }
        
        public static void SupportTypeForAOT<T>()
        {
            s_Parser.SupportTypeForAOT<T>();
        }

        public static void SupportListTypeForAOT<TList, T>()
            where TList : IList<T>, new()
        {
            s_Parser.SupportListTypeForAOT<TList, T>();
        }

        public static void SupportDictionaryTypeForAOT<TDictionary, TKey, TValue>()
            where TDictionary : IDictionary<TKey, TValue>, new()
        {
            s_Parser.SupportDictionaryTypeForAOT<TDictionary, TKey, TValue>();
        }

        public static void SupportArrayTypeForAOT<TArray, T>()
        {
            s_Parser.SupportArrayTypeForAOT<TArray, T>();
        }

        public static void SupportMultiRankArrayTypeForAOT<TArray, T>()
        {
            s_Parser.SupportMultiRankArrayTypeForAOT<TArray, T>();
        }

    }
}

