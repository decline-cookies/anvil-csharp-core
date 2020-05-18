using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    public abstract class AbstractCollectionCommand<T> : AbstractCommand<T>
        where T:AbstractCollectionCommand<T>
    {
        protected readonly List<ICommand> m_ChildCommands = new List<ICommand>();

        protected AbstractCollectionCommand(params ICommand[] childCommands):this((IEnumerable<ICommand>)childCommands)
        {
        }

        protected AbstractCollectionCommand(IEnumerable<ICommand> childCommands)
        {
            AddChildren(childCommands);
        }

        protected override void DisposeSelf()
        {
            foreach (ICommand command in m_ChildCommands)
            {
                command.Dispose();
            }
            m_ChildCommands.Clear();

            base.DisposeSelf();
        }

        /// <summary>
        /// Adds a child command to be executed in the collection.
        /// </summary>
        /// <param name="childCommand">The <see cref="ICommand"/> to add to the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand{T}"/> the child was added to. Useful for method chaining.</returns>
        /// <exception cref="Exception">Occurs when the <see cref="State"/> is not CommandState.Initialized</exception>
        public T AddChild(ICommand childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to add child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }

            m_ChildCommands.Add(childCommand);

            return (T)this;
        }

        /// <summary>
        /// Adds an <see cref="IEnumerable{ICommand}"/> to be executed in the collection.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{ICommand}"/> to add to the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand{T}"/> the child was added to. Useful for method chaining.</returns>
        public T AddChildren(IEnumerable<ICommand> childCommands)
        {
            foreach (ICommand childCommand in childCommands)
            {
                AddChild(childCommand);
            }

            return (T)this;
        }
    }
}

