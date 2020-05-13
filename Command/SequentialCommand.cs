using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    /// <summary>
    /// A <see cref="AbstractCollectionCommand"/> that will execute all children in sequence.
    /// <see cref="OnComplete"/> will be dispatched once all children have completed in order.
    /// </summary>
    public class SequentialCommand : AbstractCollectionCommand
    {
        /// <summary>
        /// Creates an instance of a <see cref="IJITInitCommand"/> that will instantiate a command of type
        /// <see cref="TToType"/> right before execution and provide reference to the previously completed command
        /// of type <see cref="TFromType"/>. The <see cref="jitInitFunction"/> is provided by the user and provides the
        /// "glue" to create the new command with data/results from the previous command.
        /// This is useful when needing to use the <see cref="SequentialCommand"/> constructors or
        /// <see cref="AddChildren"/> and <see cref="InsertChildren"/> functions.
        /// </summary>
        /// <param name="jitInitFunction"><inheritdoc cref="JITInitCommandFunction{TFromType,TToType}"/></param>
        /// <typeparam name="TFromType">The type of the previously completed command.</typeparam>
        /// <typeparam name="TToType">The type of the new command to initialize.</typeparam>
        /// <returns>An instance of <see cref="IJITInitCommand"/></returns>
        public static IJITInitCommand CreateJITInitCommand<TFromType, TToType>(JITInitCommandFunction<TFromType, TToType> jitInitFunction)
            where TFromType : AbstractCommand
            where TToType : AbstractCommand
        {
            JITInitCommand<TFromType, TToType> jitInitCommand = new JITInitCommand<TFromType, TToType>(jitInitFunction);
            return jitInitCommand;
        }

        private int m_ChildCommandIndex;
        private AbstractCommand m_PreviousChildCommand;

        /// <summary>
        /// Constructs a <see cref="SequentialCommand"/> using params for <see cref="AbstractCommand"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="AbstractCommand"/>s to pass in.</param>
        public SequentialCommand(params AbstractCommand[] childCommands) : base (childCommands)
        {
        }

        /// <summary>
        /// Constructs a <see cref="SequentialCommand"/> using an <see cref="IEnumerable{AbstractCommand}"/>.
        /// </summary>
        /// <param name="childCommands">The <see cref="IEnumerable{AbstractCommand}"/> to pass in.</param>
        public SequentialCommand(IEnumerable<AbstractCommand> childCommands) : base(childCommands)
        {
        }

        protected override void DisposeSelf()
        {
            m_PreviousChildCommand = null;
            m_ChildCommandIndex = 0;

            base.DisposeSelf();
        }

        /// <summary>
        /// Adds a child command to be executed in the collection. This child command will be instantiated just
        /// before execution.
        /// <see cref="CreateJITInitCommand{TFromType,TToType}"/>
        /// </summary>
        /// <param name="jitInitFunction"><inheritdoc cref="JITInitCommandFunction{TFromType,TToType}"/></param>
        /// <typeparam name="TFromType">The type of the previously completed command.</typeparam>
        /// <typeparam name="TToType">The type of the new command to initialize.</typeparam>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was added to. Useful for method chaining.</returns>
        public AbstractCollectionCommand AddChild<TFromType, TToType>(JITInitCommandFunction<TFromType, TToType> jitInitFunction)
            where TFromType : AbstractCommand
            where TToType : AbstractCommand
        {
            IJITInitCommand jitInitCommand = CreateJITInitCommand<TFromType, TToType>(jitInitFunction);

            return AddChild(jitInitCommand as AbstractCommand);;
        }

        /// <summary>
        /// Inserts a child command to be executed in the collection. This child command will be instantiated just
        /// before execution.
        /// <see cref="CreateJITInitCommand{TFromType,TToType}"/>
        /// </summary>
        /// <param name="index">The index for when the command should be executed.</param>
        /// <param name="jitInitFunction"><inheritdoc cref="JITInitCommandFunction{TFromType,TToType}"/></param>
        /// <typeparam name="TFromType">The type of the previously completed command.</typeparam>
        /// <typeparam name="TToType">The type of the new command to initialize.</typeparam>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was inserted into. Useful for method chaining.</returns>
        public AbstractCollectionCommand InsertChild<TFromType, TToType>(int index, JITInitCommandFunction<TFromType, TToType> jitInitFunction)
            where TFromType : AbstractCommand
            where TToType : AbstractCommand
        {
            IJITInitCommand jitInitCommand = CreateJITInitCommand<TFromType, TToType>(jitInitFunction);

            return InsertChild(index, jitInitCommand as AbstractCommand);
        }

        /// <summary>
        /// Inserts a child command to be executed in the collection.
        /// </summary>
        /// <param name="index">The index for when the command should be executed.</param>
        /// <param name="childCommand">The <see cref="AbstractCommand"/> to insert into the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was inserted into. Useful for method chaining.</returns>
        /// <exception cref="Exception">Occurs when the <see cref="State"/> is not CommandState.Initialized</exception>
        public AbstractCollectionCommand InsertChild(int index, AbstractCommand childCommand)
        {
            if (State != CommandState.Initialized)
            {
                throw new Exception($"Tried to insert child command {childCommand} to {this} but State was {State} instead of Initialized!");
            }

            m_ChildCommands.Insert(index, childCommand);

            return this;
        }

        /// <summary>
        /// Inserts an <see cref="IEnumerable{AbstractCommand}"/> to be executed in the collection.
        /// </summary>
        /// <param name="index">The index for when the beginning of the childCommands should be executed.</param>
        /// <param name="childCommands">The <see cref="IEnumerable{AbstractCommand}"/> to insert into the collection.</param>
        /// <returns>The <see cref="AbstractCollectionCommand"/> the child was inserted into. Useful for method chaining.</returns>
        public AbstractCollectionCommand InsertChildren(int index, IEnumerable<AbstractCommand> childCommands)
        {
            foreach (AbstractCommand childCommand in childCommands)
            {
                InsertChild(index, childCommand);
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

            AbstractCommand nextChildCommand = m_ChildCommands[m_ChildCommandIndex];

            //Handles actually creating the command needed if this is a command that should be created just in time.
            if (nextChildCommand is IJITInitCommand jitInitCommand)
            {
                nextChildCommand = jitInitCommand.JITInit(m_PreviousChildCommand);
                (jitInitCommand as AbstractCommand).Dispose();
            }
            m_PreviousChildCommand = null;

            nextChildCommand.OnComplete += HandleChildCommandOnComplete;
            nextChildCommand.Execute();
        }

        private void HandleChildCommandOnComplete(AbstractCommand command)
        {
            command.OnComplete -= HandleChildCommandOnComplete;
            m_ChildCommandIndex++;

            m_PreviousChildCommand = command;

            ExecuteNextChildCommandInSequence();
        }
    }
}
