using System;

namespace Anvil.CSharp.Logging
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DefaultLogHandlerAttribute : Attribute
    {
        public int Priority { get; }

        public DefaultLogHandlerAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
