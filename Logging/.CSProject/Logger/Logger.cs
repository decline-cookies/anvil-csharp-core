using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// A context specific instance that provides a mechanism to emit logs through <see cref="Log"/>.
    /// Automatically provides contextual information to <see cref="Log"/> about caller context including:
    ///  - Optional, per instance, message prefix
    ///  - Caller type name
    ///  - Caller method name
    ///  - Caller file path
    ///  - Caller line number
    /// </summary>
    public readonly struct Logger : ILogger
    {
        // NOTE: This is a duplicate of Anvil.CSharp.Reflection.TypeExtension.GetReadableName
        // because the logging namespace is isolated from the rest of Anvil, and thus cannot access that extension
        private static readonly Type s_NullableType = typeof(Nullable<>);
        private static string GetReadableName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            if (type.GetGenericTypeDefinition() == s_NullableType)
            {
                return $"{GetReadableName(type.GenericTypeArguments[0])}?";
            }

            // Remove the generic type count indicator (`n) from the type name
            // Avoid having to calculate the number of digits in the generic type count by assuming it's under 100
            int removeCount = 1 + (type.GenericTypeArguments.Length < 10 ? 1 : 2);
            string name = type.Name[..^removeCount];

            string genericTypeNames = string.Join(", ", type.GenericTypeArguments.Select(GetReadableName));

            return $"{name}<{genericTypeNames}>";
        }

        /// <summary>
        /// The name of the type this <see cref="Logger"/> represents.
        /// </summary>
        public readonly string OwnerTypeName;
        /// <summary>
        /// The custom prefix to prepend to all messages sent through this <see cref="Logger"/>.
        /// </summary>
        public readonly string MessagePrefix;

        /// <summary>
        /// Creates an instance of <see cref="Logger"/> from a <see cref="Type"/>.
        /// </summary>
        /// <param name="ownerType">The <see cref="Type"/> to create a <see cref="Logger"/> instance for.</param>
        /// <param name="messagePrefix">
        /// An optional <see cref="string"/> to prefix to all messages through this logger.
        /// Useful when there are multiple types that share the same name which need to be differentiated.
        /// </param>
        public Logger(Type ownerType, string messagePrefix = null) : this(GetReadableName(ownerType), messagePrefix) { }
        /// <summary>
        /// Creates an instance of <see cref="Logger"/> for an object instance.
        /// </summary>
        /// <param name="owner">The instance to create a <see cref="Logger"/> instance for.</param>
        /// <param name="messagePrefix">
        /// An optional <see cref="string"/> to prefix to all messages through this logger.
        /// Useful when there are multiple instances or types that share the same name which need to be differentiated.
        /// </param>
        public Logger(in object owner, string messagePrefix = null) : this(owner.GetType(), messagePrefix) { }

        private Logger(string ownerTypeName, string messagePrefix)
        {
            OwnerTypeName = ownerTypeName;
            MessagePrefix = messagePrefix;
        }

        /// <inheritdoc cref="ILogger.Debug"/>
        public void Debug(
            object message,
            [CallerMemberName] string callerMethodName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) => Log.DispatchLog(
            LogLevel.Debug,
            string.Concat(MessagePrefix, message),
            OwnerTypeName,
            callerMethodName,
            callerFilePath,
            callerLineNumber);

        /// <inheritdoc cref="ILogger.Warning"/>
        public void Warning(
            object message,
            [CallerMemberName] string callerMethodName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) => Log.DispatchLog(
            LogLevel.Warning,
            string.Concat(MessagePrefix, message),
            OwnerTypeName,
            callerMethodName,
            callerFilePath,
            callerLineNumber
        );

        /// <inheritdoc cref="ILogger.Error"/>
        public void Error(
            object message,
            [CallerMemberName] string callerMethodName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) => Log.DispatchLog(
            LogLevel.Error,
            string.Concat(MessagePrefix, message),
            OwnerTypeName,
            callerMethodName,
            callerFilePath,
            callerLineNumber
        );

        /// <inheritdoc cref="ILogger.AtLevel"/>
        public void AtLevel(
            LogLevel level,
            object message,
            [CallerMemberName] string callerMethodName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callerLineNumber = 0
        ) => Log.DispatchLog(
            level,
            string.Concat(MessagePrefix, message),
            OwnerTypeName,
            callerMethodName,
            callerFilePath,
            callerLineNumber);

        public OneTimeLogger OneTime() => new OneTimeLogger(this);
    }
}
