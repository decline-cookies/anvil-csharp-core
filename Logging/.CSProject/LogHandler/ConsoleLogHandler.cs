using System;

namespace Anvil.CSharp.Logging
{
    [DefaultLogHandler(priority: 0)]
    public class ConsoleLogHandler : ILogHandler
    {
        public void HandleLog(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Warning:
                    Console.WriteLine(message);
                    break;
                case LogLevel.Error:
                    Console.Error.WriteLine(message);
                    break;
                default:
                    throw new ArgumentException($"Unhandled log level: {level}");
            }
        }
    }
}
