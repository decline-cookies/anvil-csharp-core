using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Default <see cref="BufferCommand{T}"/> that uses <see cref="ICommand"/> as the restriction on
    /// children types.
    /// </summary>
    public class BufferCommand : BufferCommand<ICommand>
    {
    }

    /// <summary>
    /// The Buffer Command is an <see cref="ICommand"/> that is not intended to ever complete.
    /// Instead it "stays open" and allows for more child <see cref="{T}"/>s to be added to it.
    /// Commands will be executed one after the other in order until
    /// there are none left and then it will wait until more commands are added.
    /// </summary>
    public class BufferCommand<T> : AbstractCommand<BufferCommand<T>>
        where T:class, ICommand
    {
        /// <summary>
        /// Dispatches when the <see cref="BufferCommand"/> is idle, not executing any commands and has none left in
        /// the buffer. It is able to accept more commands at any time.
        /// </summary>
        public event Action<BufferCommand<T>> OnBufferIdle;

        /// <summary>
        /// <see cref="BufferCommand"/> will throw an <see cref="NotSupportedException"/> if the
        /// OnComplete event is subscribed to.
        /// </summary>
        /// <exception cref="NotSupportedException">Always thrown.</exception>
        [Obsolete("Buffer Commands are designed to never complete!", true)]
        public new event Action<BufferCommand<T>> OnComplete
        {
            add => throw new NotSupportedException($"Buffer Commands are designed to never complete!");
            remove => throw new NotSupportedException($"Buffer Commands are designed to never complete!");
        }

        protected readonly Queue<T> m_ChildCommands = new Queue<T>();
        /// <summary>
        /// The currently executing child command
        /// </summary>
        /// <remarks>
        /// Do not set outside of <see cref="BufferCommand"/>. This should only be used for direct access optimization
        /// to circumvent <see cref="CurrentChild"/> getter overhead.
        /// </remarks>
        protected T m_CurrentChild;

        /// <summary>
        /// The currently executing child command.
        /// </summary>
        public T CurrentChild
        {
            get => m_CurrentChild;
        }

        /// <summary>
        /// Is true when the <see cref="BufferCommand"/> is idle, not executing any commands and has none left in
        /// the buffer. It is able to accept more commands at any time.
        /// </summary>
        public bool IsBufferIdle { get; private set; } = true;

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that is initially empty.
        /// </summary>
        public BufferCommand()
        {
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in a set of
        /// <see cref="{T}"/>s.
        /// </summary>
        /// <param name="childCommands">A set of <see cref="{T}"/>s to populate with.</param>
        public BufferCommand(params T[] childCommands):this((IEnumerable<T>)childCommands)
        {
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in a <see cref="IEnumerable{T}"/>
        /// to populate with.
        /// </summary>
        /// <param name="childCommands">A set of <see cref="IEnumerable{T}"/>s to populate with.</param>
        public BufferCommand(IEnumerable<T> childCommands)
        {
            m_ChildCommands = new Queue<T>(childCommands);
        }

        protected override void DisposeSelf()
        {
            Clear();
            OnBufferIdle = null;

            base.DisposeSelf();
        }

        /// <summary>
        /// Adds a <see cref="T"/> to be executed as part of the <see cref="BufferCommand"/>.
        /// </summary>
        /// <param name="childCommand">The <see cref="T"/> to execute.</param>
        /// <returns>A reference to this <see cref="BufferCommand"/>. Useful for method chaining.</returns>
        public BufferCommand<T> AddChild(T childCommand)
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
        /// Adds a <see cref="IEnumerable{(T}"/> to the Buffer Command to be executed.
        /// </summary>
        /// <param name="children">The <see cref="IEnumerable{(T}"/> to add.</param>
        /// <returns>A reference to this <see cref="BufferCommand"/>. Useful for method chaining.</returns>
        public BufferCommand<T> AddChildren(IEnumerable<T> childCommands)
        {
            foreach (T childCommand in childCommands)
            {
                AddChild(childCommand);
            }

            return this;
        }

        /// <summary>
        /// Clears all commands in the <see cref="BufferCommand"/> and disposes them.
        /// Will not dispatch <see cref="OnBufferIdle"/>.
        /// </summary>
        /// <param name="excludeCurrentChild">
        /// If true, the currently executing child will not be cleared/disposed and it will continue to execute.
        /// </param>
        public void Clear(bool excludeCurrentChild = false)
        {
            if (excludeCurrentChild && m_CurrentChild != null)
            {
                m_ChildCommands.Dequeue();
            }

            foreach (T childCommand in m_ChildCommands)
            {
                childCommand.Dispose();
            }
            m_ChildCommands.Clear();

            if (excludeCurrentChild && m_CurrentChild != null)
            {
                m_ChildCommands.Enqueue(m_CurrentChild);
            }
            else
            {
                m_CurrentChild = null;
            }
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
            IsBufferIdle = false;
            m_CurrentChild = m_ChildCommands.Peek();

            m_CurrentChild.OnComplete += ChildCommand_OnComplete;
            m_CurrentChild.Execute();
        }

        private void ChildCommand_OnComplete(ICommand childCommand)
        {
            Debug.Assert(childCommand == m_ChildCommands.Peek());
            Debug.Assert(m_CurrentChild == childCommand);

            childCommand.OnComplete -= ChildCommand_OnComplete;

            m_ChildCommands.Dequeue();

            m_CurrentChild = null;

            if (m_ChildCommands.Count > 0)
            {
                ExecuteNextChildCommandInBuffer();
            }
            else
            {
                IsBufferIdle = true;
                OnBufferIdle?.Invoke(this);
            }
        }
    }
}