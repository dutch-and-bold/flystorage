using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Principal;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Local.Contracts;

namespace DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies
{
    [SupportedOSPlatform("windows")]
    public class WindowsFilePermissionsStrategy : IFilePermissionStrategy
    {
        private readonly FileSystemAccessRule _privateFileAccessRule;
        private readonly FileSystemAccessRule _privateDirectoryAccessRule;

        /// <summary>
        /// Use guests identity with default access rules.
        /// </summary>
        public WindowsFilePermissionsStrategy() : this(null, null, null)
        {
        }

        /// <summary>
        /// Use specified access rules.
        /// </summary>
        /// <param name="privateFileAccessRule"></param>
        /// <param name="privateDirectoryAccessRule"></param>
        public WindowsFilePermissionsStrategy(
            FileSystemAccessRule privateFileAccessRule,
            FileSystemAccessRule privateDirectoryAccessRule)
            : this(null, privateFileAccessRule, privateDirectoryAccessRule)
        {
        }

        /// <summary>
        /// Use specified identity with default access rules.
        /// </summary>
        /// <param name="identity"></param>
        public WindowsFilePermissionsStrategy(IdentityReference identity)
            : this(identity, null, null)
        {
        }

        private WindowsFilePermissionsStrategy(
            IdentityReference identity,
            FileSystemAccessRule privateFileAccessRule,
            FileSystemAccessRule privateDirectoryAccessRule)
        {
            identity ??= new SecurityIdentifier(WellKnownSidType.BuiltinGuestsSid, null).Translate(typeof(NTAccount));
            _privateFileAccessRule = privateFileAccessRule ?? new FileSystemAccessRule(identity,
                FileSystemRights.FullControl,
                InheritanceFlags.None, PropagationFlags.None,
                AccessControlType.Deny);
            _privateDirectoryAccessRule = privateDirectoryAccessRule ?? new FileSystemAccessRule(identity,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None,
                AccessControlType.Deny);
        }

        public void SetFilePermissions(string fullPath, Visibility visibility)
        {
            var fileInfo = new FileInfo(fullPath);
            var accessControl = fileInfo.GetAccessControl();

            switch (visibility)
            {
                case Visibility.Public:
                    accessControl.RemoveAccessRule(_privateFileAccessRule);
                    return;
                case Visibility.Private:
                    accessControl.AddAccessRule(_privateFileAccessRule);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
            }

            fileInfo.SetAccessControl(accessControl);
        }

        public void SetDirectoryPermissions(string fullPath, Visibility visibility)
        {
            var directoryInfo = new DirectoryInfo(fullPath);
            var accessControl = directoryInfo.GetAccessControl();

            switch (visibility)
            {
                case Visibility.Public:
                    accessControl.RemoveAccessRule(_privateDirectoryAccessRule);
                    break;
                case Visibility.Private:
                    accessControl.AddAccessRule(_privateDirectoryAccessRule);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(visibility), visibility, null);
            }

            directoryInfo.SetAccessControl(accessControl);
        }

        public Visibility GetFilePermissions(string fullPath) =>
            CheckVisibility(new FileInfo(fullPath).GetAccessControl(), _privateFileAccessRule);

        public Visibility GetDirectoryPermissions(string fullPath) =>
            CheckVisibility(new DirectoryInfo(fullPath).GetAccessControl(), _privateDirectoryAccessRule);

        private Visibility CheckVisibility(FileSystemSecurity accessControl, FileSystemAccessRule privateRule)
        {
            var isPrivate = accessControl.GetAccessRules(true, false, typeof(NTAccount))
                .OfType<FileSystemAccessRule>()
                .ToList()
                .Any(r => r.IdentityReference == privateRule.IdentityReference &&
                          r.FileSystemRights == privateRule.FileSystemRights);

            return isPrivate ? Visibility.Private : Visibility.Public;
        }
    }
}