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
        private readonly FileAccessPermissions _filePermissionsPublic = FileAccessPermissions.UserRead |
                                                                        FileAccessPermissions.UserWrite |
                                                                        FileAccessPermissions.GroupRead |
                                                                        FileAccessPermissions.OtherRead;

        private readonly FileAccessPermissions _filePermissionsPrivate = FileAccessPermissions.UserRead |
                                                                         FileAccessPermissions.UserWrite;

        private readonly FileAccessPermissions _directoryPermissionsPublic = FileAccessPermissions.UserRead |
                                                                             FileAccessPermissions.UserWrite |
                                                                             FileAccessPermissions.UserExecute |
                                                                             FileAccessPermissions.GroupRead |
                                                                             FileAccessPermissions.GroupExecute |
                                                                             FileAccessPermissions.OtherRead |
                                                                             FileAccessPermissions.OtherExecute;

        private readonly FileAccessPermissions _directoryPermissionsPrivate = FileAccessPermissions.UserRead |
                                                                              FileAccessPermissions.UserWrite |
                                                                              FileAccessPermissions.UserExecute;

        public UnixFilePermissionStrategy(
            FileAccessPermissions? filePermissionsPublic = null,
            FileAccessPermissions? filePermissionsPrivate = null,
            FileAccessPermissions? directoryPermissionsPublic = null,
            FileAccessPermissions? directoryPermissionsPrivate = null)
        {
            _filePermissionsPublic = filePermissionsPublic ?? _filePermissionsPublic;
            _filePermissionsPrivate = filePermissionsPrivate ?? _filePermissionsPrivate;
            _directoryPermissionsPublic = directoryPermissionsPublic ?? _directoryPermissionsPublic;
            _directoryPermissionsPrivate = directoryPermissionsPrivate ?? _directoryPermissionsPrivate;
        }

        public void SetFilePermissions(string fullPath, Visibility visibility)
        {
            try
            {
                var unixFileInfo = new UnixFileInfo(fullPath)
                {
                    FileAccessPermissions = visibility == Visibility.Public
                        ? _filePermissionsPublic
                        : _filePermissionsPrivate
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
                        ? _directoryPermissionsPublic
                        : _directoryPermissionsPrivate
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
            return new UnixFileInfo(fullPath).FileAccessPermissions.CompareTo(_filePermissionsPublic) >= 0
                ? Visibility.Public
                : Visibility.Private;
        }

        public Visibility GetDirectoryPermissions(string fullPath)
        {
            return new UnixDirectoryInfo(fullPath).FileAccessPermissions.CompareTo(_directoryPermissionsPublic) >= 0
                ? Visibility.Public
                : Visibility.Private;
        }
    }
}