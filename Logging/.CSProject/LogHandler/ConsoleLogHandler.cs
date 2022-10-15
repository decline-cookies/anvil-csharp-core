using System;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Forwards logs to <see cref="Console"/>.
    /// </summary>
    [DefaultLogHandler(PRIORITY)]
    public class ConsoleLogHandler : ILogHandler
    {
        public const uint PRIORITY = 0;

        public void HandleLog(LogLevel level, string message, in CallerInfo callerInfo)
        {
            if (callerInfo.LineNumber > 0)
            {
               message = $"({callerInfo.TypeName}.{callerInfo.MethodName}|{callerInfo.FileName}:{callerInfo.LineNumber}) {message}";
            }
            else
            {
               message = $"({callerInfo.TypeName}.{callerInfo.MethodName}|{callerInfo.FileName}) {message}";
            }

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
