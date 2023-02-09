using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Default <see cref="ParallelCommand{T}"/> that uses <see cref="ICommand"/> as the restriction on
    /// children types.
    /// </summary>
    public class ParallelCommand : ParallelCommand<ICommand> { }

    /// <summary>
    /// A <see cref="AbstractCollectionCommand"/> that will execute all children in parallel.
    /// <see cref="OnComplete"/> will be dispatched once all children have completed.
    /// </summary>
    public class ParallelCommand<T> : AbstractCollectionCommand<ParallelCommand<T>, T>
        where T : class, ICommand
    {
        private int m_ChildCommandsLeftToComplete;

        /// <summary>
        /// The currently executing children commands.
        /// </summary>
        public IReadOnlyList<T> CurrentChildren => m_ChildCommands.AsReadOnly();

        /// <summary>
        /// Constructs a <see cref="ParallelCommand"/> using params for <see cref="{T}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="{T}"/>s to pass in.</param>
        public ParallelCommand(params T[] childCommands) : base(childCommands) { }

        /// <summary>
        /// Constructs a <see cref="ParallelCommand"/> using an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{T}"/> to pass in.</param>
        public ParallelCommand(IEnumerable<T> childCommands) : base(childCommands) { }

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

            foreach (T childCommand in m_ChildCommands)
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
