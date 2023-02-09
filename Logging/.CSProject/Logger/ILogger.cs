using System.Runtime.CompilerServices;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// An instance that provides a mechanism to emit logs through <see cref="Log"/>.
    /// Not intended to be used directly. This is used to keep the APIs of <see cref="Logger"/> and
    /// <see cref="OneTimeLogger"/> in sync.
    /// </summary>
    internal interface ILogger
    {
        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">
        /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
        /// </param>
        void Debug(
            object message,
            [CallerFilePath] string callerPath = "",
            [CallerMemberName] string callerName = "",
            [CallerLineNumber] int callerLine = 0);

        /// <summary>
        /// Logs a warning message.
        /// </summary>
        /// <param name="message">
        /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
        /// </param>
        void Warning(
            object message,
            [CallerFilePath] string callerPath = "",
            [CallerMemberName] string callerName = "",
            [CallerLineNumber] int callerLine = 0);

        /// <summary>
        /// Logs an error message.
        /// </summary>
        /// <param name="message">
        /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
        /// </param>
        void Error(
            object message,
            [CallerFilePath] string callerPath = "",
            [CallerMemberName] string callerName = "",
            [CallerLineNumber] int callerLine = 0);

        /// <summary>
        /// Logs a message to the level provided.
        /// </summary>
        /// <param name="level">The level to log at.</param>
        /// <param name="message">
        /// The message object to log. The object is converted to a <see cref="string"/> by <see cref="object.ToString"/>.
        /// </param>
        void AtLevel(
            LogLevel level,
            object message,
            [CallerFilePath] string callerPath = "",
            [CallerMemberName] string callerName = "",
            [CallerLineNumber] int callerLine = 0);
    }
}
