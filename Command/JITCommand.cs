using System;

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
    public class JITCommand<T> : AbstractCommand<JITCommand<T>>
        where T : ICommand
    {
        /// <summary>
        /// Gets the JIT command that was created <see cref="{T}"/>
        /// </summary>
        public T Command { get; private set; }

        private ConstructCommandJIT<T> m_ConstructCommandJITFunction;

        /// <summary>
        /// Constructs a new <see cref="JITCommand{T}"/>
        /// </summary>
        /// <param name="constructCommandJITFunction">The function used to construct an instance of the
        /// generic type <see cref="ICommand"/>. See <see cref="ConstructCommandJIT{T}"/>.
        /// </param>
        public JITCommand(ConstructCommandJIT<T> constructCommandJITFunction)
        {
            if(constructCommandJITFunction == null)
            {
                throw new ArgumentException($"{nameof(constructCommandJITFunction)} cannot be null!");
            }

            m_ConstructCommandJITFunction = constructCommandJITFunction;
        }

        protected override void DisposeSelf()
        {
            m_ConstructCommandJITFunction = null;
            Command?.Dispose();

            base.DisposeSelf();
        }

        protected override void ExecuteCommand()
        {
            Command = m_ConstructCommandJITFunction();
            Command.OnComplete += HandleOnComplete;

            Command.Execute();
        }

        private void HandleOnComplete(ICommand command)
        {
            Command.OnComplete -= HandleOnComplete;
            CompleteCommand();
        }
    }
}

