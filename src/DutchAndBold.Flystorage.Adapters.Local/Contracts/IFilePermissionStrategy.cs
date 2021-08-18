using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Adapters.Local.Contracts
{
    public interface IFilePermissionStrategy
    {
        /// <summary>
        /// Set file permissions for file at given path.
        /// </summary>
        /// <param name="fullPath">The full location path to the file.</param>
        /// <param name="visibility"></param>
        public void SetFilePermissions(string fullPath, Visibility visibility);

        /// <summary>
        /// Set directory permissions for file at given path.
        /// </summary>
        /// <param name="fullPath">The full location path to the directory.</param>
        /// <param name="visibility"></param>
        public void SetDirectoryPermissions(string fullPath, Visibility visibility);

        /// <summary>
        /// Gets the permissions for a file at given path.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public Visibility GetFilePermissions(string fullPath);

        /// <summary>
        /// Gets the permissions for a directory at given path.
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public Visibility GetDirectoryPermissions(string fullPath);
    }
}