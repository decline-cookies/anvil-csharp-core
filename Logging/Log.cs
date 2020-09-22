using System;
using System.Collections.Generic;

namespace Anvil.CSharp.Logging
{
    public static class Log
    {
        private static ILogHandler s_DefaultHandler = new ConsoleLogHandler();

        private static readonly HashSet<ILogHandler> s_AdditionalHandlerList = new HashSet<ILogHandler>();

        public static void OverrideDefaultHandler(ILogHandler handler)
        {
            s_DefaultHandler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        public static bool AddHandler(ILogHandler handler) => s_AdditionalHandlerList.Add(handler);
        public static bool RemoveHandler(ILogHandler handler) => s_AdditionalHandlerList.Remove(handler);

        public static void Debug(string message) => DispatchLog(LogLevel.Debug, message);
        public static void DebugFormat(string format, params object[] args) => Debug(string.Format(format, args));

        public static void Warning(string message) => DispatchLog(LogLevel.Warning, message);
        public static void WarningFormat(string format, params object[] args) => Warning(string.Format(format, args));

        public static void Error(string message) => DispatchLog(LogLevel.Error, message);
        public static void ErrorFormat(string format, params object[] args) => Error(string.Format(format, args));

        private static void DispatchLog(LogLevel level, string message)
        {
            s_DefaultHandler.HandleLog(level, message);

            foreach (ILogHandler handler in s_AdditionalHandlerList)
            {
                handler.HandleLog(level, message);
            }
        }
    }
}
