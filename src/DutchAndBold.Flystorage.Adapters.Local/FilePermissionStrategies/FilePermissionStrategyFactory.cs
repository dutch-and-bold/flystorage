using System;
using DutchAndBold.Flystorage.Adapters.Local.Contracts;

namespace DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies
{
    public class FilePermissionStrategyFactory : IFilePermissionStrategyFactory
    {
        public IFilePermissionStrategy CreateForOS()
        {
            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                return new UnixFilePermissionStrategy();
            }

            if (OperatingSystem.IsWindows())
            {
                return new WindowsFilePermissionsStrategy();
            }

            throw new InvalidOperationException("Invalid operating system. Linux, MacOs and Windows are supported");
        }
    }
}