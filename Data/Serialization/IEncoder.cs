using System;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    /// <summary>
    /// Used to convert strongly typed objects to a JSON string.
    /// </summary>
    public interface IEncoder : IDisposable
    {
        /// <summary>
        /// Encodes a given <see cref="object"/> to a JSON string.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to encode.</param>
        /// <param name="options">The <see cref="EncodeOptions"/> to use.</param>
        /// <returns>A JSON string representing the object</returns>
        string Encode(object obj, EncodeOptions options = EncodeOptions.None);
    }
}

