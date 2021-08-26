using System;
using DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies;
using FluentAssertions;
using Xunit;

namespace DutchAndBold.Flystorage.Adapters.Local.Tests
{
    public class FilePermissionStrategyFactoryTests
    {
        [SkippableFact]
        public void CreateForOS_OnMacOsOrLinux_ReturnsUnixFilePermissionStrategy()
        {
            Skip.IfNot(OperatingSystem.IsMacOS() || OperatingSystem.IsLinux());

            // Arrange
            var factory = new FilePermissionStrategyFactory();

            // Act
            var strategy = factory.CreateForOS();

            // Assert
            strategy.Should().BeOfType<UnixFilePermissionStrategy>();
        }

        [SkippableFact]
        public void CreateForOS_OnWindows_ReturnsUnixFilePermissionStrategy()
        {
            Skip.IfNot(OperatingSystem.IsWindows());

            // Arrange
            var factory = new FilePermissionStrategyFactory();

            // Act
            var strategy = factory.CreateForOS();

            // Assert
            strategy.Should().BeOfType<WindowsFilePermissionsStrategy>();
        }
    }
}