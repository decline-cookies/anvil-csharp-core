using System;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Forwards logs to <see cref="Console"/>.
    /// </summary>
    [DefaultLogHandler(PRIORITY)]
    public class ConsoleLogHandler : AbstractLogHandler
    {
        public const uint PRIORITY = 0;

        protected override void HandleFormattedLog(LogLevel level, string formattedLog)
        {
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Warning:
                    Console.WriteLine(formattedLog);
                    break;

                case LogLevel.Error:
                    Console.Error.WriteLine(formattedLog);
                    break;

                default:
                    throw new NotImplementedException($"Unhandled log level: {level}");
            }
        }
    }
}