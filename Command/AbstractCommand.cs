using System;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Extension of <see cref="AbstractCommand"> to allow for strong typing <see cref="ICommand"> events.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ICommand"/> to use.</typeparam>
    public abstract class AbstractCommand<T> : AbstractCommand
        where T : AbstractCommand<T>
    {
        /// <summary>
        /// <inheritdoc cref="ICommand.OnComplete"/>
        /// Strongly typed Command
        /// </summary>
        public new event Action<T> OnComplete;

        /// <summary>
        /// <inheritdoc cref="ICommand.OnDisposing"/>
        /// Strongly typed Command
        /// </summary>
        public new event Action<T> OnDisposing;

        protected override void DisposeSelf()
        {
            OnComplete = null;
            OnDisposing = null;

            base.DisposeSelf();
        }

        protected sealed override void DispatchOnComplete()
        {
            //The OnComplete{T} event in this class is a separate event to the OnComplete{ICommand}
            //in the base class. Anyone listening to ICommand.OnComplete would not be listening to
            //this OnComplete{T} and vice-versa. Therefore we need to ensure we dispatch both events
            //so that all listeners get notified properly.
            OnComplete?.Invoke((T)this);
            base.DispatchOnComplete();
        }

        protected sealed override void DispatchOnDisposing()
        {
            //The OnDisposing{T} event in this class is a separate event to the OnDisposing{ICommand}
            //in the base class. Anyone listening to ICommand.OnDisposing would not be listening to
            //this OnDisposing{T} and vice-versa. Therefore we need to ensure we dispatch both events
            //so that all listeners get notified properly.
            OnDisposing?.Invoke((T)this);
            base.DispatchOnDisposing();
        }
    }

    /// <summary>
    /// Concrete implementation of <see cref="ICommand"/>
    /// </summary>
    public abstract class AbstractCommand : AbstractAnvilDisposable, ICommand
    {
        /// <summary>
        /// <inheritdoc cref="ICommand.OnComplete"/>
        /// </summary>
        public event Action<ICommand> OnComplete;

        /// <summary>
        /// <inheritdoc cref="ICommand.OnDisposing"/>
        /// </summary>
        public event Action<ICommand> OnDisposing;

        /// <summary>
        /// <inheritdoc cref="ICommand.State"/>
        /// Should only be set by abstract commands that need to modify expected command flow.
        /// Ex: <see cref="AbstractCancelableCommand{T}"/>
        /// </summary>
        public CommandState State { get; private protected set; } = CommandState.Initialized;

        protected AbstractCommand()
        {
        }

        protected override void DisposeSelf()
        {
            OnComplete = null;
            DispatchOnDisposing();

            base.DisposeSelf();
        }

        /// <summary>
        /// <inheritdoc cref="ICommand.Execute"/>
        /// </summary>
        /// <remarks>Do not implement command logic here. Use <see cref="ExecuteCommand"/></remarks>
        /// <exception cref="InvalidOperationException">Occurs when the <see cref="State"/> is not <see cref="CommandState.Initialized"/></exception>
        public void Execute()
        {
            if (State != CommandState.Initialized)
            {
                throw new InvalidOperationException($"Tried to call {nameof(Execute)} on {this} but State was {State} instead of {CommandState.Initialized}!");
            }

            ExecuteInternal();
        }

        private protected virtual void ExecuteInternal()
        {
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
        /// <exception cref="InvalidOperationException">Occurs when <see cref="State"/> is not <see cref="CommandState.Executing"/></exception>
        protected void CompleteCommand()
        {
            if (State != CommandState.Executing)
            {
                throw new InvalidOperationException($"Tried to call {nameof(CompleteCommand)} on {this} but State was {State} instead of {CommandState.Executing}!");
            }

            State = CommandState.Completed;
            DispatchOnComplete();
            Dispose();
        }

        protected virtual void DispatchOnComplete()
        {
            OnComplete?.Invoke(this);
        }

        protected virtual void DispatchOnDisposing()
        {
            OnDisposing?.Invoke(this);
        }
    }
}

