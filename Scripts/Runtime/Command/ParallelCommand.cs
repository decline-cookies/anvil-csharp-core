using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// A <see cref="AbstractCollectionCommand"/> that will execute all children in parallel.
    /// <see cref="OnComplete"/> will be dispatched once all children have completed.
    /// </summary>
    public class ParallelCommand : AbstractCollectionCommand
    {
        private int m_ChildCommandsLeftToComplete;
        
        /// <summary>
        /// Constructs a <see cref="ParallelCommand"/> using params for <see cref="AbstractCommand"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="AbstractCommand"/>s to pass in.</param>
        public ParallelCommand(params AbstractCommand[] childCommands) : base (childCommands)
        {
        }
        
        /// <summary>
        /// Constructs a <see cref="ParallelCommand"/> using an <see cref="IEnumerable{AbstractCommand}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{AbstractCommand}"/> to pass in.</param>
        public ParallelCommand(IEnumerable<AbstractCommand> childCommands) : base(childCommands)
        {
        }

        protected override void DisposeSelf()
        {
            m_ChildCommandsLeftToComplete = 0;
            
            base.DisposeSelf();
        }

        protected override void ExecuteCommand()
        {
            m_ChildCommandsLeftToComplete = m_ChildCommands.Count;
            
            if (m_ChildCommandsLeftToComplete == 0)
            {
                CompleteCommand();
                return;
            }
            
            foreach (AbstractCommand command in m_ChildCommands)
            {
                command.OnComplete += HandleChildCommandOnComplete;
                command.Execute();
            }
        }

        private void HandleChildCommandOnComplete(AbstractCommand command)
        {
            command.OnComplete -= HandleChildCommandOnComplete;
            m_ChildCommandsLeftToComplete--;
            if (m_ChildCommandsLeftToComplete == 0)
            {
                CompleteCommand();
            }
        }
    }
}

