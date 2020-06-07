using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Default <see cref="SequentialCommand{T}"/> that uses <see cref="ICommand"/> as the restriction on
    /// children types.
    /// </summary>
    public class SequentialCommand : SequentialCommand<ICommand>
    {
    }

    /// <summary>
    /// A <see cref="AbstractCollectionCommand"/> that will execute all children in sequence.
    /// <see cref="OnComplete"/> will be dispatched once all children have completed in order.
    /// </summary>
    public class SequentialCommand<T> : AbstractCollectionCommand<SequentialCommand<T>, T>
        where T:class, ICommand
    {
        private int m_ChildCommandIndex;

        /// <summary>
        /// The currently executing child command.
        /// </summary>
        public T CurrentChild
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructs a <see cref="SequentialCommand"/> using params for <see cref="{T}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="{T}"/>s to pass in.</param>
        public SequentialCommand(params T[] childCommands) : base (childCommands)
        {
        }

        /// <summary>
        /// Constructs a <see cref="SequentialCommand"/> using an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{T}"/> to pass in.</param>
        public SequentialCommand(IEnumerable<T> childCommands) : base(childCommands)
        {
        }

        protected override void DisposeSelf()
        {
            m_ChildCommandIndex = 0;

            base.DisposeSelf();
        }

        /// <summary>
        /// Inserts a child command to be executed in the collection.
        /// </summary>
        /// <param name="index">The index for when the command should be executed.</param>
        /// <param name="childCommand">The <see cref="{T}"/> to insert into the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was inserted into. Useful for method chaining.</returns>
        /// <exception cref="Exception">Occurs when the <see cref="State"/> is not CommandState.Initialized</exception>
        public SequentialCommand<T> InsertChild(int index, T childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to insert child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }

            m_ChildCommands.Insert(index, childCommand);

            return this;
        }

        /// <summary>
        /// Inserts an <see cref="IEnumerable{T}"/> to be executed in the collection.
        /// </summary>
        /// <param name="index">The index for when the beginning of the childCommands should be executed.</param>
        /// <param name="childCommands">The <see cref="IEnumerable{T}"/> to insert into the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was inserted into. Useful for method chaining.</returns>
        public SequentialCommand<T> InsertChildren(int index, IEnumerable<T> childCommands)
        {
            foreach (T childCommand in childCommands)
            {
                InsertChild(index, childCommand);
                index++;
            }

            return this;
        }

        protected override void ExecuteCommand()
        {
            m_ChildCommandIndex = 0;
            ExecuteNextChildCommandInSequence();
        }

        private void ExecuteNextChildCommandInSequence()
        {
            if (m_ChildCommandIndex >= m_ChildCommands.Count)
            {
                CompleteCommand();
                return;
            }

            CurrentChild = m_ChildCommands[m_ChildCommandIndex];

            CurrentChild.OnComplete += ChildCommand_OnComplete;
            CurrentChild.Execute();
        }

        private void ChildCommand_OnComplete(ICommand childCommand)
        {
            childCommand.OnComplete -= ChildCommand_OnComplete;

            CurrentChild = null;
            m_ChildCommandIndex++;

            ExecuteNextChildCommandInSequence();
        }
    }
}
