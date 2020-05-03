using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Base class for the Command System, all commands will inherit off of this class.
    /// Provides the flow of logic for executing a discrete piece of logic and will dispose itself once complete.
    /// </summary>
    public abstract class AbstractCommand : AbstractAnvilDisposable
    {
        /// <summary>
        /// Dispatches when the Command is complete.
        /// Passes along itself <see cref="AbstractCommand"/> for easy access to the results of the command.
        /// </summary>
        public event Action<AbstractCommand> OnComplete;
        
        /// <summary>
        /// The current <see cref="CommandState"/> of the Command.
        /// </summary>
        public CommandState State { get; private set; } = CommandState.Initialized;


        protected AbstractCommand()
        {
        }

        protected override void DisposeSelf()
        {
            OnComplete = null;
            
            base.DisposeSelf();
        }
        
        /// <summary>
        /// Begins execution of the command.
        /// </summary>
        /// <exception cref="Exception">Occurs when the <see cref="State"/> is not CommandState.Initialized</exception>
        public void Execute()
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tired to call {nameof(Execute)} on {this} but State was {State} instead of {CommandState.Initialized}!");
            }

            State = CommandState.Executing;
            ExecuteCommand();
        }
        
        /// <summary>
        /// Called from <see cref="AbstractCommand.Execute"/>. Override and implement to execute the logic of the command.
        /// </summary>
        protected abstract void ExecuteCommand();
        
        /// <summary>
        /// Call when the command is complete. Command will auto dispose after dispatching <see cref="OnComplete"/>
        /// </summary>
        /// <exception cref="Exception">Occurs when <see cref="State"/> is not CommandState.Executing</exception>
        protected void CompleteCommand()
        {
            if (State != CommandState.Executing)
            {
                throw new Exception($"Tried to call {nameof(CompleteCommand)} on {this} but State was {State} instead of {CommandState.Executing}!");
            }
            State = CommandState.Completed;
            OnComplete?.Invoke(this);
            Dispose();
        }
    }
}

