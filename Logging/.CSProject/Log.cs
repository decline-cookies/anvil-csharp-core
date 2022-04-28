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
        /// <summary>
        /// A context specific instance that provides a mechanism to emit logs through <see cref="Log"/>.
        /// Automatically provides contextual information to <see cref="Log"/> about caller context including:
        ///  - Optional, per instance, message prefix
        ///  - Caller type name
        ///  - Caller file path
        ///  - Caller name
        ///  - Caller line number
        /// </summary>
        public readonly struct Logger
        {
            private readonly string m_DerivedTypeName;
            private readonly string m_MessagePrefix;

            /// <summary>
            /// Creates an instance of <see cref="Logger"/> from a <see cref="Type"/>.
            /// </summary>
            /// <param name="type">The <see cref="Type"/> to create the <see cref="Logger"/> instance for.</param>
            /// <param name="messagePrefix">
            /// An optional <see cref="string"/> to prefix to all messages through this logger.
            /// Useful when there are multiple types that share the same name which need to be differentiated.
            /// </param>
            public Logger(Type type, string messagePrefix = null) : this(type.Name, messagePrefix) { }
            /// <summary>
            /// Creates an instance of <see cref="Logger"/> from an instance.
            /// </summary>
            /// <param name="instance">The instance to create the <see cref="Logger"/> instance for.</param>
            /// <param name="messagePrefix">
            /// An optional <see cref="string"/> to prefix to all messages through this logger.
            /// Useful when there are multiple instances or types that share the same name which need to be differentiated.
            /// </param>
            public Logger(in object instance, string messagePrefix = null) : this(instance.GetType().Name, messagePrefix) { }

            private Logger(string derivedTypeName, string messagePrefix)
            {
                m_DerivedTypeName = derivedTypeName;
                m_MessagePrefix = messagePrefix;
            }

            /// <summary>
            /// Logs a message.
            /// </summary>
            /// <param name="message">
            /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
            /// </param>
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
            /// <param name="message">
            /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
            /// </param>
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
            /// <param name="message">
            /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
            /// </param>
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
            /// <param name="message">
            /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
            /// </param>
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

        /// <summary>
        /// Returns true while a log is being evaluated by handlers.
        /// Returns false at all other times.
        /// </summary>
        public static bool SupressLogging { get; set; } = false;

        /// <summary>
        /// While set to true handling of any incoming log messages is skipped.
        /// Defaults to false.
        /// </summary>
        /// <remarks>
        /// This is not a mechanism to omit logs from release builds (although it would work).
        /// This is generally used to suppress logs that the developer doesn't want to emit but can't stop the output of.
        /// Ex: false flag warnings during a non-standard operation in a library.
        /// </remarks>
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
        /// Add a custom <see cref="ILogHandler"/>, which will receive all logs that pass through <see cref="Log"/>.
        /// </summary>
        /// <param name="handler">The <see cref="ILogHandler"/> instance to add.</param>
        /// <returns>
        /// Returns true if the <see cref="ILogHandler"/> is successfully added, or false if the handler is null or
        /// has already been added.
        /// </returns>
        public static bool AddHandler(ILogHandler handler) => (handler != null && s_AdditionalHandlerList.Add(handler));

        /// <summary>
        /// Remove a custom <see cref="ILogHandler"/>, which was previously added.
        /// </summary>
        /// <param name="handler">The <see cref="ILogHandler"/> to remove.</param>
        /// <returns>Returns true if the <see cref="ILogHandler"/> was successfully removed.</returns>
        public static bool RemoveHandler(ILogHandler handler) => s_AdditionalHandlerList.Remove(handler);

        /// <summary>
        /// Removes all <see cref="ILogHandler"/>s including any default <see cref="ILogHandler"/>s.
        /// </summary>
        public static void RemoveAllHandlers() => s_AdditionalHandlerList.Clear();

        /// <summary>
        /// Creates a <see cref="Logger"/> instance for a given static <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to create the <see cref="Logger"/> instance for.</param>
        /// <param name="messagePrefix">
        /// An optional <see cref="string"/> to prefix to all messages through this logger.
        /// Useful when there are multiple types that share the same name which need to be differentiated.
        /// </param>
        /// <returns>The configured <see cref="Logger"/> instance</returns>
        public static Logger GetStaticLogger(Type type, string messagePrefix = null) => new Logger(type, messagePrefix);

        /// <summary>
        /// Creates a <see cref="Logger"/> instance for a given instance.
        /// </summary>
        /// <param name="instance">The instance to create the <see cref="Logger"/> instance for.</param>
        /// <param name="messagePrefix">
        /// An optional <see cref="string"/> to prefix to all messages through this logger.
        /// Useful when there are multiple instances or types that share the same name which need to be differentiated.
        /// </param>
        /// <returns>The configured <see cref="Logger"/> instance</returns>
        public static Logger GetLogger(in object instance, string messagePrefix = null) => new Logger(in instance, messagePrefix);

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
