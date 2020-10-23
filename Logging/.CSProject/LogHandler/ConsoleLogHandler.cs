﻿using System;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Forwards logs to System.Console.
    /// </summary>
    [DefaultLogHandler(PRIORITY)]
    public class ConsoleLogHandler : ILogHandler
    {
        public const int PRIORITY = 0;

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
