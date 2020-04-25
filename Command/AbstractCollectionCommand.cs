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
                AddChild(childCommand);
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
        
        /// <summary>
        /// Adds a child command to be executed in the collection.
        /// </summary>
        /// <param name="childCommand">The <see cref="AbstractCommand"/> to add to the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was added to. Useful for method chaining.</returns>
        /// <exception cref="Exception">Occurs when the <see cref="State"/> is not CommandState.Initialized</exception>
        public AbstractCollectionCommand AddChild(AbstractCommand childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to add child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }
            
            m_ChildCommands.Add(childCommand);

            return this;
        }
        
        /// <summary>
        /// Adds an <see cref="IEnumerable{AbstractCommand}"/> to be executed in the collection.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{AbstractCommand}"/> to add to the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was added to. Useful for method chaining.</returns>
        public AbstractCollectionCommand AddChildren(IEnumerable<AbstractCommand> childCommands)
        {
            foreach (AbstractCommand childCommand in childCommands)
            {
                AddChild(childCommand);
            }

            return this;
        }
    }
}

