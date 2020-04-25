namespace Anvil.CSharp.Command
{
    /// <summary>
    /// An interface for commands that allows them to be cancelled mid execution.
    /// </summary>
    public interface ICancelableCommand
    {
        /// <summary>
        /// Whether a command was cancelled or not.
        /// <returns>true if cancelled, false if not.</returns>
        /// </summary>
        bool WasCancelled { get; }
        /// <summary>
        /// Tells the command to cancel. Developer must handle and implement accordingly.
        /// </summary>
        void Cancel();
    }
}

