using System;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies;
using DutchAndBold.Flystorage.Adapters.Shared;
using DutchAndBold.Flystorage.Adapters.Tests;
using Xunit.Abstractions;

namespace DutchAndBold.Flystorage.Adapters.Local.Tests
{
    public class LocalFilesystemBaseTests : SyncFilesystemAdapterBaseTests
    {
        protected override Func<IFilesystemAdapter> SyncFilesystemAdapterFactory { get; }

        public LocalFilesystemBaseTests(ITestOutputHelper testOutputHelper)
        {
            SyncFilesystemAdapterFactory = () => new LocalFilesystemAdapter(
                new PathPrefixer($"{Environment.CurrentDirectory}/.test/{testOutputHelper.GetTestName()}"),
                new FilePermissionStrategyFactory().CreateForOS());
        }
    }
}