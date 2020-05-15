namespace Anvil.CSharp.Command
{
    /// <summary>
    /// Delegate for a function that returns an instance of an <see cref="ICommand"/>.
    /// This is used with <see cref="JITCommand"/> to construct a command Just In Time allowing for passing in
    /// data that may only be available at the time of construction.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="ICommand"/> to construct.</typeparam>
    public delegate T ConstructCommandJIT<out T>() where T : ICommand;
}
