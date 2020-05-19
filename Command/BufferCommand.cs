using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// The Buffer Command is an <see cref="ICommand"/> that is not intended to ever complete.
    /// Instead it "stays open" and allows for more child <see cref="ICommand"/>s to be added to it
    /// along with a priority. Commands will be executed one after the other in priority order until
    /// there are none left and then it will wait until more commands are added.
    /// </summary>
    public class BufferCommand : AbstractCommand<BufferCommand>
    {
        private readonly List<ICommand> m_ChildCommands = new List<ICommand>();
        private readonly List<float> m_ChildPriorities = new List<float>();

        private ICommand m_ActiveCommand;


        /// <summary>
        /// <see cref="BufferCommand"/> will throw an <see cref="InvalidOperationException"/> if the
        /// OnComplete event is subscribed to.
        /// </summary>
        public new event Action<BufferCommand> OnComplete
        {
            add => throw new InvalidOperationException($"Buffer Commands are designed to never complete!");
            remove => throw new InvalidOperationException($"Buffer Commands are designed to never complete!");
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that is initially empty.
        /// </summary>
        public BufferCommand()
        {

        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in
        /// <see cref="ICommand"/>/<see cref="float"/> tuples to initially populate.
        /// </summary>
        /// <param name="children">A set of <see cref="ICommand"/>/<see cref="float"/> tuple to populate with.</param>
        public BufferCommand(params (ICommand Command, float Priority)[] children):this((IEnumerable<(ICommand Command, float Priority)>)children)
        {
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in a set of
        /// <see cref="ICommand"/>s. Assumes a priority of 0.0f for each Command.
        /// </summary>
        /// <param name="childCommands">A set of <see cref="ICommand"/>s to populate with. They all have the same
        /// priority of 0.0f.</param>
        public BufferCommand(params ICommand[] childCommands):this((IEnumerable<ICommand>)childCommands)
        {
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in a
        /// <see cref="Enumerable{(ICommand, float)}"/> tuples to initially populate.
        /// </summary>
        /// <param name="children">An <see cref="IEnumerable{(ICommand, float)}"/> tuples to populate with.</param>
        public BufferCommand(IEnumerable<(ICommand Command, float Priority)> children)
        {
            AddChildren(children);
        }

        /// <summary>
        /// Creates a new instance of a <see cref="BufferCommand"/> that takes in a <see cref="IEnumerable{ICommand}"/>
        /// to populate with. Assumes a priority of 0.0f for each Command.
        /// </summary>
        /// <param name="childCommands">A set of <see cref="IEnumerable{ICommand}"/>s to populate with. They all have
        /// the same priority of 0.0f.</param>
        public BufferCommand(IEnumerable<ICommand> childCommands)
        {
            AddChildren(childCommands);
        }

        protected override void DisposeSelf()
        {
            foreach (ICommand command in m_ChildCommands)
            {
                command.Dispose();
            }
            m_ChildCommands.Clear();
            m_ChildPriorities.Clear();

            m_ActiveCommand = null;

            base.DisposeSelf();
        }

        /// <summary>
        /// Adds a <see cref="ICommand"/> to be executed as part of the <see cref="BufferCommand"/>.
        /// </summary>
        /// <param name="childCommand">The <see cref="ICommand"/> to execute.</param>
        /// <param name="priority">The priority of the <see cref="ICommand"/>.
        /// This is a <see cref="float"/> value where the higher the number, the higher the priority. Commands
        /// with the same priority will execute in the order they were added.</param>
        /// <param name="shouldRemoveFollowingCommands">Optional flag for whether all commands in the buffer AFTER the
        /// passed in childCommand should be removed and disposed. Useful for when you need to interrupt an
        /// existing queue of commands. Note that all commands removed will be disposed.</param>
        /// <returns>A reference to this <see cref="BufferCommand"/>. Useful for method chaining.</returns>
        public BufferCommand AddChild(ICommand childCommand, float priority = 0.0f, bool shouldRemoveFollowingCommands = false)
        {
            //Assume we're adding to the end of the queue
            int insertIndex = m_ChildCommands.Count;

            //Iterate through to find where we should insert based on our priority.
            //TODO: Might be able to optimize this to find the insertion faster? Wait until it matters. Likely only a few in the queue at a time.
            for (int i = 0; i < m_ChildCommands.Count; ++i)
            {
                float currentPriority = m_ChildPriorities[i];
                if (priority > currentPriority)
                {
                    insertIndex = i;
                    break;
                }
            }

            //Ensure Commands and Priority queues match.
            m_ChildCommands.Insert(insertIndex, childCommand);
            m_ChildPriorities.Insert(insertIndex, priority);

            if (shouldRemoveFollowingCommands)
            {
                //Normally could do a RemoveRange, but we want to ensure we Dispose
                for (int i = m_ChildCommands.Count - 1; i > insertIndex; --i)
                {
                    ICommand command = m_ChildCommands[i];
                    command.Dispose();
                    m_ChildCommands.RemoveAt(i);
                }
            }

            //If the Buffer has been started and we're not currently executing anything, we should kick off the next command
            if (State == CommandState.Executing && m_ActiveCommand == null)
            {
                ExecuteNextChildCommandInBuffer();
            }

            return this;
        }

        /// <summary>
        /// Adds a <see cref="IEnumerable{(ICommand, float}"/> set of tuples to the Buffer Command to be executed.
        /// </summary>
        /// <param name="children">The <see cref="IEnumerable{(ICommand, float}"/> set of tuples to add.</param>
        /// <returns>A reference to this <see cref="BufferCommand"/>. Useful for method chaining.</returns>
        public BufferCommand AddChildren(IEnumerable<(ICommand Command, float Priority)> children)
        {
            foreach ((ICommand command, float priority) in children)
            {
                AddChild(command, priority);
            }

            return this;
        }

        /// <summary>
        /// Adds a <see cref="IEnumerable{(ICommand}"/> set of tuples to the Buffer Command to be executed.
        /// All priorities for the Commands will be 0.0f
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

        protected override void ExecuteCommand()
        {
            ExecuteNextChildCommandInBuffer();
        }

        private void ExecuteNextChildCommandInBuffer()
        {
            if (m_ChildCommands.Count == 0)
            {
                return;
            }

            ICommand childCommand = m_ChildCommands[0];

            m_ChildCommands.RemoveAt(0);
            m_ChildPriorities.RemoveAt(0);

            m_ActiveCommand = childCommand;
            m_ActiveCommand.OnComplete += ChildCommand_OnComplete;
            m_ActiveCommand.Execute();
        }

        private void ChildCommand_OnComplete(ICommand childCommand)
        {
            childCommand.OnComplete -= ChildCommand_OnComplete;
            m_ActiveCommand = null;

            ExecuteNextChildCommandInBuffer();
        }
    }
}
