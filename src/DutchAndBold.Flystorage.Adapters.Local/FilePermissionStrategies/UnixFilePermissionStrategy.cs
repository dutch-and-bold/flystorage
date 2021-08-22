using System;
using System.Runtime.Versioning;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Local.Contracts;
using Mono.Unix;

namespace DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public class UnixFilePermissionStrategy : IFilePermissionStrategy
    {
        private const FileAccessPermissions FilePermissionsPublic = FileAccessPermissions.UserRead |
                                                                     FileAccessPermissions.UserWrite |
                                                                     FileAccessPermissions.GroupRead |
                                                                     FileAccessPermissions.OtherRead;

        private const FileAccessPermissions FilePermissionsPrivate = FileAccessPermissions.UserRead |
                                                                      FileAccessPermissions.UserWrite;

        private const FileAccessPermissions DirectoryPermissionsPublic = FileAccessPermissions.UserRead |
                                                                         FileAccessPermissions.UserWrite |
                                                                         FileAccessPermissions.UserExecute |
                                                                         FileAccessPermissions.GroupRead |
                                                                         FileAccessPermissions.GroupExecute |
                                                                         FileAccessPermissions.OtherRead |
                                                                         FileAccessPermissions.OtherExecute;

        private const FileAccessPermissions DirectoryPermissionsPrivate = FileAccessPermissions.UserRead |
                                                                           FileAccessPermissions.UserWrite |
                                                                           FileAccessPermissions.UserExecute;

        public void SetFilePermissions(string fullPath, Visibility visibility)
        {
            try
            {
                var unixFileInfo = new UnixFileInfo(fullPath)
                {
                    FileAccessPermissions = visibility == Visibility.Public
                        ? FilePermissionsPublic
                        : FilePermissionsPrivate
                };
                unixFileInfo.Refresh();
            }
            catch (InvalidOperationException e)
            {
                throw UnableToSetVisibilityException.AtLocation(fullPath, e);
            }
        }

        public void SetDirectoryPermissions(string fullPath, Visibility visibility)
        {
            try
            {
                var unixDirectoryInfo = new UnixDirectoryInfo(fullPath)
                {
                    FileAccessPermissions = visibility == Visibility.Public
                        ? DirectoryPermissionsPublic
                        : DirectoryPermissionsPrivate
                };
                unixDirectoryInfo.Refresh();
            }
            catch (InvalidOperationException e)
            {
                throw UnableToSetVisibilityException.AtLocation(fullPath, e);
            }
        }

        public Visibility GetFilePermissions(string fullPath)
        {
            return new UnixFileInfo(fullPath).FileAccessPermissions.CompareTo(FilePermissionsPublic) >= 0
                ? Visibility.Public
                : Visibility.Private;
        }

        public Visibility GetDirectoryPermissions(string fullPath)
        {
            return new UnixDirectoryInfo(fullPath).FileAccessPermissions.CompareTo(DirectoryPermissionsPublic) >= 0
                ? Visibility.Public
                : Visibility.Private;
        }
    }
}