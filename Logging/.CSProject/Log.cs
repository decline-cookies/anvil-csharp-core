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
            private readonly string m_MessagePrefix;

            public Logger(Type type, string messagePrefix = null) : this(type.Name, messagePrefix) { }
            public Logger(in object instance, string messagePrefix = null) : this(instance.GetType().Name, messagePrefix) { }

            private Logger(string derivedTypeName, string messagePrefix)
            {
                m_DerivedTypeName = derivedTypeName;
                m_MessagePrefix = messagePrefix;
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
                    string.Concat(m_MessagePrefix, message),
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
                    string.Concat(m_MessagePrefix, message),
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
                    string.Concat(m_MessagePrefix, message),
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
                    string.Concat(m_MessagePrefix, message),
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

        public static bool SupressLogging { get; set; } = false;
        public static bool IsHandlingLog { get; private set; } = false;

        static Log()
        {
            IEnumerable<Type> candidateTypes = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !ShouldIgnoreAssembly(a))
                .SelectMany(a => a.GetTypes());

            InitLogHandlers(candidateTypes);
            InitLogListeners(candidateTypes);

            if (IGNORE_ASSEMBLIES.Any())
            {
                GetStaticLogger(typeof(Log)).Debug($"Default log handler and listener search ignored assemblies: {IGNORE_ASSEMBLIES.Aggregate((a, b) => $"{a}, {b}")}");
            }
        }

        private static void InitLogHandlers(IEnumerable<Type> candidateTypes)
        {
            Type defaultLogHandlerType = candidateTypes
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
        }

        private static void InitLogListeners(IEnumerable<Type> candidateTypes)
        {
            candidateTypes = candidateTypes
                .Where(t => t.IsDefined(typeof(DefaultLogListenerAttribute)))
                .Where(t => t.GetInterfaces().Contains(typeof(ILogListener)));

            if (!candidateTypes.Any())
            {
                return;
            }

            var logger = GetStaticLogger(typeof(Log));
            foreach (Type listener in candidateTypes)
            {   
                Activator.CreateInstance(listener);
                logger.Debug($"Default Log listener {listener.Name} initialized. Source: {listener.AssemblyQualifiedName}");
            }
        }

        private static bool ShouldIgnoreAssembly(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            return IGNORE_ASSEMBLIES.Any(ignore => name == ignore || name.StartsWith($"{ignore}."));
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

        public static Logger GetStaticLogger(Type type, string messagePrefix = null)
        {
            return new Logger(type, messagePrefix);
        }

        public static Logger GetLogger(in object instance, string messagePrefix = null)
        {
            return new Logger(in instance, messagePrefix);
        }

        private static void DispatchLog(
            LogLevel level,
            string message,
            string callerDerivedTypeName,
            string callerPath,
            string callerName,
            int callerLine)
        {
            if (SupressLogging)
            {
                return;
            }

            Debug.Assert(!string.IsNullOrEmpty(callerDerivedTypeName));
            Debug.Assert(!IsHandlingLog);

            IsHandlingLog = true;
            foreach (ILogHandler handler in s_AdditionalHandlerList)
            {
                handler.HandleLog(level,
                message,
                callerDerivedTypeName,
                callerPath,
                callerName,
                callerLine);
            }
            IsHandlingLog = false;
        }
    }
}
