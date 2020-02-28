using System.Collections.Generic;

namespace Anvil.CSharp.Command
{
    public class SequentialCommand : AbstractCommand
    {
        private List<AbstractCommand> m_SubCommands = new List<AbstractCommand>();
        private int m_SubCommandIndex;
        public SequentialCommand(params AbstractCommand[] commands)
        {
            foreach (AbstractCommand command in commands)
            {
                m_SubCommands.Add(command);
            }
        }

        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        public override void Execute()
        {
            
        }
    }
}

