namespace Anvil.CSharp.Command
{
    public class SequentialCommand : AbstractCollectionCommand
    {
        
        private int m_ChildCommandIndex;
        public SequentialCommand(params AbstractCommand[] commands) : base(commands)
        {
        }

        protected override void DisposeSelf()
        {
            m_ChildCommandIndex = 0;
            base.DisposeSelf();
        }

        protected override void ExecuteCommand()
        {
            m_ChildCommandIndex = 0;
            ExecuteNextChildCommandInSequence();
        }
        
        private void ExecuteNextChildCommandInSequence()
        {
            if (m_ChildCommands.Count <= m_ChildCommandIndex)
            {
                CompleteCommand();
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