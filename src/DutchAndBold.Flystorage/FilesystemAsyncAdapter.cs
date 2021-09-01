using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Models;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage
{
    public class FilesystemAsyncAdapter : IFilesystemAdapterAsync
    {
        private readonly IFilesystemAdapter _filesystemAdapter;

        public FilesystemAsyncAdapter(IFilesystemAdapter filesystemAdapter)
        {
            _filesystemAdapter = filesystemAdapter;
        }

        public Task<bool> FileExists(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filesystemAdapter.FileExists(path));
        }

        public Task Write(
            string path,
            Stream contents,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            _filesystemAdapter.Write(path, contents, config);
            return Task.CompletedTask;
        }

        public Task<Stream> Read(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filesystemAdapter.Read(path));
        }

        public Task Delete(string path, CancellationToken cancellationToken = default)
        {
            _filesystemAdapter.Delete(path);
            return Task.CompletedTask;
        }

        public Task DeleteDirectory(string path, CancellationToken cancellationToken = default)
        {
            _filesystemAdapter.DeleteDirectory(path);
            return Task.CompletedTask;
        }

        public Task CreateDirectory(string path, Config config = null, CancellationToken cancellationToken = default)
        {
            _filesystemAdapter.CreateDirectory(path, config);
            return Task.CompletedTask;
        }

        public Task SetVisibility(string path, Visibility visibility, CancellationToken cancellationToken = default)
        {
            _filesystemAdapter.SetVisibility(path, visibility);
            return Task.CompletedTask;
        }

        public Task<FileAttributes> Visibility(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filesystemAdapter.Visibility(path));
        }

        public Task<FileAttributes> MimeType(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filesystemAdapter.MimeType(path));
        }

        public Task<FileAttributes> LastModified(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filesystemAdapter.LastModified(path));
        }

        public Task<FileAttributes> FileSize(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_filesystemAdapter.FileSize(path));
        }

        public async IAsyncEnumerable<StorageAttributes> ListContents(
            string path,
            bool deep,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach(var item in _filesystemAdapter.ListContents(path, deep))
            {
                yield return await Task.FromResult(item);
            }
        }

        public Task Move(
            string source,
            string destination,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            _filesystemAdapter.Move(source, destination, config);
            return Task.CompletedTask;
        }

        public Task Copy(
            string source,
            string destination,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            _filesystemAdapter.Copy(source, destination, config);
            return Task.CompletedTask;
        }
    }
}