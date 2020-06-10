using System;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    /// <summary>
    /// Used to convert JSON text to strong typed objects
    /// </summary>
    public interface IDecoder : IDisposable
    {
        /// <summary>
        /// Decodes a given JSON string into strongly typed objects
        /// </summary>
        /// <param name="jsonString">The JSON string to decode</param>
        /// <returns>A <see cref="Variant"/> which is a proxy object to then apply to a strongly typed object</returns>
        Variant Decode(string jsonString);
    }
}
