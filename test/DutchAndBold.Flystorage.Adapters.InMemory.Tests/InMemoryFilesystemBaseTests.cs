using System;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Adapters.Shared;
using DutchAndBold.Flystorage.Adapters.Tests;

namespace DutchAndBold.Flystorage.Adapters.InMemory.Tests
{
    public class InMemoryFilesystemBaseTests : SyncFilesystemAdapterBaseTests
    {
        protected override Func<IFilesystemAdapter> SyncFilesystemAdapterFactory { get; } =
            () => new InMemoryFilesystemAdapter(new DirectorySplitter());
    }
}