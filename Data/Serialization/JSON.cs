using System.Collections.Generic;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    /// <summary>
    /// Class to access all JSON functionality
    /// </summary>
    public static class JSON
    {
        /// <summary>
        /// Allows for overriding the <see cref="IJSONParser"/> used by all JSON functionality.
        /// </summary>
        /// <param name="instance">The <see cref="IJSONParser"/> to override with.</param>
        public static void OverrideParser(IJSONParser instance)
        {
            if (instance.Priority > s_Parser.Priority)
            {
                s_Parser = instance;
            }
        }

        private static IJSONParser s_Parser = new TinyJSONParser<Encoder, Decoder>();

        /// <inheritdoc cref="IJSONParser.Encode"/>
        public static string Encode(object data, EncodeOptions options = EncodeOptions.None)
        {
            return s_Parser.Encode(data, options);
        }

        /// <inheritdoc cref="IJSONParser.Decode"/>
        public static T Decode<T>(string jsonString)
        {
            return s_Parser.Decode<T>(jsonString);
        }

        /// <inheritdoc cref="IJSONParser.MakeInto"/>
        public static void MakeInto<T>(Variant data, out T item)
        {
            s_Parser.MakeInto(data, out item);
        }

        /// <inheritdoc cref="IJSONParser.SupportTypeForAOT{T}"/>
        public static void SupportTypeForAOT<T>()
        {
            s_Parser.SupportTypeForAOT<T>();
        }

        /// <inheritdoc cref="IJSONParser.SupportListTypeForAOT{TList, T}"/>
        public static void SupportListTypeForAOT<TList, T>()
            where TList : IList<T>, new()
        {
            s_Parser.SupportListTypeForAOT<TList, T>();
        }

        /// <inheritdoc cref="IJSONParser.SupportDictionaryTypeForAOT{TDictionary, TKey, TValue}"/>
        public static void SupportDictionaryTypeForAOT<TDictionary, TKey, TValue>()
            where TDictionary : IDictionary<TKey, TValue>, new()
        {
            s_Parser.SupportDictionaryTypeForAOT<TDictionary, TKey, TValue>();
        }

        /// <inheritdoc cref="IJSONParser.SupportArrayTypeForAOT{TArray, T}"/>
        public static void SupportArrayTypeForAOT<TArray, T>()
        {
            s_Parser.SupportArrayTypeForAOT<TArray, T>();
        }

        /// <inheritdoc cref="IJSONParser.SupportMultiRankArrayTypeForAOT{TArray, T}"/>
        public static void SupportMultiRankArrayTypeForAOT<TArray, T>()
        {
            s_Parser.SupportMultiRankArrayTypeForAOT<TArray, T>();
        }

    }
}

