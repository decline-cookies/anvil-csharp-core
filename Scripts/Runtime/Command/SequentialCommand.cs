using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// A <see cref="AbstractCollectionCommand"/> that will execute all children in sequence.
    /// <see cref="OnComplete"/> will be dispatched once all children have completed in order.
    /// </summary>
    public class SequentialCommand : AbstractCollectionCommand
    {
        private int m_ChildCommandIndex;
        /// <summary>
        /// Constructs a <see cref="SequentialCommand"/> using params for <see cref="AbstractCommand"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="AbstractCommand"/>s to pass in.</param>
        public SequentialCommand(params AbstractCommand[] childCommands) : base (childCommands)
        {
        }
        
        /// <summary>
        /// Constructs a <see cref="SequentialCommand"/> using an <see cref="IEnumerable{AbstractCommand}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{AbstractCommand}"/> to pass in.</param>
        public SequentialCommand(IEnumerable<AbstractCommand> childCommands) : base(childCommands)
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
        /// <param name="childCommand">The <see cref="AbstractCommand"/> to insert into the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was inserted into. Useful for method chaining.</returns>
        /// <exception cref="Exception">Occurs when the <see cref="State"/> is not CommandState.Initialized</exception>
        public AbstractCollectionCommand InsertChild(int index, AbstractCommand childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to insert child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }
            
            m_ChildCommands.Insert(index, childCommand);

            return this;
        }

        /// <summary>
        /// Inserts an <see cref="IEnumerable{AbstractCommand}"/> to be executed in the collection.
        /// </summary>
        /// <param name="index">The index for when the beginning of the childCommands should be executed.</param>
        /// <param name="childCommands">The <see cref="IEnumerable{AbstractCommand}"/> to insert into the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was inserted into. Useful for method chaining.</returns>
        public AbstractCollectionCommand InsertChildren(int index, IEnumerable<AbstractCommand> childCommands)
        {
            foreach (AbstractCommand childCommand in childCommands)
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
            AbstractCommand childCommand = m_ChildCommands[m_ChildCommandIndex];
            childCommand.OnComplete += HandleChildCommandOnComplete;
            childCommand.Execute();
        }

        private void HandleChildCommandOnComplete(AbstractCommand command)
        {
            command.OnComplete -= HandleChildCommandOnComplete;
            m_ChildCommandIndex++;
            ExecuteNextChildCommandInSequence();
        }
    }
}