using System;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies;
using DutchAndBold.Flystorage.Adapters.Shared;
using DutchAndBold.Flystorage.Adapters.Tests;
using Xunit.Abstractions;

namespace DutchAndBold.Flystorage.Adapters.Local.Tests
{
    public class LocalFilesystemBaseTests : FileSystemAdapterBaseTests
    {
        protected override Func<IFilesystemAdapter> FilesystemAdapterFactory { get; }

        public LocalFilesystemBaseTests(ITestOutputHelper testOutputHelper)
        {
            FilesystemAdapterFactory = () => new LocalFilesystemAdapter(
                new PathPrefixer($"{Environment.CurrentDirectory}/.test/{testOutputHelper.GetTestName()}"),
                new FilePermissionStrategyFactory().CreateForOS());
        }
    }
}