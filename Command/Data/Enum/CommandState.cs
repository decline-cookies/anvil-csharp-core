namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Enum for the state of a <see cref="AbstractCommand"/> as it goes through its lifecycle.
    /// </summary>
    public enum CommandState
    {
        /// <summary>
        /// Indicates the <see cref="AbstractCommand"/> has been constructed but not yet executed.
        /// </summary>
        Initialized,
        /// <summary>
        /// Indicates the <see cref="AbstractCommand"/> has been executed and is still in progress.
        /// </summary>
        Executing,
        /// <summary>
        /// Indicates the <see cref="AbstractCommand"/> has been completed.
        /// </summary>
        Completed
    }
}
