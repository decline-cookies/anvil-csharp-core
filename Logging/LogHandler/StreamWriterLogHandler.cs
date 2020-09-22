using System.IO;

namespace Anvil.CSharp.Logging
{
    public class StreamWriterLogHandler : ILogHandler
    {
        private readonly StreamWriter m_Writer;

        public StreamWriterLogHandler(StreamWriter writer)
        {
            m_Writer = writer;
        }

        public StreamWriterLogHandler(string path, bool append = true)
        {
            m_Writer = new StreamWriter(path, append);
        }

        public void HandleLog(LogLevel level, string message)
        {
            m_Writer.WriteLine(message);
        }
    }
}
