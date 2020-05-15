namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Enum for the state of a <see cref="ICommand"/> as it goes through its lifecycle.
    /// </summary>
    public enum CommandState
    {
        /// <summary>
        /// Indicates the <see cref="ICommand"/> has been constructed but not yet executed.
        /// </summary>
        Initialized,
        /// <summary>
        /// Indicates the <see cref="ICommand"/> has been executed and is still in progress.
        /// </summary>
        Executing,
        /// <summary>
        /// Indicates the <see cref="ICommand"/> has been completed.
        /// </summary>
        Completed
    }
}
