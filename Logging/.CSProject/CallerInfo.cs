namespace Anvil.CSharp.Logging
{
    public readonly struct CallerInfo
    {
        public readonly string LoggerName;
        public readonly string MethodName;
        public readonly string FullPath;
        public readonly string FileName;
        public readonly int LineNumber;

        public CallerInfo(
            string loggerName,
            string methodName,
            string fullPath,
            string fileName,
            int lineNumber
        )
        {
            LoggerName = loggerName;
            MethodName = methodName;
            FullPath = fullPath;
            FileName = fileName;
            LineNumber = lineNumber;
        }
    }
}