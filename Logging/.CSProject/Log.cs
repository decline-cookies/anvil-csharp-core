using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Contains functions for logging messages through various systems, to aid in project development.
    /// </summary>
    public static class Log
    {
        private static readonly string[] IGNORE_ASSEMBLIES =
        {
            "System", "mscorlib", "Unity", "UnityEngine", "UnityEditor", "nunit"
        };

        private static readonly HashSet<ILogHandler> s_AdditionalHandlerList = new HashSet<ILogHandler>();

        static Log()
        {
            Type defaultLogHandlerType = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !ShouldIgnore(a))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsDefined(typeof(DefaultLogHandlerAttribute)))
                .OrderByDescending(t => t.GetCustomAttribute<DefaultLogHandlerAttribute>().Priority)
                .FirstOrDefault();

            if (defaultLogHandlerType == null)
            {
                throw new Exception($"No types found with {nameof(DefaultLogHandlerAttribute)}, failed to initialize");
            }

            if (!defaultLogHandlerType.GetInterfaces().Contains(typeof(ILogHandler)))
            {
                throw new Exception($"Default log handler {defaultLogHandlerType} does not implement {nameof(ILogHandler)}");
            }

            AddHandler((ILogHandler)Activator.CreateInstance(defaultLogHandlerType));

            bool ShouldIgnore(Assembly assembly)
            {
                string name = assembly.GetName().Name;
                return IGNORE_ASSEMBLIES.Any(ignore => name == ignore || name.StartsWith($"{ignore}."));
            }

            if (IGNORE_ASSEMBLIES.Any())
            {
                Debug($"Default logger search ignoring assemblies: {IGNORE_ASSEMBLIES.Aggregate((a, b) => $"{a}, {b}")}");
            }
        }

        /// <summary>
        /// Add a custom log handler, which will receive all logs that pass through <see cref="Log"/>.
        /// </summary>
        /// <param name="handler">The log handler to add.</param>
        /// <returns>Returns true if the handler is successfully added, or false if the handler is null or
        /// has already been added.</returns>
        public static bool AddHandler(ILogHandler handler) => (handler != null && s_AdditionalHandlerList.Add(handler));

        /// <summary>
        /// Remove a custom log handler, which was previously added.
        /// </summary>
        /// <param name="handler">The log handler to remove.</param>
        /// <returns>Returns whether the handler was successfully removed.</returns>
        public static bool RemoveHandler(ILogHandler handler) => s_AdditionalHandlerList.Remove(handler);

        /// <summary>
        /// Removes all log handlers including any default handlers.
        /// </summary>
        public static void RemoveAllHandlers() => s_AdditionalHandlerList.Clear();

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="message">The message object to log. The object is converting to a log by ToString().</param>
        public static void Debug(object message) => DispatchLog(LogLevel.Debug, $"{message}");

        /// <summary>
        /// Logs a formatted message.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The format arguments.</param>
        public static void DebugFormat(string format, params object[] args) => Debug(string.Format(format, args));

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">The message object to log. The object is converted to a log by ToString().</param>
        public static void Warning(object message) => DispatchLog(LogLevel.Warning, $"{message}");

        /// <summary>
        /// Logs a formatted warning message.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The format arguments.</param>
        public static void WarningFormat(string format, params object[] args) => Warning(string.Format(format, args));

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">The message object to log. The object is converted to a log by ToString().</param>
        public static void Error(object message) => DispatchLog(LogLevel.Error, $"{message}");

        /// <summary>
        /// Logs a formatted error message.
        /// </summary>
        /// <param name="format">A format string.</param>
        /// <param name="args">The format arguments.</param>
        public static void ErrorFormat(string format, params object[] args) => Error(string.Format(format, args));

        /// <summary>
        /// Logs a formatted message to the level provided.
        /// </summary>
        /// <param name="level">The level to log at.</param>
        /// <param name="format">A format string.</param>
        /// <param name="args">The format arguments.</param>
        public static void AtLevel(LogLevel level, string format, params object[] args) => AtLevel(level, string.Format(format, args));

        /// <summary>
        /// Logs a message to the level provided.
        /// </summary>
        /// <param name="level">The level to log at.</param>
        /// <param name="message">The message object to log. The object is converted to a log by ToString().</param>
        public static void AtLevel(LogLevel level, object message) => DispatchLog(level, $"{message}");


        private static void DispatchLog(LogLevel level, string message)
        {
            foreach (ILogHandler handler in s_AdditionalHandlerList)
            {
                handler.HandleLog(level, message);
            }
        }
    }
}
