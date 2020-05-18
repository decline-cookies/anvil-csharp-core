using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// A <see cref="AbstractCollectionCommand"/> that will execute all children in parallel.
    /// <see cref="OnComplete"/> will be dispatched once all children have completed.
    /// </summary>
    public class ParallelCommand : AbstractCollectionCommand<ParallelCommand>
    {
        private int m_ChildCommandsLeftToComplete;

        /// <summary>
        /// Constructs a <see cref="ParallelCommand"/> using params for <see cref="ICommand"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="ICommand"/>s to pass in.</param>
        public ParallelCommand(params ICommand[] childCommands) : base (childCommands)
        {
        }

        /// <summary>
        /// Constructs a <see cref="ParallelCommand"/> using an <see cref="IEnumerable{ICommand}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{ICommand}"/> to pass in.</param>
        public ParallelCommand(IEnumerable<ICommand> childCommands) : base(childCommands)
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

            foreach (ICommand childCommand in m_ChildCommands)
            {
                childCommand.OnComplete += ChildCommand_OnComplete;
                childCommand.Execute();
            }
        }

        private void ChildCommand_OnComplete(ICommand childCommand)
        {
            childCommand.OnComplete -= ChildCommand_OnComplete;
            m_ChildCommandsLeftToComplete--;
            if (m_ChildCommandsLeftToComplete == 0)
            {
                CompleteCommand();
            }
        }
    }
}

