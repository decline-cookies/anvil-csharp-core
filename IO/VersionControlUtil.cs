using System.IO;
using System.Linq;

namespace Anvil.CSharp.IO
{
    /// <summary>
    /// Useful utilities for interacting with version control.
    /// </summary>
    public static class VersionControlUtil
    {
        /// <summary>
        /// Searches the provided path and parents for the root of the repository this project is located.
        ///
        /// </summary>
        /// <param name="searchPath">The path to start the search from.</param>
        /// <returns>
        /// The full path to the root of the repository. If no repository is found then null is returned.
        /// </returns>
        public static string FindRepositoryPath(string searchPath)
        {
            DirectoryInfo currentPath = new DirectoryInfo(searchPath);
            while (currentPath != null && !currentPath.EnumerateDirectories(".git").Any())
            {
                currentPath = currentPath.Parent;
            }

            return currentPath?.FullName;
        }
    }
}