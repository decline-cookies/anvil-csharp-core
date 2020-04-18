using Anvil.CSharp.Data;

namespace Anvil.CSharp.Content
{
    /// <summary>
    /// A configuration data structure for an <see cref="AbstractContentGroup"/>.
    /// Extend this class to be able to customize inherited Content Groups.
    /// </summary>
    public class ContentGroupConfigVO : AbstractAnvilVO
    {
        /// <summary>
        /// The ID to identify the <see cref="AbstractContentGroup"/> by.
        /// </summary>
        public readonly string ID;

        /// <summary>
        /// Creates a new <see cref="ContentGroupConfigVO"/> for use in constructing <see cref="AbstractContentGroup"/>
        /// </summary>
        /// <param name="id">See <see cref="ID"/></param>
        public ContentGroupConfigVO(string id)
        {
            ID = id;
        }
    }
}

