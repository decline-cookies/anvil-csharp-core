using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Contains functions for logging messages through various systems, to aid in project development.
    /// </summary>
    public static class Log
    {
        public readonly struct Logger
        {
            private readonly string m_DerivedTypeName;
            
            public Logger(Type type)
            {
                m_DerivedTypeName = type.Name;
            }
            public Logger(in object instance)
            {
                m_DerivedTypeName = instance.GetType().Name;
            }

            /// <summary>
            /// Logs a message.
            /// </summary>
            /// <param name="message">The message object to log. The object is converting to a log by ToString().</param>
            public void Debug(
                object message,
                [CallerFilePath] string callerPath = "",
                [CallerMemberName] string callerName = "",
                [CallerLineNumber] int callerLine = 0
                ) => DispatchLog(
                    LogLevel.Debug,
                    (string)message,
                    m_DerivedTypeName,
                    callerPath,
                    callerName,
                    callerLine);

            /// <summary>
            /// Logs a warning message.
            /// </summary>
            /// <param name="message">The message object to log. The object is converted to a log by ToString().</param>
            public void Warning(
                object message,
                [CallerFilePath] string callerPath = "",
                [CallerMemberName] string callerName = "",
                [CallerLineNumber] int callerLine = 0
                ) => DispatchLog(
                    LogLevel.Warning,
                    (string)message,
                    m_DerivedTypeName,
                    callerPath,
                    callerName,
                    callerLine
                    );

            /// <summary>
            /// Logs an error message.
            /// </summary>
            /// <param name="message">The message object to log. The object is converted to a log by ToString().</param>
            public void Error(
                object message,
                [CallerFilePath] string callerPath = "",
                [CallerMemberName] string callerName = "",
                [CallerLineNumber] int callerLine = 0
                ) => DispatchLog(
                    LogLevel.Error,
                    (string)message,
                    m_DerivedTypeName,
                    callerPath,
                    callerName,
                    callerLine
                    );

            /// <summary>
            /// Logs a message to the level provided.
            /// </summary>
            /// <param name="level">The level to log at.</param>
            /// <param name="message">The message object to log. The object is converted to a log by ToString().</param>
            public void AtLevel(
                LogLevel level,
                object message,
                [CallerFilePath] string callerPath = "",
                [CallerMemberName] string callerName = "",
                [CallerLineNumber] int callerLine = 0
                ) => DispatchLog(
                    level,
                    (string)message,
                    m_DerivedTypeName,
                    callerPath,
                    callerName,
                    callerLine);
        }

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
                GetStaticLogger(typeof(Log)).Debug($"Default logger search ignoring assemblies: {IGNORE_ASSEMBLIES.Aggregate((a, b) => $"{a}, {b}")}");
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

        public static Logger GetStaticLogger(Type type)
        {
            return new Logger(type);
        }

        public static Logger GetLogger(in object instance)
        {
            return new Logger(in instance);
        }

        private static void DispatchLog(
            LogLevel level,
            string message,
            string callerDerivedTypeName,
            string callerPath,
            string callerName,
            int callerLine)
        {
            Debug.Assert(!string.IsNullOrEmpty(callerDerivedTypeName));
            foreach (ILogHandler handler in s_AdditionalHandlerList)
            {
                handler.HandleLog(level,
                message,
                callerDerivedTypeName,
                callerPath,
                callerName,
                callerLine);
            }
        }
    }
}
