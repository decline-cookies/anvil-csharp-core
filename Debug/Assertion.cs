using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Anvil.CSharp.Debugging
{
    public static class Assertion
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        [Conditional("DEBUG")]
        public static void IsCaller(string callerName, string message)
        {
            Debug.Assert(new StackFrame(2).GetMethod().Name == callerName, message);
        }
    }
}
