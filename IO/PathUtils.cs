using System.IO;

namespace Anvil.CSharp.IO
{
    public static class PathUtils
    {
        public static bool IsValidDirectoryPath(string path)
        {
            // Make sure there isn't a file at the path AND it doesn't contain any invalid characters.
            // Note: Path.GetInvalidPathChars isn't comprehensive. It's a lazy best effort. - https://docs.microsoft.com/en-us/dotnet/api/system.io.path.getinvalidpathchars?view=net-5.0#remarks
            return !string.IsNullOrEmpty(path)
             && !File.Exists(path)
             && path.IndexOfAny(Path.GetInvalidPathChars()) == -1;
        }
    }
}
