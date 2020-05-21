using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// The Buffer Command is an <see cref="ICommand"/> that is not intended to ever complete.
    /// Instead it "stays open" and allows for more child <see cref="ICommand"/>s to be added to it.
    /// Commands will be executed one after the other in order until
    /// there are none left and then it will wait until more commands are added.
    /// </summary>
    public class BufferCommand : AbstractCommand<BufferCommand>
    {
        /// <summary>
        /// Dispatches when the buffer of commands is empty. The <see cref="BufferCommand"/> will be in an idle state
        /// and able to accept more commands at any time.
        /// </summary>
        public event Action<BufferCommand> OnBufferEmpty;

        private readonly Queue<ICommand> m_ChildCommands = new Queue<ICommand>();
        private ICommand m_ActiveCommand;

        /// <summary>
        /// <see cref="BufferCommand"/> will throw an <see cref="NotSupportedException"/> if the
        /// OnComplete event is subscribed to.
        /// </summary>
        [Obsolete("Buffer Commands are designed to never complete!", true)]
        public new event Action<BufferCommand> OnComplete
        {
            add => throw new NotSupportedException($"Buffer Commands are designed to never complete!");
            remove => throw new NotSupportedException($"Buffer Commands are designed to never complete!");
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that is initially empty.
        /// </summary>
        public BufferCommand()
        {

        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in a set of
        /// <see cref="ICommand"/>s.
        /// </summary>
        /// <param name="childCommands">A set of <see cref="ICommand"/>s to populate with.</param>
        public BufferCommand(params ICommand[] childCommands):this((IEnumerable<ICommand>)childCommands)
        {
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in a <see cref="IEnumerable{ICommand}"/>
        /// to populate with.
        /// </summary>
        /// <param name="childCommands">A set of <see cref="IEnumerable{ICommand}"/>s to populate with.</param>
        public BufferCommand(IEnumerable<ICommand> childCommands)
        {
            AddChildren(childCommands);
        }

        protected override void DisposeSelf()
        {
            Clear();
            OnBufferEmpty = null;

            m_ActiveCommand?.Dispose();
            m_ActiveCommand = null;

            base.DisposeSelf();
        }

        /// <summary>
        /// Adds a <see cref="ICommand"/> to be executed as part of the <see cref="BufferCommand"/>.
        /// </summary>
        /// <param name="childCommand">The <see cref="ICommand"/> to execute.</param>
        /// <returns>A reference to this <see cref="BufferCommand"/>. Useful for method chaining.</returns>
        public BufferCommand AddChild(ICommand childCommand)
        {
            m_ChildCommands.Enqueue(childCommand);

            //If the Buffer has been started and we're not currently executing anything, we should kick off the next command
            if (State == CommandState.Executing && m_ActiveCommand == null)
            {
                ExecuteNextChildCommandInBuffer();
            }

            return this;
        }

        /// <summary>
        /// Adds a <see cref="IEnumerable{(ICommand}"/> to the Buffer Command to be executed.
        /// </summary>
        /// <param name="children">The <see cref="IEnumerable{(ICommand}"/> to add.</param>
        /// <returns>A reference to this <see cref="BufferCommand"/>. Useful for method chaining.</returns>
        public BufferCommand AddChildren(IEnumerable<ICommand> childCommands)
        {
            foreach (ICommand childCommand in childCommands)
            {
                AddChild(childCommand);
            }

            return this;
        }

        /// <summary>
        /// Clears all queued commands in the <see cref="BufferCommand"/> and disposes them.
        /// Will dispatch <see cref="BufferCommand.OnBufferEmpty"/> so long as the <see cref="BufferCommand"/> is
        /// not Disposing or Disposed.
        ///
        /// This function will do no work and will not dispatch <see cref="BufferCommand.OnBufferEmpty"/> if the buffer
        /// is already empty.
        ///
        /// The active command currently being executed will not be affected by this function.
        /// </summary>
        public void Clear()
        {
            //Early return to prevent duplicate firing of OnBufferEmpty
            if (m_ChildCommands.Count == 0)
            {
                return;
            }

            foreach (ICommand childCommand in m_ChildCommands)
            {
                childCommand.Dispose();
            }
            m_ChildCommands.Clear();

            if (!IsDisposing && !IsDisposed)
            {
                OnBufferEmpty?.Invoke(this);
            }
        }

        protected override void ExecuteCommand()
        {
            ExecuteNextChildCommandInBuffer();
        }

        private void ExecuteNextChildCommandInBuffer()
        {
            //Early return if the buffer is empty and we want to idle.
            if (m_ChildCommands.Count == 0)
            {
                return;
            }

            //Kick off the next command in the buffer
            ICommand childCommand = m_ChildCommands.Dequeue();

            m_ActiveCommand = childCommand;
            m_ActiveCommand.OnComplete += ChildCommand_OnComplete;
            m_ActiveCommand.Execute();

            //Second check to see if that was the last command in the buffer to alert that the buffer is empty
            if (m_ChildCommands.Count == 0)
            {
                OnBufferEmpty?.Invoke(this);
            }
        }

        private void ChildCommand_OnComplete(ICommand childCommand)
        {
            childCommand.OnComplete -= ChildCommand_OnComplete;
            m_ActiveCommand = null;

            ExecuteNextChildCommandInBuffer();
        }
    }
}
