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
        /// Dispatches when the <see cref="BufferCommand"/> is idle and not executing any commands and has none left in
        /// the buffer. It is able to accept more commands at any time.
        /// </summary>
        public event Action<BufferCommand> OnBufferIdle;

        private readonly Queue<ICommand> m_ChildCommands = new Queue<ICommand>();

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
            OnBufferIdle = null;

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

            //If the Buffer has been started and this is the first command in the buffer, we should kick it off.
            if (State == CommandState.Executing && m_ChildCommands.Count == 1)
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
        /// Clears all commands in the <see cref="BufferCommand"/> and disposes them.
        /// Will not dispatch <see cref="OnBufferIdle"/>.
        /// </summary>
        public void Clear()
        {
            foreach (ICommand childCommand in m_ChildCommands)
            {
                childCommand.Dispose();
            }
            m_ChildCommands.Clear();
        }

        protected override void ExecuteCommand()
        {
            //Check in case Execute was called after creation with no additions. AddChild handles the check for the only
            //other way ExecuteNextChildCommandInBuffer can be called with no commands in the queue.
            if (m_ChildCommands.Count == 0)
            {
                return;
            }
            ExecuteNextChildCommandInBuffer();
        }

        private void ExecuteNextChildCommandInBuffer()
        {
            ICommand childCommand = m_ChildCommands.Peek();
            childCommand.OnComplete += ChildCommand_OnComplete;
            childCommand.Execute();
        }

        private void ChildCommand_OnComplete(ICommand childCommand)
        {
            childCommand.OnComplete -= ChildCommand_OnComplete;
            m_ChildCommands.Dequeue();

            if (m_ChildCommands.Count > 0)
            {
                ExecuteNextChildCommandInBuffer();
            }
            else
            {
                OnBufferIdle?.Invoke(this);
            }
        }
    }
}
