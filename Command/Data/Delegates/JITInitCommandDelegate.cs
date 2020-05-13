namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Delegate definition for constructing a new command of type <see cref="TToType"/>
    /// based on an instance of a command with type <see cref="TFromType"/>.
    /// This allows for initialization of the new command only once the previous command has been completed
    /// and its data or result is available to influence the initialization.
    /// </summary>
    /// <param name="fromCommand">The instance of the previously completed command.</param>
    /// <typeparam name="TFromType">The type of the previously completed command.</typeparam>
    /// <typeparam name="TToType">The type of the new command to initialize.</typeparam>
    public delegate TToType JITInitCommandFunction<in TFromType, out TToType>(TFromType fromCommand)
        where TFromType : AbstractCommand
        where TToType : AbstractCommand;
}
