using System;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// A type capable of handling logs received by <see cref="Log"/>.
    /// </summary>
    public abstract class AbstractLogHandler
    {
        /// <summary> Log format part - the log message itself </summary>
        public const string LOG_PART_MESSAGE = "{0}";
        /// <summary> Log format part - the caller's type name </summary>
        public const string LOG_PART_CALLER_TYPE = "{1}";
        /// <summary> Log format part - the caller's method name </summary>
        public const string LOG_PART_CALLER_METHOD = "{2}";
        /// <summary> Log format part - the caller's source file path </summary>
        public const string LOG_PART_CALLER_PATH = "{3}";
        /// <summary> Log format part - the caller's source file name </summary>
        public const string LOG_PART_CALLER_FILE = "{4}";
        /// <summary> Log format part - the caller's source file line number </summary>
        public const string LOG_PART_CALLER_LINE = "{5}";
        /// <summary> Log format part - the timestamp at which the log was emitted. See <see cref="TimestampFormat"/> to customize format. </summary>
        public const string LOG_PART_TIMESTAMP = "{6}";
        /// <summary> Log format part - the log level. </summary>
        public const string LOG_PART_LOG_LEVEL = "{7}";
        /// <summary>
        /// Log format part - the color applied to a portion of the log.
        /// This is just the color value, it's up to the <see cref="LogFormat"/> to include the appropriate syntax for
        /// the handler's target (if supported).
        /// </summary>
        public const string LOG_PART_LOG_HIGHLIGHT_COLOR = "{8}";

        /// <summary>
        /// The default log format used by log handlers. Override in derived types to use a different default,
        /// or use <see cref="LogFormat"/> to customize the format for individual log handler instances.
        /// </summary>
        protected virtual string DefaultLogFormat =>
            $"({LOG_PART_CALLER_TYPE}.{LOG_PART_CALLER_METHOD}|{LOG_PART_CALLER_FILE}:{LOG_PART_CALLER_LINE}) {LOG_PART_MESSAGE}";

        private string m_LogFormat;

        private bool m_IncludeTimestamp = false;
        private bool m_IncludeLogLevel = false;
        private bool m_IncludeHighlightColor = false;

        /// <summary>
        /// Defines the format of the log message.
        /// The following wrapped in {} are substituted at runtime
        ///  - <see cref="LOG_PART_MESSAGE"/>
        ///  - <see cref="LOG_PART_CALLER_TYPE"/>
        ///  - <see cref="LOG_PART_CALLER_METHOD"/>
        ///  - <see cref="LOG_PART_CALLER_PATH"/>
        ///  - <see cref="LOG_PART_CALLER_FILE"/>
        ///  - <see cref="LOG_PART_CALLER_LINE"/>
        ///  - <see cref="LOG_PART_TIMESTAMP"/>
        ///  - <see cref="LOG_PART_LOG_LEVEL"/>
        ///  - <see cref="LOG_PART_LOG_HIGHLIGHT_COLOR"/>
        ///
        /// Default: "({LOG_PART_CALLER_TYPE}.{LOG_PART_CALLER_METHOD}|{LOG_PART_CALLER_FILE}:{LOG_PART_CALLER_LINE}) {LOG_PART_MESSAGE}"
        /// </summary>
        /// <example>
        /// The default format produces "(MyType.MyMethod|MyFile:12) Message" for a log "Message" issued in the
        /// file "MyFile" on line "12" from type "MyType" and method "MyMethod".
        /// </example>
        public string LogFormat
        {
            get { return m_LogFormat; }
            set
            {
                m_LogFormat = value;

                m_IncludeLogLevel = m_LogFormat.Contains(LOG_PART_LOG_LEVEL);
                m_IncludeTimestamp = m_LogFormat.Contains(LOG_PART_TIMESTAMP);
                m_IncludeHighlightColor = m_LogFormat.Contains(LOG_PART_LOG_HIGHLIGHT_COLOR);
            }
        }

        /// <summary>
        /// Defines the format of the timestamp written to the log, when included in the log format
        /// Default: HH:mm:ss (ex: 12:34:56)
        /// </summary>
        public string TimestampFormat { get; set; } = "HH:mm:ss";

        /// <summary>
        /// Indicates the minimum message severity to handle. Logs below this level are ignored.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        protected AbstractLogHandler()
        {
            LogFormat = DefaultLogFormat;
        }

        internal void HandleLog(LogLevel level, string message, in CallerInfo callerInfo)
        {
            if ((int)level < (int)MinimumLevel)
            {
                return;
            }

            HandleFormattedLog(level, FormatLog(level, message, in callerInfo));
        }

        /// <summary>
        /// Allows log handlers to handle a log formatted with <see cref="LogFormat"/>.
        /// </summary>
        /// <param name="level">The log's level.</param>
        /// <param name="formattedLog">The formatted log, based on <see cref="LogFormat"/>.</param>
        protected abstract void HandleFormattedLog(LogLevel level, string formattedLog);

        /// <summary>
        /// Return a color value as a string to highlight a section of the log.
        /// Color may be constant or based on <see cref="CallerInfo"/>
        /// Related: <see cref="LOG_PART_LOG_HIGHLIGHT_COLOR"/>
        /// </summary>
        /// <param name="callerInfo">The log caller's info.</param>
        /// <returns>A color value as a string to use for highlighting.</returns>
        protected virtual string GetHighlightColorFor(in CallerInfo callerInfo)
        {
            // Middle grey has the best chance of showing up on dark and light backgrounds for a default value.
            // handlers that support color are expected to define their own value(s).
            return "#888888";
        }

        private string FormatLog(LogLevel level, string message, in CallerInfo callerInfo)
        {
            return string.Format(
                m_LogFormat,
                message,
                callerInfo.TypeName,
                callerInfo.MethodName,
                callerInfo.FilePath,
                callerInfo.FileName,
                callerInfo.LineNumber,
                (m_IncludeTimestamp ? DateTime.Now.ToString(TimestampFormat) : string.Empty),
                (m_IncludeLogLevel ? $"[{level.ToString()[0]}]" : string.Empty),
                (m_IncludeHighlightColor ? GetHighlightColorFor(in callerInfo) : string.Empty)
            );
        }

        // ----- Inner Types ----- //
        protected internal readonly struct CallerInfo
        {
            public readonly string TypeName;
            public readonly string MethodName;
            public readonly string FilePath;
            public readonly string FileName;
            public readonly int LineNumber;

            public CallerInfo(
                string typeName,
                string methodName,
                string filePath,
                string fileName,
                int lineNumber
            )
            {
                TypeName = typeName;
                MethodName = methodName;
                FilePath = filePath;
                FileName = fileName;
                LineNumber = lineNumber;
            }
        }
    }
}