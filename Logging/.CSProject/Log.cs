using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Contains functions for logging messages through various systems, to aid in project development.
    /// </summary>
    public static class Log
    {
        private const string UNKNOWN_CONTEXT = "<unknown>";

        private static readonly HashSet<AbstractLogHandler> s_AdditionalHandlerList = new HashSet<AbstractLogHandler>();

        /// <summary>
        /// Returns true while a log is being evaluated by handlers.
        /// Returns false at all other times.
        /// </summary>
        public static bool SuppressLogging { get; set; } = false;

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

            if (!defaultLogHandlerType.IsSubclassOf(typeof(AbstractLogHandler)))
            {
                throw new Exception($"Default log handler {defaultLogHandlerType} is not a subclass of {nameof(AbstractLogHandler)}");
            }

            AddHandler((AbstractLogHandler)Activator.CreateInstance(defaultLogHandlerType));
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

            Logger logger = GetStaticLogger(typeof(Log));
            foreach (Type listener in candidateTypes)
            {
                Activator.CreateInstance(listener);
                logger.Debug($"Default Log listener {listener.Name} initialized. Source: {listener.AssemblyQualifiedName}");
            }
        }

        /// <summary>
        /// Determines if an assembly should be ignored based on whether it references this logging assembly or not.
        /// </summary>
        /// <param name="assembly">The assembly to evaluate.</param>
        /// <returns>true if the assembly should be ignored.</returns>
        /// <remarks>If the assembly provided is the logging assembly this method will return false.</remarks>
        private static bool ShouldIgnoreAssembly(Assembly assembly)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            // If the assembly is referencing this assembly (logging) then we shouldn't ignore it.
            if (currentAssembly == assembly)
            {
                return false;
            }

            // With strongly typed assemblies the best effort check is to compare Assembly.ToString().
            // Discussion: https://stackoverflow.com/a/3459277/640196
            string currentAssemblyName = currentAssembly.GetName().ToString();
            bool isAssemblyReferenceLogging = assembly.GetReferencedAssemblies()
                .Any(referencedAssemblyName => referencedAssemblyName.ToString() == currentAssemblyName);

            return !isAssemblyReferenceLogging;
        }

        /// <summary>
        /// Add a custom <see cref="AbstractLogHandler"/>, which will receive all logs that pass through <see cref="Log"/>.
        /// </summary>
        /// <param name="handler">The <see cref="AbstractLogHandler"/> instance to add.</param>
        /// <returns>
        /// Returns true if the <see cref="AbstractLogHandler"/> is successfully added, or false if the handler is null or
        /// has already been added.
        /// </returns>
        public static bool AddHandler(AbstractLogHandler handler) => (handler != null && s_AdditionalHandlerList.Add(handler));

        /// <summary>
        /// Remove a custom <see cref="AbstractLogHandler"/>, which was previously added.
        /// </summary>
        /// <param name="handler">The <see cref="AbstractLogHandler"/> to remove.</param>
        /// <returns>Returns true if the <see cref="AbstractLogHandler"/> was successfully removed.</returns>
        public static bool RemoveHandler(AbstractLogHandler handler) => s_AdditionalHandlerList.Remove(handler);

        /// <summary>
        /// Removes all <see cref="AbstractLogHandler"/>s including any default <see cref="AbstractLogHandler"/>s.
        /// </summary>
        public static void RemoveAllHandlers() => s_AdditionalHandlerList.Clear();

        /// <summary>
        /// Creates a <see cref="Logger"/> instance for a given static <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to create a <see cref="Logger"/> instance for.</param>
        /// <param name="messagePrefix">
        /// An optional <see cref="string"/> to prefix to all messages through this logger.
        /// Useful when there are multiple types that share the same name which need to be differentiated.
        /// </param>
        /// <returns>The configured <see cref="Logger"/> instance</returns>
        public static Logger GetStaticLogger(Type type, string messagePrefix = null) => new Logger(type, messagePrefix);

        /// <summary>
        /// Creates a <see cref="Logger"/> instance for a given instance.
        /// </summary>
        /// <param name="owner">The instance to create a <see cref="Logger"/> instance for.</param>
        /// <param name="messagePrefix">
        /// An optional <see cref="string"/> to prefix to all messages through this logger.
        /// Useful when there are multiple instances or types that share the same name which need to be differentiated.
        /// </param>
        /// <returns>The configured <see cref="Logger"/> instance</returns>
        public static Logger GetLogger(in object owner, string messagePrefix = null) => new Logger(in owner, messagePrefix);

        internal static void DispatchLog(
            LogLevel level,
            string message,
            string callerTypeName,
            string callerMethodName,
            string callerFilePath,
            int callerLineNumber)
        {
            if (SuppressLogging)
            {
                return;
            }

            Debug.Assert(!string.IsNullOrEmpty(callerTypeName));

            if (IsHandlingLog)
            {
                throw new Exception("A log is already being handled");
            }

            string callerFileName;
            if (!string.IsNullOrEmpty(callerFilePath))
            {
                int lastPathSeparatorIndex = callerFilePath.LastIndexOf('/');
                if (lastPathSeparatorIndex == -1)
                {
                    lastPathSeparatorIndex = callerFilePath.LastIndexOf('\\');
                }

                if (lastPathSeparatorIndex != -1)
                {
                    callerFileName = callerFilePath.Substring(lastPathSeparatorIndex + 1);
                }
                else
                {
                    callerFileName = callerFilePath;
                }
            }
            else
            {
                callerFilePath = UNKNOWN_CONTEXT;
                callerFileName = UNKNOWN_CONTEXT;
            }

            CallerInfo callerInfo = new CallerInfo(
                callerTypeName,
                callerMethodName ?? UNKNOWN_CONTEXT,
                callerFilePath,
                callerFileName,
                callerLineNumber);

            IsHandlingLog = true;
            foreach (AbstractLogHandler handler in s_AdditionalHandlerList)
            {
                handler.HandleLog(level, message, in callerInfo);
            }
            IsHandlingLog = false;
        }
    }
}