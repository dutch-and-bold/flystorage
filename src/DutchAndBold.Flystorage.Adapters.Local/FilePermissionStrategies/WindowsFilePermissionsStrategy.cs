using System;
using System.IO;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Local.Contracts;

namespace DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies
{
    [SupportedOSPlatform("windows")]
    public class WindowsFilePermissionsStrategy : IFilePermissionStrategy
    {
        public void SetFilePermissions(string fullPath, Visibility visibility)
        {
            var fileSecurity = new FileSecurity();

            if (visibility == Visibility.Public)
            {
                fileSecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Read,
                        AccessControlType.Allow));
                fileSecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Write,
                        AccessControlType.Allow));

                // TODO:  Add group and other access rights.
                throw new NotImplementedException("Public access rights are not yet implemented on Windows.");
            }

            if (visibility == Visibility.Private)
            {
                fileSecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Read,
                        AccessControlType.Allow));
                fileSecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Write,
                        AccessControlType.Allow));
            }

            var fileInfo = new FileInfo(fullPath);
            fileInfo.SetAccessControl(fileSecurity);
        }

        public void SetDirectoryPermissions(string fullPath, Visibility visibility)
        {
            var directorySecurity = new DirectorySecurity();

            if (visibility == Visibility.Public)
            {
                directorySecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Read,
                        AccessControlType.Allow));
                directorySecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Write,
                        AccessControlType.Allow));
                directorySecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.ExecuteFile,
                        AccessControlType.Allow));

                // TODO:  Add group and other access rights.
                throw new NotImplementedException("Public access rights are not yet implemented on Windows.");
            }

            if (visibility == Visibility.Private)
            {
                directorySecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Read,
                        AccessControlType.Allow));
                directorySecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.Write,
                        AccessControlType.Allow));
                directorySecurity.AddAccessRule(
                    new FileSystemAccessRule(
                        Environment.UserDomainName,
                        FileSystemRights.ExecuteFile,
                        AccessControlType.Allow));
            }

            var directoryInfo = new DirectoryInfo(fullPath);
            directoryInfo.SetAccessControl(directorySecurity);
        }

        public Visibility GetFilePermissions(string fullPath)
        {
            throw new NotImplementedException();
        }

        public Visibility GetDirectoryPermissions(string fullPath)
        {
            throw new NotImplementedException();
        }
    }
}