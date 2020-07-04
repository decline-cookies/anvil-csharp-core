using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    public abstract class AbstractCollectionCommand<T, TChild> : AbstractCommand<T>
        where T:AbstractCollectionCommand<T, TChild>
        where TChild:class, ICommand
    {
        protected readonly List<TChild> m_ChildCommands = new List<TChild>();

        protected AbstractCollectionCommand(params TChild[] childCommands):this((IEnumerable<TChild>)childCommands)
        {
        }

        protected AbstractCollectionCommand(IEnumerable<TChild> childCommands)
        {
            AddChildren(childCommands);
        }

        protected override void DisposeSelf()
        {
            foreach (TChild command in m_ChildCommands)
            {
                command.Dispose();
            }
            m_ChildCommands.Clear();

            base.DisposeSelf();
        }

        /// <summary>
        /// Adds a child command to be executed in the collection.
        /// </summary>
        /// <param name="childCommand">The <see cref="{TChild}"/> to add to the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand{T}"/> the child was added to. Useful for method chaining.</returns>
        /// <exception cref="InvalidOperationException">Occurs when the <see cref="State"/> is not <see cref="cref="CommandState.Initialized"/></exception>
        public T AddChild(TChild childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new InvalidOperationException($"Tried to add child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }

            m_ChildCommands.Add(childCommand);

            return (T)this;
        }

        /// <summary>
        /// Adds an <see cref="IEnumerable{TChild}"/> to be executed in the collection.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{TChild}"/> to add to the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand{T}"/> the child was added to. Useful for method chaining.</returns>
        public T AddChildren(IEnumerable<TChild> childCommands)
        {
            foreach (TChild childCommand in childCommands)
            {
                AddChild(childCommand);
            }

            return (T)this;
        }
    }
}

