namespace Anvil.CSharp.Logging
{
    public interface ILogHandler
    {
        void HandleLog(LogLevel level, string message);
    }
}
