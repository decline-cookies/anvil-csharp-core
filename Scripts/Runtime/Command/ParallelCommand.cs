namespace Anvil.CSharp.Command
{
    public class ParallelCommand : AbstractCollectionCommand
    {
        private int m_ChildCommandsLeftToComplete;

        public ParallelCommand(params AbstractCommand[] commands) : base (commands)
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

