using System;
using System.Collections.Generic;

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
        
        public AbstractCollectionCommand InsertChildCommand(int index, AbstractCommand childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to insert child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }

            childCommand.ParentCollectionCommand = this;
            m_ChildCommands.Insert(index, childCommand);

            return this;
        }

        public AbstractCollectionCommand InsertChildCommands(int index, IEnumerable<AbstractCommand> childCommands)
        {
            foreach (AbstractCommand childCommand in childCommands)
            {
                InsertChildCommand(index, childCommand);
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