using Anvil.CSharp.Core;
using TinyJSON;

namespace Anvil.CSharp.Data
{
    public abstract class AbstractAnvilVO : AbstractAnvilDisposable
    {
        private const EncodeOptions DEFAULT_ENCODE_OPTIONS = EncodeOptions.PrettyPrint;
        public string ToJSON(EncodeOptions encodeOptions = DEFAULT_ENCODE_OPTIONS)
        {
            return JSON.Dump(this, encodeOptions);
        }
    }
}