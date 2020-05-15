namespace Anvil.CSharp.Command
{
    /// <summary>
    /// A specialized Command that will construct the passed in generic type of <see cref="ICommand"/>
    /// right when this Command is executed. This allows for the concept of JIT (Just In Time) since
    /// at the time of the JITCommand construction, data necessary for the construction of the generic
    /// <see cref="ICommand"/> may not be available.
    /// </summary>
    /// <remarks>
    /// Typically this is used in cases with <see cref="SequentialCommand"/> or
    /// <see cref="ParallelCommand"/> where Commands are added to the collection to then be run in
    /// sequence or parallel. Some of those commands will output data that subsequent commands require.
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="ICommand"/> to construct.</typeparam>
    public class JITCommand<T> : AbstractCommand<T>
        where T: class, ICommand
    {
        /// <summary>
        /// <inheritdoc cref="ICommand.State"/>
        /// This method bypasses the State for the <see cref="JITCommand{T}"/> and instead returns
        /// the <see cref="CommandState"/> of the generic type <see cref="ICommand"/>
        /// </summary>
        public new CommandState State => m_Command != null ? m_Command.State : CommandState.None;

        private ConstructCommandJIT<T> m_ConstructCommandJITFunction;
        private T m_Command;

        /// <summary>
        /// Constructs a new <see cref="JITCommand{T}"/>
        /// </summary>
        /// <param name="constructCommandJITFunction">The function used to construct an instance of the
        /// generic type <see cref="ICommand"/>. See <see cref="ConstructCommandJIT{T}"/>.
        /// </param>
        public JITCommand(ConstructCommandJIT<T> constructCommandJITFunction)
        {
            m_ConstructCommandJITFunction = constructCommandJITFunction;
        }

        protected override void DisposeSelf()
        {
            m_ConstructCommandJITFunction = null;
            m_Command?.Dispose();
            m_Command = null;
            base.DisposeSelf();
        }

        protected override void ExecuteCommand()
        {
            m_Command = m_ConstructCommandJITFunction?.Invoke();
            m_Command.OnComplete += HandleOnComplete;
        }

        private void HandleOnComplete(ICommand command)
        {
            m_Command.OnComplete -= HandleOnComplete;
            CompleteCommand();
        }
    }
}

