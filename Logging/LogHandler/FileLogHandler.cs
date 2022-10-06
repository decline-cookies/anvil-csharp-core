using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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
        public enum WriteMode
        {
            /// <summary>
            /// New logs are appended to the existing log file.
            /// </summary>
            Append,
            /// <summary>
            /// A new log file is created, overwriting the previous one if it exists.
            /// </summary>
            Replace,
        }

        public const string LOG_CONTEXT_CALLER_DERIVED_TYPE = "{0}";
        public const string LOG_CONTEXT_CALLER_FILE = "{1}";
        public const string LOG_CONTEXT_CALLER_METHOD = "{2}";
        public const string LOG_CONTEXT_CALLER_LINE = "{3}";

        private const string TRUNCATED_LOG_MESSAGE = "...(message exceeds file size limit and was truncated)";

        private StreamWriter m_Writer;
        private readonly string m_Path;
        private readonly string m_Directory;
        private readonly string m_FileName;
        private readonly string m_Extension;

        private readonly WriteMode m_WriteMode;
        
        private readonly long? m_RotateFileSizeLimit;
        private readonly int? m_RotateFileCountLimit;

        private readonly int m_TruncatedLogMessageByteCount;
        private readonly int m_NewlineByteCount;

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
        /// Default: "({LOG_CONTEXT_CALLER_FILE}:{LOG_CONTEXT_CALLER_LINE}|{LOG_CONTEXT_CALLER_METHOD}) "
        /// </summary>
        /// <example>
        /// The default format produces "(MyFile:12|MyCallingMethod) " for a log issued in the
        /// file "MyFile" on line "12" from method "MyCallingMethod".
        /// </example>
        public string LogContextFormat { get; set; } = $"({LOG_CONTEXT_CALLER_FILE}:{LOG_CONTEXT_CALLER_LINE}|{LOG_CONTEXT_CALLER_METHOD}) ";

        /// <summary>
        /// Indicates the minimum message severity to handle. Logs below this level are ignored.
        /// </summary>
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Creates a `FileLogHandler` that can write to the given file path.
        /// </summary>
        /// <param name="path">The text file to output messages to.</param>
        /// <param name="writeMode">The <see cref="WriteMode"/> to use.</param>
        /// <param name="rotateFileSizeLimit">
        /// The maximum file size in bytes before the file is rotated. Default is 10MB.
        /// If set, the file will be rotated when the next log would exceed the limit. When a file is rotated, an index is appended to it,
        /// ex. "log.txt" is renamed "log.1.txt", and a new "log.txt" is opened. If "log.1.txt" already exists, it is first renamed
        /// "log.2.txt", and so on, rotating through log files.
        /// </param>
        /// <param name="rotateFileCountLimit">
        /// The maximum number of rotated files to keep before deleting the oldest. Default is 5.
        /// Ex. If the limit is 5, up to 5 rotated files in addition to the original log file, will be kept. After reaching this limit,
        /// the next time the log file is rotated, the oldest file will be deleted. If the limit is 0, no rotated files are kept,
        /// i.e. when the log file is rotated, it is simply erased and started over.
        /// </summary>
        public FileLogHandler(
            string path,
            WriteMode writeMode = WriteMode.Append,
            long? rotateFileSizeLimit = 10L * 1024 * 1024,
            int? rotateFileCountLimit = 5
        )
        {
            m_Path = path;
            m_WriteMode = writeMode;
            m_RotateFileSizeLimit = rotateFileSizeLimit;
            m_RotateFileCountLimit = rotateFileCountLimit;

            m_Directory = Path.GetDirectoryName(m_Path);
            m_FileName = Path.GetFileNameWithoutExtension(m_Path);
            m_Extension = Path.GetExtension(m_Path);

            if (!Directory.Exists(m_Directory))
            {
                Directory.CreateDirectory(m_Directory);
            }

            CreateWriter();

            m_TruncatedLogMessageByteCount = m_Writer.Encoding.GetByteCount(TRUNCATED_LOG_MESSAGE);
            m_NewlineByteCount = m_Writer.Encoding.GetByteCount(m_Writer.NewLine);

            Trace.Assert(m_RotateFileSizeLimit >= m_TruncatedLogMessageByteCount, "Invalid rotated file size limit");
            Trace.Assert(m_RotateFileCountLimit >= 0, "Invalid rotated file count limit");

            if (m_WriteMode == WriteMode.Replace && m_RotateFileSizeLimit.HasValue)
            {
                foreach (string filePath in GetExistingRotatedFilePaths())
                {
                    File.Delete(filePath);
                }
            }
        }

        private void CreateWriter()
        {
            m_Writer = new StreamWriter(m_Path, append: (m_WriteMode == WriteMode.Append))
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

            string log = $"{timestamp}{logLevel}{context}{message}";

            if (m_RotateFileSizeLimit != null)
            {
                // Check GetMaxByteCount() first as it's much cheaper to run on every log
                int logMaxByteCount = m_Writer.Encoding.GetMaxByteCount(log.Length) + m_NewlineByteCount;
                long fileByteCount = m_Writer.BaseStream.Length;

                if ((fileByteCount + logMaxByteCount) > m_RotateFileSizeLimit)
                {
                    // If this log may push the file over the size limit, run the more expensive GetByteCount()
                    int logByteCount = m_Writer.Encoding.GetByteCount(log) + m_NewlineByteCount;

                    if ((fileByteCount + logByteCount) > m_RotateFileSizeLimit)
                    {
                        // If the file is empty, and this log alone is exceeding the file size limit, don't rotate
                        if (fileByteCount > 0)
                        {
                            this.RotateFiles();
                        }

                        if (logByteCount > m_RotateFileSizeLimit)
                        {
                            // This should be impossible, no single string can exceed `int.MaxValue` bytes
                            Debug.Assert(m_RotateFileSizeLimit <= int.MaxValue);

                            // Remove at least enough characters to meet the file size limit
                            // This may remove more than strictly necessary, if any characters are multiple bytes
                            int bytesToRemove = (logByteCount - (int)m_RotateFileSizeLimit) + m_TruncatedLogMessageByteCount;
                            int targetLength = log.Length - bytesToRemove;

                            log = log.Substring(0, Math.Max(0, targetLength)) + TRUNCATED_LOG_MESSAGE;
                        }
                    }
                }
            }

            m_Writer.WriteLine(log);
        }

        private void RotateFiles()
        {
            m_Writer.Dispose();
            m_Writer = null;

            // Get all existing rotated log file paths, collecting valid paths to be rotated
            List<string> existingFilePaths = GetExistingRotatedFilePaths().ToList();
            List<string> validPaths = new List<string>{ m_Path };

            for (int i = 1; i < m_RotateFileCountLimit && existingFilePaths.Any(); i++)
            {
                string filePath = GetRotatedFilePathForIndex(i);
                if (existingFilePaths.Remove(filePath))
                {
                    validPaths.Add(filePath);
                }
                else
                {
                    break;
                }
            }

            // Delete any remaining out-of-range files
            foreach (string path in existingFilePaths)
            {
                File.Delete(path);
            }

            // Rotate valid files
            for (int i = validPaths.Count - 1; i >= 0; i--)
            {
                File.Move(validPaths[i], GetRotatedFilePathForIndex(i + 1));
            }

            CreateWriter();
        }

        private string GetRotatedFilePathForIndex(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException($"Invalid file index: {index}");
            }

            return (index == 0 ? m_Path : Path.Combine(m_Directory, $"{m_FileName}.{index}{m_Extension}"));
        }

        private IEnumerable<string> GetExistingRotatedFilePaths()
        {
            string pattern = @$"{m_FileName}\.(\d+){(m_Extension.Length == 0 ? "" : @$"\{m_Extension}")}";

            return Directory.GetFiles(m_Directory, $"{m_FileName}.*").Where(path => Regex.IsMatch(path, pattern));
        }
    }
}