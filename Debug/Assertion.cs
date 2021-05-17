using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Anvil.CSharp.Debugging
{
    /// <summary>
    /// A collection of convenient checks to make it easier/clearner for developers to assert
    /// complex conditions.
    /// </summary>
    public static class Assertion
    {
        /// <summary>
        /// Returns true if the executing method is being called by a method with the provided name.
        /// </summary>
        /// <param name="callerName">The method name of the expected caller.</param>
        /// <example>Debug.Assert(IsCaller(nameof(MyExpectedCallingMethod)))</example>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static bool IsCaller(string callerName)
        {
            // Look at the frame two levels up because that's the one calling the method
            // that calls this method.
            return new StackFrame(2).GetMethod().Name == callerName;
        }
    }
}
