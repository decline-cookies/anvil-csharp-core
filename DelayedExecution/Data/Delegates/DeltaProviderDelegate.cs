namespace Anvil.CSharp.DelayedExecution
{
    /// <summary>
    /// Delegate for a function that returns a float value for how much "time" has elapsed each occurence of a
    /// <see cref="CallAfterHandle"/> being updated.
    /// This allows for custom logic to provide a delta time dependent on different clocks or methods of calculating
    /// a delta between updates.
    /// </summary>
    public delegate float DeltaProvider();
}
