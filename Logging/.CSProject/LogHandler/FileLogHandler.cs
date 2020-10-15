using System;
using System.IO;

namespace Anvil.CSharp.Logging
{
    public class FileLogHandler : ILogHandler
    {
        private readonly StreamWriter m_Writer;

        public bool IncludeTimestamp { get; set; } = true;
        public bool IncludeLogLevel { get; set; } = true;
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

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
