using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    public abstract class AbstractCollectionCommand : AbstractCommand
    {
        protected readonly List<AbstractCommand> m_ChildCommands = new List<AbstractCommand>();

        protected AbstractCollectionCommand(params AbstractCommand[] commands)
        {
            foreach (AbstractCommand command in commands)
            {
                AddChildCommand(command);
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
        
        public AbstractCollectionCommand AddChildCommand(AbstractCommand command)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to add sub command {command} to {this} but State was {State} instead of Initialized!");
            }

            command.SetParentCommand(this);

            m_ChildCommands.Add(command);
            return this;
        }
    }
}

