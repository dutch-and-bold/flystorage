using System;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Adapters.Tests;

namespace DutchAndBold.Flystorage.Adapters.InMemory.Tests
{
    public class InMemoryFilesystemBaseTests : FileSystemAdapterBaseTests
    {
        protected override Func<IFilesystemAdapter> FilesystemAdapterFactory { get; } =
            () => new InMemoryFilesystemAdapter();
    }
}