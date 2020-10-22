using System;
using System.IO;

namespace Anvil.CSharp.Logging
{
    /// <summary>
    /// Forwards logs to a given text file.
    /// </summary>
    public class FileLogHandler : ILogHandler
    {
        private readonly StreamWriter m_Writer;

        /// <summary>
        /// Indicates whether to prefix logs with a timestamp.
        /// Example: 12:34:56
        /// </summary>
        public bool IncludeTimestamp { get; set; } = true;

        /// <summary>
        /// Indicates whether to prefix logs with a symbol indicating their severity.
        /// Example: [D]
        /// </summary>
        public bool IncludeLogLevel { get; set; } = true;

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

        public void HandleLog(LogLevel level, string message)
        {
            if ((int)level < (int)MinimumLevel)
            {
                return;
            }

            if (IncludeLogLevel)
            {
                message = $"[{level.ToString()[0]}] {message}";
            }

            if (IncludeTimestamp)
            {
                message = $"{DateTime.Now:HH:mm:ss} {message}";
            }

            m_Writer.WriteLine(message);
        }
    }
}
