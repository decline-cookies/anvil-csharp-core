namespace Anvil.CSharp.Command
{
    /// <summary>
    /// An implementation of <see cref="ICancelableCommand"/> based off of <see cref="AbstractCancelableCommand{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ICancelableCommand"/> to use.</typeparam>
    public abstract class AbstractCancelableCommand<T> : AbstractCommand<T>, ICancelableCommand where T : AbstractCancelableCommand<T>
    {
        /// <summary>
        /// <inheritdoc cref="ICancelableCommand.WasCancelled"/>
        /// </summary>
        public bool WasCancelled { get; private set; }


        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }

        /// <summary>
        /// <inheritdoc cref="AbstractCommand.Execute"/>
        /// </summary>
        public override void Execute()
        {
            // If the command has been cancelled skip execution and go straight to complete
            if(State == CommandState.Initialized && WasCancelled)
            {
                State = CommandState.Executing;
                CompleteCommand();
                return;
            }

            base.Execute();
        }

        /// <summary>
        /// <inheritdoc cref="ICancelableCommand.Cancel"/>
        /// </summary>
        public void Cancel()
        {
            if (WasCancelled || State == CommandState.Completed)
            {
                return;
            }

            WasCancelled = true;

            if (State == CommandState.Executing)
            {
                CancelCommand();
            }
        }

        /// <summary>
        /// Gracefully cancel execution.
        /// Only called if command was executing when <see cref="Cancel"/> was called.
        /// Remeber to call <see cref="AbstractCommand.CompleteCommand"/> when finished cancelling!
        /// </summary>
        protected abstract void CancelCommand();
    }
}
