using System;
using System.IO;
using Anvil.CSharp.Core;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Forwards logs to a given text file.
    /// </summary>
    public class FileLogHandler : AbstractAnvilBase, ILogHandler
    {
        /// <summary>
        /// The type of log files the handler should generate.
        /// </summary>
        public enum LogType
        {
            /// <summary>
            /// New logs are appended to the existing log file.
            /// </summary>
            Append,
            /// <summary>
            /// A new log file is created each session, overwriting the previous one if it exists.
            /// </summary>
            Replace,
        }

        public const string LOG_CONTEXT_CALLER_DERIVED_TYPE = "{0}";
        public const string LOG_CONTEXT_CALLER_FILE = "{1}";
        public const string LOG_CONTEXT_CALLER_METHOD = "{2}";
        public const string LOG_CONTEXT_CALLER_LINE = "{3}";

        private StreamWriter m_Writer;
        private readonly string m_Path;
        private readonly LogType m_LogType;
        private readonly bool m_Rotate;

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
        /// The following wrapped in {} are substituted at runtime
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
        /// The maximum file size in bytes before the file is rotated. Default is 10MB.
        /// </summary>
        public long RotateFileSizeLimit { get; set; } = 10L * 1024 * 1024;

        /// <summary>
        /// The maximum number of rotated files to keep before deleting the oldest. Default is 5.
        /// Ex. If the limit is 5, up to 5 rotated files in addition to the original log file, will be kept. After reaching this limit,
        /// the next time the log file is rotated, the oldest file will be deleted. If the limit is 0, no rotated files are kept,
        /// i.e. when the log file is rotated, it is simply erased and starts over.
        /// </summary>
        public int RotateFileCountLimit { get; set; } = 5;

        /// <summary>
        /// Creates a `FileLogHandler` that can write to the given file path.
        /// </summary>
        /// <param name="path">The text file to output messages to.</param>
        /// <param name="logType">The <see cref="LogType"/> to use.</param>
        /// <param name="rotate">
        /// If true, the file will be rotated when the size limit is reached. When a file is rotated, it has an index appended to it,
        /// i.e. "log.txt" is renamed "log.1.txt", and a new "log.txt" is opened. If "log.1.txt" already exists, it is first renamed
        /// "log.2.txt", and so on, rotating through log files. Once the file count limit is reached, the oldest file is deleted.
        /// </param>
        public FileLogHandler(string path, LogType logType = LogType.Append, bool rotate = true)
        {
            m_Path = path;
            m_LogType = logType;
            m_Rotate = rotate;

            string directory = Path.GetDirectoryName(m_Path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            CreateWriter();

            if (logType == LogType.Replace && rotate)
            {
                int index = 1;
                string rotatedFilePath = GetRotatedFilePath(index);
                while (File.Exists(rotatedFilePath))
                {
                    File.Delete(rotatedFilePath);
                    rotatedFilePath = GetRotatedFilePath(++index);
                }
            }
        }

        private void CreateWriter()
        {
            m_Writer = new StreamWriter(m_Path, append: (m_LogType == LogType.Append))
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

            if (m_Rotate && m_Writer.BaseStream.Length > RotateFileSizeLimit)
            {
                this.RotateFiles();
            }
        }

        private void RotateFiles()
        {
            m_Writer.Close();
            m_Writer = null;

            // Determine how many rotated log files already exist
            int index = 1;
            string rotatedFilePath = GetRotatedFilePath(index);
            while (File.Exists(rotatedFilePath))
            {
                rotatedFilePath = GetRotatedFilePath(++index);
            }
            --index;

            // Delete any files in excess of the file count limit
            while (index >= RotateFileCountLimit)
            {
                File.Delete(GetRotatedFilePath(index));
                --index;
            }

            // Rotate remaining files
            while (index >= 0)
            {
                File.Move(GetRotatedFilePath(index), GetRotatedFilePath(index + 1));
                --index;
            }

            CreateWriter();
        }

        private string GetRotatedFilePath(int index)
        {
            if (index == 0)
            {
                return m_Path;
            }

            string directory = Path.GetDirectoryName(m_Path);
            string fileName = Path.GetFileNameWithoutExtension(m_Path);
            string extension = Path.GetExtension(m_Path);
            
            return Path.Combine(directory, $"{fileName}.{index}{extension}");
        }
    }
}