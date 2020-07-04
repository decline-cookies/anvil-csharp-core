namespace Anvil.CSharp.Command
{
    /// <summary>
    /// An interface for commands that allows them to be gracefully cancelled.
    /// </summary>
    public interface ICancelableCommand : ICommand
    {
        /// <summary>
        /// Whether a command was cancelled or not.
        /// <returns>true if cancelled, false if not.</returns>
        /// </summary>
        bool WasCancelled { get; }

        /// <summary>
        /// Tells the command to cancel. Developer must handle and implement accordingly.
        /// If currently executing the command is still expected to trigger <see cref="ICommand.OnComplete" />
        /// once it has gracefully cancelled.
        /// If the command hasn't yet executed the command is expected to immediately call
        /// <see cref="ICommand.OnComplete" /> when <see cref="ICommand.Execute"/> is called.
        /// </summary>
        void Cancel();
    }
}

