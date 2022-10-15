namespace Anvil.CSharp.Logging
{
    public readonly struct CallerInfo
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