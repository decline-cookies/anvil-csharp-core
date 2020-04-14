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
        
        //TODO: Look into how we might be able to use Generics to strongly type this. See: https://github.com/jkeon/anvil-csharp-core/pull/4#discussion_r407669597
        /// <summary>
        /// Optional Shared Data to store with this Command. Useful when used with <see cref="AbstractCollectionCommand"/>s.
        /// <seealso cref="SetSharedData"/>
        /// </summary>
        public object SharedData { get; private set; }

        /// <summary>
        /// Optional Parent Collection Command to nest Commands as children of each other.
        /// </summary>
        public AbstractCollectionCommand ParentCollectionCommand
        {
            get => m_ParentCollectionCommand;
            internal set
            {
                if (State != CommandState.Initialized)
                {
                    throw new Exception($"Tried to set the ParentCommand on {this} but State was {State} instead of Initialized!");
                }

                m_ParentCollectionCommand = value;
            }
        }

        private AbstractCollectionCommand m_ParentCollectionCommand;
        

        protected AbstractCommand()
        {
        }

        protected override void DisposeSelf()
        {
            if (State == CommandState.Disposed)
            {
                return;
            }
            
            State = CommandState.Disposed;
            ParentCollectionCommand = null;
            SharedData = null;
            OnComplete = null;
            
            base.DisposeSelf();
        }

        public AbstractCommand SetSharedData(object sharedData)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to call SetSharedData on {this} but State was {State} instead of Initialized!");
            }

            SharedData = sharedData;
            
            return this;
        }

        public void Execute()
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tired to call Execute on {this} but State was {State} instead of Initialized!");
            }

            State = CommandState.Executing;
            ExecuteCommand();
        }

        protected abstract void ExecuteCommand();

        protected void CompleteCommand()
        {
            if (State != CommandState.Executing)
            {
                throw new Exception($"Tried to call CompleteCommand on {this} but State was {State} instead of Executing!");
            }
            State = CommandState.Completed;
            OnComplete?.Invoke(this);
            Dispose();
        }
    }
}

