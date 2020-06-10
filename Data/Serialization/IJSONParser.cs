using System.Collections.Generic;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    public interface IJSONParser
    {
        /// <summary>
        /// The priority of this parser. Higher values can override an existing parser.
        /// <see cref="JSON"/>
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Encodes an object into a JSON string representation.
        /// </summary>
        /// <param name="data">The <see cref="object"/> to encode.</param>
        /// <param name="options">The <see cref="EncodeOptions"/> to use when encoding.</param>
        /// <returns>The JSON <see cref="string"/> representing the object.</returns>
        string Encode(object data, EncodeOptions options);

        /// <summary>
        /// Decodes a JSON string into a strongly typed object.
        /// </summary>
        /// <param name="jsonString">The JSON string to decode.</param>
        /// <typeparam name="T">The type of the object the JSON string represents.</typeparam>
        /// <returns>An instance of the type of object that the JSON string represents.</returns>
        T Decode<T>(string jsonString);

        /// <summary>
        /// Converts from a proxy object <see cref="Variant"/> into the strongly typed version of the
        /// desired object.
        /// </summary>
        /// <param name="data">The <see cref="Variant"/> proxy object to convert.</param>
        /// <param name="item">The strongly typed version of the object to convert to.</param>
        /// <typeparam name="T">The type of the strongly typed version of the object.</typeparam>
        void MakeInto<T>(Variant data, out T item);

        /// <summary>
        /// Generates the code paths for AOT compilation for a given type.
        /// </summary>
        /// <typeparam name="T">The type to generate AOT code paths for.</typeparam>
        void SupportTypeForAOT<T>();

        /// <summary>
        /// Generates the code paths for AOT compilation for a given <see cref="IList"/> type.
        /// </summary>
        /// <typeparam name="TList">The <see cref="IList"/> type.</typeparam>
        /// <typeparam name="T">The type contained in the List.</typeparam>
        void SupportListTypeForAOT<TList, T>()
            where TList : IList<T>, new();

        /// <summary>
        /// Generates the code paths for AOT compilation for a given
        /// <see cref="IDictionary{TKey,TValue}"/> type.
        /// </summary>
        /// <typeparam name="TDictionary">The <see cref="IDictionary{TKey,TValue}"/> type.</typeparam>
        /// <typeparam name="TKey">The key type in the Dictionary</typeparam>
        /// <typeparam name="TValue">The value type in the Dictionary</typeparam>
        void SupportDictionaryTypeForAOT<TDictionary, TKey, TValue>()
            where TDictionary : IDictionary<TKey, TValue>, new();

        /// <summary>
        /// Generates the code paths for AOT compilation for a given Array type
        /// </summary>
        /// <typeparam name="TArray">The type of Array</typeparam>
        /// <typeparam name="T">The type contained in the Array</typeparam>
        void SupportArrayTypeForAOT<TArray, T>();

        /// <summary>
        /// Generates the code paths for AOT compilation for a given Multi Rank Array type.
        /// </summary>
        /// <typeparam name="TArray">The type of Array</typeparam>
        /// <typeparam name="T">The type contained in the Array</typeparam>
        void SupportMultiRankArrayTypeForAOT<TArray, T>();
    }
}
