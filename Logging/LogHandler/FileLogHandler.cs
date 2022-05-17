using System;
using System.IO;
using System.Text;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Forwards logs to a given text file.
    /// </summary>
    public class FileLogHandler : AbstractAnvilBase, ILogHandler
    {
        public const string LOG_CONTEXT_CALLER_DERIVED_TYPE = "{0}";
        public const string LOG_CONTEXT_CALLER_FILE = "{1}";
        public const string LOG_CONTEXT_CALLER_METHOD = "{2}";
        public const string LOG_CONTEXT_CALLER_LINE = "{3}";

        private readonly StreamWriter m_Writer;

        /// <summary>
        /// Indicates whether to prefix logs with a timestamp.
        /// The format is dictated by <see cref="TimestampFormat"/>
        /// </summary>
        public bool IncludeTimestamp { get; set; } = true;

        /// <summary>
        /// Defines the format of the timestamp written to the log
        /// Default: HH:mm:ss(Ex: 12:34:56)
        /// </summary>
        public string TimestampFormat { get; set; } = "HH:mm:ss";

        /// <summary>
        /// Indicates whether to prefix logs with a symbol indicating their severity.
        /// </summary>
        /// <example>[D]</example>
        public bool IncludeLogLevel { get; set; } = true;

        /// <summary>
        /// Defines the format of the context added to log messages.
        /// The following wraped in {} are substituted at runtime
        ///  - <see cref="LOG_CONTEXT_CALLER_DERIVED_TYPE"/>
        ///  - <see cref="LOG_CONTEXT_CALLER_FILE"/>
        ///  - <see cref="LOG_CONTEXT_CALLER_METHOD"/>
        ///  - <see cref="LOG_CONTEXT_CALLER_LINE"/>
        ///
        /// Default: "({LOG_CONTEXT_CALLER_FILE}|{LOG_CONTEXT_CALLER_METHOD}:{LOG_CONTEXT_CALLER_LINE}) "
        /// </summary>
        /// <example>
        /// The default format produces "(MyFile|MyCallingMethod:12) " for a log issued in the
        /// file "MyFile" from method "MyCallingMethod" on line "12".
        /// </example>
        public string LogContextFormat { get; set; } = $"({LOG_CONTEXT_CALLER_FILE}|{LOG_CONTEXT_CALLER_METHOD}:{LOG_CONTEXT_CALLER_LINE}) ";

        /// <summary>
        /// Indicates the minimum message severity to handle. Logs below this level are ignored.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Creates a `FileLogHandler` that can write to the given file path.
        /// </summary>
        /// <param name="path">The text file to output messages to.</param>
        /// <param name="append">If true, the file will have new logs appended to it, otherwise the file is replaced.</param>
        public FileLogHandler(string path, bool append = true)
        {
            m_Writer = new StreamWriter(path, append)
            {
                AutoFlush = true
            };
        }

        protected override void DisposeSelf()
        {
            m_Writer.Dispose();
        }

        public void HandleLog(
            LogLevel level,
            string message,
            string callerDerivedTypeName,
            string callerPath,
            string callerName,
            int callerLine)
        {
            if ((int)level < (int)MinimumLevel)
            {
                return;
            }

            string timestamp = IncludeTimestamp ? $"{DateTime.Now.ToString(TimestampFormat)} " : string.Empty;
            string logLevel = IncludeLogLevel ? $"[{level.ToString()[0]}] " : string.Empty;

            string filename = Path.GetFileNameWithoutExtension(callerPath);
            string context = string.Format(
                LogContextFormat, 
                callerDerivedTypeName, filename, callerName, callerLine
                );

            m_Writer.WriteLine($"{timestamp}{logLevel}{context}{message}");
        }
    }
}
