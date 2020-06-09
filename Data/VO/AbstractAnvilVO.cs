using Anvil.CSharp.Core;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    /// <summary>
    /// The base class for all VO data structures in the Anvil Framework
    /// </summary>
    public abstract class AbstractAnvilVO : AbstractAnvilDisposable
    {
        /// <summary>
        /// Default <see cref="EncodeOptions"/> set to <see cref="EncodeOptions.PrettyPrint"/>
        /// This allows for easy human reading and seeing changes in a diff nicely.
        /// </summary>
        public const EncodeOptions DEFAULT_ENCODE_OPTIONS = EncodeOptions.PrettyPrint;

        /// <summary>
        /// Converts the VO to a JSON string representation.
        /// </summary>
        /// <param name="encodeOptions">The <see cref="EncodeOptions"/> to use for formatting the JSON string.</param>
        /// <returns>A JSON string representing the VO</returns>
        public string ToJSON(EncodeOptions encodeOptions = DEFAULT_ENCODE_OPTIONS)
        {
            return JSON.Encode(this, encodeOptions);
        }
    }
}
