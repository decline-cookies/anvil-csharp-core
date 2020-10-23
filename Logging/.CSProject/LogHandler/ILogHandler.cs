namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// A type capable of handling logs received by <see cref="Log"/>.
    /// </summary>
    public interface ILogHandler
    {
        void HandleLog(LogLevel level, string message);
    }
}
