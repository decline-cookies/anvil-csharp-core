using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    public abstract class AbstractCollectionCommand : AbstractCommand
    {
        protected readonly List<AbstractCommand> m_ChildCommands = new List<AbstractCommand>();

        protected AbstractCollectionCommand(params AbstractCommand[] childCommands):this((IEnumerable<AbstractCommand>)childCommands)
        {
        }
        
        protected AbstractCollectionCommand(IEnumerable<AbstractCommand> childCommands)
        {
            foreach (AbstractCommand childCommand in childCommands)
            {
                AddChildCommand(childCommand);
            }
        }

        protected override void DisposeSelf()
        {
            foreach (AbstractCommand command in m_ChildCommands)
            {
                command.Dispose();
            }
            m_ChildCommands.Clear();
            
            base.DisposeSelf();
        }
        
        public AbstractCollectionCommand AddChildCommand(AbstractCommand childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to add child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }

            childCommand.ParentCollectionCommand = this;
            m_ChildCommands.Add(childCommand);

            return this;
        }

        public AbstractCollectionCommand AddChildCommands(IEnumerable<AbstractCommand> childCommands)
        {
            foreach (AbstractCommand childCommand in childCommands)
            {
                AddChildCommand(childCommand);
            }

            return this;
        }
    }
}

