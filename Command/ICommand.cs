using Anvil.CSharp.Core;
using System;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Interface for the concept of a Command in the Anvil Framework.
    /// A Command provides the flow for executing a discrete piece of logic and will dispose itself once complete.
    /// </summary>
    public interface ICommand : IAnvilDisposable
    {
        /// <summary>
        /// Dispatches when the Command is complete.
        /// Passes along itself <see cref="ICommand"/> for easy access to the results of the command.
        /// </summary>
        event Action<ICommand> OnComplete;

        /// <summary>
        /// Dispatches when the Command is disposing.
        /// Passes along itself <see cref="ICommand"/> for easy access to the results of the command.
        /// </summary>
        event Action<ICommand> OnDisposing;

        /// <summary>
        /// The current <see cref="CommandState"/> of the Command.
        /// </summary>
        CommandState State { get; }

        /// <summary>
        /// Begins execution of the command.
        /// </summary>
        void Execute();
    }
}
