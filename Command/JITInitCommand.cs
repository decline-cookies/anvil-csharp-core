using System;

namespace Anvil.CSharp.Command
{
    internal class JITInitCommand<TFromType, TToType> : AbstractCommand, IJITInitCommand
        where TFromType : AbstractCommand
        where TToType : AbstractCommand
    {
        private JITInitCommandFunction<TFromType, TToType> m_JITInitFunction;
        public JITInitCommand(JITInitCommandFunction<TFromType, TToType> jitInitFunction)
        {
            m_JITInitFunction = jitInitFunction;
        }

        protected override void DisposeSelf()
        {
            m_JITInitFunction = null;
            base.DisposeSelf();
        }

        public AbstractCommand JITInit(AbstractCommand fromCommand)
        {
            return m_JITInitFunction?.Invoke(fromCommand as TFromType);
        }

        protected override void ExecuteCommand()
        {
            throw new NotImplementedException();
        }
    }
}

