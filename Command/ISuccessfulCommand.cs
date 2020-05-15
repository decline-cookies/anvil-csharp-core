using System;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// An interface for commands that could fail at runtime. Ex: Server Calls.
    /// </summary>
    public interface ISuccessfulCommand : ICommand
    {
        /// <summary>
        /// Whether the command was successful or not.
        /// <returns>true if successful, false if there was a failure</returns>
        /// </summary>
        bool WasSuccessful { get; }

        /// <summary>
        /// If <see cref="WasSuccessful"/> returns false, then the Exception can provide the developer with details
        /// of the failure.
        /// <returns>The <see cref="AggregateException"/> that caused the command to not be successful.</returns>
        /// </summary>
        AggregateException Exception { get; }
    }
}

