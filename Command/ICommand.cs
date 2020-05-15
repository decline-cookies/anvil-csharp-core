using System;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// A specialized version of <see cref="ICommand"/> to allow for strong typing of the Command
    /// returned in events.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ICommand"/> to use.</typeparam>
    public interface ICommand<out T> : ICommand
        where T:ICommand
    {
        /// <summary>
        /// <inheritdoc cref="ICommand.OnComplete"/>
        /// This enhances the event to be strong typed.
        /// </summary>
        new event Action<T> OnComplete;

        /// <summary>
        /// <inheritdoc cref="ICommand.OnDisposing"/>
        /// This enhances the event to be strong typed.
        /// </summary>
        new event Action<T> OnDisposing;
    }

    /// <summary>
    /// Interface for the concept of a Command in the Anvil Framework.
    /// A Command provides the flow for executing a discrete piece of logic and will dispose itself once complete.
    /// </summary>
    public interface ICommand : IDisposable
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

