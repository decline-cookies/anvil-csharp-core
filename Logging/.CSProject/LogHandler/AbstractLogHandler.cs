using System;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// A type capable of handling logs received by <see cref="Log"/>.
    /// </summary>
    public abstract class AbstractLogHandler
    {
        public const string LOG_PART_MESSAGE = "{0}";
        public const string LOG_PART_CALLER_TYPE = "{1}";
        public const string LOG_PART_CALLER_METHOD = "{2}";
        public const string LOG_PART_CALLER_PATH = "{3}";
        public const string LOG_PART_CALLER_FILE = "{4}";
        public const string LOG_PART_CALLER_LINE = "{5}";
        public const string LOG_PART_TIMESTAMP = "{6}";
        public const string LOG_PART_LOG_LEVEL = "{7}";

        protected virtual string DefaultLogFormat =>
            $"({LOG_PART_CALLER_TYPE}.{LOG_PART_CALLER_METHOD}|{LOG_PART_CALLER_FILE}:{LOG_PART_CALLER_LINE}) {LOG_PART_MESSAGE}";

        private string m_LogFormat;

        private bool m_IncludeTimestamp = false;
        private bool m_IncludeLogLevel = false;

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
            }
        }

        /// <summary>
        /// Defines the format of the timestamp written to the log, when included in the log format
        /// Default: HH:mm:ss(Ex: 12:34:56)
        /// </summary>
        public string TimestampFormat { get; set; } = "HH:mm:ss";

        public AbstractLogHandler()
        {
            LogFormat = DefaultLogFormat;
        }

        public void HandleLog(LogLevel level, string message, in CallerInfo callerInfo)
        {
            HandleFormattedLog(level, string.Format(
                m_LogFormat,
                message,
                callerInfo.TypeName,
                callerInfo.MethodName,
                callerInfo.FilePath,
                callerInfo.FileName,
                callerInfo.LineNumber,
                (m_IncludeTimestamp ? DateTime.Now.ToString(TimestampFormat) : string.Empty),
                (m_IncludeLogLevel ? $"[{level.ToString()[0]}]" : string.Empty)
            ));
        }

        protected abstract void HandleFormattedLog(LogLevel level, string formattedLog);
    }
}
