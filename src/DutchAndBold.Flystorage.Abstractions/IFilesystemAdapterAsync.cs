using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage.Abstractions
{
    public interface IFilesystemAdapterAsync
    {
        /// <summary>
        /// Checks for existence of the file at the given path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        /// <returns>Whether the file exists or not.</returns>
        public Task<bool> FileExists(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Writes a stream to the given location.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="contents">The stream to write to the file.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToWriteFileException"></exception>
        public Task Write(
            string path,
            Stream contents,
            Config config = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Reads the file at path as Stream.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToReadFileException"></exception>
        /// <returns>File as stream.</returns>
        public Task<Stream> Read(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the file at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToDeleteFileException"></exception>
        public Task Delete(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes the directory at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToDeleteDirectoryException"></exception>
        public Task DeleteDirectory(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates the directory at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToCreateDirectoryException"></exception>
        public Task CreateDirectory(string path, Config config = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Set's the visibility of directory or file at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <param name="visibility">The visibility value.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="InvalidVisibilityProvidedException"></exception>
        /// <exception cref="UnableToSetVisibilityException"></exception>
        public Task SetVisibility(string path, Visibility visibility, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get's the visibility of directory or file at path.
        /// </summary>
        /// <param name="path">The location of the directory or file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public Task<FileAttributes> Visibility(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get's the mime type file at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public Task<FileAttributes> MimeType(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get's the last modified at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public Task<FileAttributes> LastModified(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get's the file size of the file at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public Task<FileAttributes> FileSize(string path, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get's the contents of a directory at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <param name="deep">Include sub directories.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        public IAsyncEnumerable<StorageAttributes> ListContents(
            string path,
            bool deep,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Moves the file at source to the destination path.
        /// </summary>
        /// <param name="source">The location of the file.</param>
        /// <param name="destination">The location destination directory.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        public Task Move(
            string source,
            string destination,
            Config config = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Copies the file at source to the destination path.
        /// </summary>
        /// <param name="source">The location of the file.</param>
        /// <param name="destination">The location destination directory.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        public Task Copy(
            string source,
            string destination,
            Config config = null,
            CancellationToken cancellationToken = default);
    }
}