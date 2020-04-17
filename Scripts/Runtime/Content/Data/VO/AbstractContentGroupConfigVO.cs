using Anvil.CSharp.Data;

namespace Anvil.CSharp.Content
{
    public abstract class AbstractContentGroupConfigVO : AbstractAnvilVO
    {
        public readonly string ID;

        protected AbstractContentGroupConfigVO(string id)
        {
            ID = id;
        }
    }
}

