using System;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Indicates a type which may act as the default log listener for <see cref="Log"/>.
    /// During static initialization, <see cref="Log"/> instantiates all listeners with this attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultLogListenerAttribute : Attribute
    {
        public DefaultLogListenerAttribute() { }
    }
}
