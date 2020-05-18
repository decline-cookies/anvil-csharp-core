using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    public class BufferCommand : AbstractCommand<BufferCommand>
    {
        private readonly List<ICommand> m_ChildCommands = new List<ICommand>();
        private readonly List<float> m_ChildPriorities = new List<float>();

        private ICommand m_ActiveCommand;


        public new event Action<BufferCommand> OnComplete
        {
            add => throw new Exception($"Buffer Commands are designed to never complete!");
            remove => throw new Exception($"Buffer Commands are designed to never complete!");
        }

        public BufferCommand()
        {

        }

        public BufferCommand(params (ICommand Command, float Priority)[] children):this((IEnumerable<(ICommand Command, float Priority)>)children)
        {
        }

        public BufferCommand(params ICommand[] childCommands):this((IEnumerable<ICommand>)childCommands)
        {
        }

        public BufferCommand(IEnumerable<(ICommand Command, float Priority)> children)
        {
            AddChildren(children);
        }

        public BufferCommand(IEnumerable<ICommand> childCommands)
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

        public BufferCommand AddChild(ICommand childCommand, float priority = 0.0f, bool shouldRemoveFollowingCommands = false)
        {
            int insertIndex = m_ChildCommands.Count;
            for (int i = 0; i < m_ChildCommands.Count; ++i)
            {
                float currentPriority = m_ChildPriorities[i];
                if (priority > currentPriority)
                {
                    insertIndex = i;
                    break;
                }
            }

            m_ChildCommands.Insert(insertIndex, childCommand);
            m_ChildPriorities.Insert(insertIndex, priority);

            if (shouldRemoveFollowingCommands)
            {
                for (int i = m_ChildCommands.Count - 1; i > insertIndex; --i)
                {
                    ICommand command = m_ChildCommands[i];
                    command.Dispose();
                    m_ChildCommands.RemoveAt(i);
                }
            }

            if (State == CommandState.Executing && m_ActiveCommand == null)
            {
                ExecuteNextChildCommandInBuffer();
            }

            return this;
        }

        public BufferCommand AddChildren(IEnumerable<(ICommand Command, float Priority)> children)
        {
            foreach ((ICommand command, float priority) in children)
            {
                AddChild(command, priority);
            }

            return this;
        }

        public BufferCommand AddChildren(IEnumerable<ICommand> childCommands)
        {
            foreach (ICommand childCommand in childCommands)
            {
                AddChild(childCommand);
            }

            return this;
        }

        protected override void ExecuteCommand()
        {
            ExecuteNextChildCommandInBuffer();
        }

        private void ExecuteNextChildCommandInBuffer()
        {
            if (m_ChildCommands.Count == 0)
            {
                return;
            }

            ICommand childCommand = m_ChildCommands[0];

            m_ChildCommands.RemoveAt(0);
            m_ChildPriorities.RemoveAt(0);

            childCommand.OnComplete += ChildCommand_OnComplete;
            childCommand.Execute();
        }

        private void ChildCommand_OnComplete(ICommand childCommand)
        {
            childCommand.OnComplete -= ChildCommand_OnComplete;

            ExecuteNextChildCommandInBuffer();
        }
    }
}
