using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        private StreamWriter m_Writer;
        private readonly string m_Path;
        private readonly string m_Directory;
        private readonly string m_FileName;
        private readonly string m_Extension;

        private readonly WriteMode m_WriteMode;
        
        private readonly long? m_RotateFileSizeLimit;
        private readonly int? m_RotateFileCountLimit;

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

            if (m_RotateFileSizeLimit < 0)
            {
                throw new Exception($"Expected a positive integer for file size limit: {m_RotateFileSizeLimit}");
            }
            if (m_RotateFileCountLimit < 0)
            {
                throw new Exception($"Expected a positive integer for file count limit: {m_RotateFileCountLimit}");
            }

            m_Directory = Path.GetDirectoryName(m_Path);
            m_FileName = Path.GetFileNameWithoutExtension(m_Path);
            m_Extension = Path.GetExtension(m_Path);

            if (!Directory.Exists(m_Directory))
            {
                Directory.CreateDirectory(m_Directory);
            }

            CreateWriter();

            if (m_WriteMode == WriteMode.Replace && m_RotateFileSizeLimit.HasValue)
            {
                foreach ((_, string filePath) in GetRotatedFiles())
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
                int logLength = Encoding.Unicode.GetByteCount(log);
                long fileLength = m_Writer.BaseStream.Length;

                if ((fileLength + logLength) > m_RotateFileSizeLimit)
                {
                    if (logLength > m_RotateFileSizeLimit)
                    {
                        throw new Exception("Message size exceeds file size limit, logging failed " +
                            $"({logLength} > {m_RotateFileSizeLimit})");
                    }

                    this.RotateFiles();
                }
            }

            m_Writer.WriteLine(log);
        }

        private void RotateFiles()
        {
            m_Writer.Dispose();
            m_Writer = null;

            Dictionary<int, string> fileMap = new Dictionary<int, string>{ [0] = m_Path };
            foreach ((int index, string path) in GetRotatedFiles())
            {
                if (index < 0 || index > m_RotateFileCountLimit)
                {
                    File.Delete(path);
                }
                else if (fileMap.ContainsKey(index))
                {
                    // In case of duplicate indices (ex. "log.1.txt" vs "log.001.txt") keep the shortest
                    if (fileMap[index].Length < path.Length)
                    {
                        File.Delete(path);
                    }
                    else
                    {
                        File.Delete(fileMap[index]);
                        fileMap[index] = path;
                    }
                }
                else
                {
                    fileMap[index] = path;
                }
            }

            RotateRecursive(0);

            CreateWriter();

            void RotateRecursive(int index)
            {
                if (index >= m_RotateFileCountLimit)
                {
                    File.Delete(fileMap[index]);
                }
                else
                {
                    if (fileMap.ContainsKey(index + 1))
                    {
                        RotateRecursive(index + 1);
                    }

                    File.Move(fileMap[index], GetRotatedFilePath(index + 1));
                }
            }
        }

        private string GetRotatedFilePath(int index)
        {
            if (index < 0)
            {
                throw new IndexOutOfRangeException($"Invalid file index: {index}");
            }

            return (index == 0 ? m_Path : Path.Combine(m_Directory, $"{m_FileName}.{index}{m_Extension}"));
        }

        private IEnumerable<(int index, string path)> GetRotatedFiles()
        {
            string pattern = @$"{m_FileName}\.(\d+){(m_Extension.Length == 0 ? "" : @$"\{m_Extension}")}";

            return Directory.GetFiles(m_Directory, $"{m_FileName}.*")
                .Select(path => (path, match: Regex.Match(path, pattern)))
                .Where(data => data.match.Success)
                .Select(data => (index: ParseIndex(data.match.Groups[1].Value), data.path));

            // Interpret index digits as an int, falling back on default
            int ParseIndex(string indexStr)
            {
                int index;
                int.TryParse(indexStr, out index);
                return index;
            }
        }
    }
}