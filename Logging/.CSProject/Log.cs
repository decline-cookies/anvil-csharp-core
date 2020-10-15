using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Anvil.CSharp.Logging
{
    public static class Log
    {
        private static readonly string[] IGNORE_ASSEMBLIES =
        {
            "System", "mscorlib", "Unity", "UnityEngine", "UnityEditor", "nunit"
        };

        private static readonly ILogHandler DEFAULT_HANDLER;

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

            DEFAULT_HANDLER = (ILogHandler)Activator.CreateInstance(defaultLogHandlerType);

            bool ShouldIgnore(Assembly assembly)
            {
                string name = assembly.GetName().Name;
                return IGNORE_ASSEMBLIES.Any(ignore => name == ignore || name.StartsWith(ignore));
            }
        }

        public static bool AddHandler(ILogHandler handler) => (handler != null && s_AdditionalHandlerList.Add(handler));
        public static bool RemoveHandler(ILogHandler handler) => s_AdditionalHandlerList.Remove(handler);

        public static void Debug(string message) => DispatchLog(LogLevel.Debug, message);
        public static void DebugFormat(string format, params object[] args) => Debug(string.Format(format, args));

        public static void Warning(string message) => DispatchLog(LogLevel.Warning, message);
        public static void WarningFormat(string format, params object[] args) => Warning(string.Format(format, args));

        public static void Error(string message) => DispatchLog(LogLevel.Error, message);
        public static void ErrorFormat(string format, params object[] args) => Error(string.Format(format, args));

        private static void DispatchLog(LogLevel level, string message)
        {
            DEFAULT_HANDLER.HandleLog(level, message);

            foreach (ILogHandler handler in s_AdditionalHandlerList)
            {
                handler.HandleLog(level, message);
            }
        }
    }
}
