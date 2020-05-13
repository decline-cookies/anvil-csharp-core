namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Interface for interacting with a Command that will be instantiated right before execution.
    /// <see cref="SequentialCommand.CreateJITInitCommand"/>
    /// </summary>
    public interface IJITInitCommand
    {
        /// <summary>
        /// Creates the instance of the new command passing in the previously completed command.
        /// </summary>
        /// <param name="fromCommand">The previously completed command.</param>
        /// <returns>The newly instantiated command.</returns>
        AbstractCommand JITInit(AbstractCommand fromCommand);
    }
}
