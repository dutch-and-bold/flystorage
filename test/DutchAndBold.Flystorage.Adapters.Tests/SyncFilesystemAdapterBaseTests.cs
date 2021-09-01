using System;
using DutchAndBold.Flystorage.Abstractions;

namespace DutchAndBold.Flystorage.Adapters.Tests
{
    public abstract class SyncFilesystemAdapterBaseTests : FileSystemAdapterBaseTests
    {
        protected abstract Func<IFilesystemAdapter> SyncFilesystemAdapterFactory { get; }

        protected override Func<IFilesystemAdapterAsync> FilesystemAdapterFactory => () =>
            new FilesystemAsyncAdapter(SyncFilesystemAdapterFactory.Invoke());
    }
}