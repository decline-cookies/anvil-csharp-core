using System;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Indicates a type which may act as the default log handler for <see cref="Log"/>.
    /// During static initialization, <see cref="Log"/> selects the handler with this attribute that has the highest priority.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultLogHandlerAttribute : Attribute
    {
        public uint Priority { get; }

        public DefaultLogHandlerAttribute(uint priority) => Priority = priority;
    }
}
