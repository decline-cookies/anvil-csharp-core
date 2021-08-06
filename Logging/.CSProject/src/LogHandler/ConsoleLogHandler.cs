using System;
using System.IO;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Forwards logs to System.Console.
    /// </summary>
    [DefaultLogHandler(PRIORITY)]
    public class ConsoleLogHandler : ILogHandler
    {
        public const uint PRIORITY = 0;

        public void HandleLog(
            LogLevel level, 
            string message,
            string callerDerivedTypeName,
            string callerPath,
            string callerName,
            int callerLine)
        {
            message = $"({Path.GetFileNameWithoutExtension(callerPath)}|{callerName}:{callerLine}) {message}";

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
                    throw new NotImplementedException($"Unhandled log level: {level}");
            }
        }
    }
}
