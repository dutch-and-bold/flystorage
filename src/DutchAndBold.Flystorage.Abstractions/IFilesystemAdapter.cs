using System.Collections.Generic;
using System.IO;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage.Abstractions
{
    public interface IFilesystemAdapter
    {
        /// <summary>
        /// Checks for existence of the file at the given path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        /// <returns>Whether the file exists or not.</returns>
        public bool FileExists(string path);

        /// <summary>
        /// Writes a string value to the given location.
        /// Warning: Only use when using a Stream is not possible.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToWriteFileException"></exception>
        public void Write(string path, string contents, Config config = null);

        /// <summary>
        /// Writes a stream to the given location.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <param name="contents">The stream to write to the file.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToWriteFileException"></exception>
        public void Write(string path, Stream contents, Config config = null);

        /// <summary>
        /// Reads the file at path as Stream.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToReadFileException"></exception>
        /// <returns>File as stream.</returns>
        public Stream Read(string path);

        /// <summary>
        /// Reads the file at path as String.
        /// Because of it's memory inefficient nature, always use <see cref="Read(string)"/> unless you need a string.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToReadFileException"></exception>
        /// <returns>File as stream.</returns>
        public string ReadString(string path);

        /// <summary>
        /// Deletes the file at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToDeleteFileException"></exception>
        public void Delete(string path);

        /// <summary>
        /// Deletes the directory at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToDeleteDirectoryException"></exception>
        public void DeleteDirectory(string path);

        /// <summary>
        /// Creates the directory at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToCreateDirectoryException"></exception>
        public void CreateDirectory(string path, Config config);

        /// <summary>
        /// Set's the visibility of directory or file at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <param name="visibility">The visibility value.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="InvalidVisibilityProvidedException"></exception>
        /// <exception cref="UnableToSetVisibilityException"></exception>
        public void SetVisibility(string path, Visibility visibility);

        /// <summary>
        /// Get's the visibility of directory or file at path.
        /// </summary>
        /// <param name="path">The location of the directory or file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public FileAttributes Visibility(string path);

        /// <summary>
        /// Get's the mime type file at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public FileAttributes MimeType(string path);

        /// <summary>
        /// Get's the last modified at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public FileAttributes LastModified(string path);

        /// <summary>
        /// Get's the file size of the file at path.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToRetrieveMetadataException"></exception>
        public FileAttributes FileSize(string path);

        /// <summary>
        /// Get's the contents of a directory at path.
        /// </summary>
        /// <param name="path">The location of the directory.</param>
        /// <param name="deep">Include sub directories.</param>
        /// <exception cref="FilesystemException"></exception>
        public IEnumerable<StorageAttributes> ListContents(string path, bool deep);

        /// <summary>
        /// Moves the file at source to the destination path.
        /// </summary>
        /// <param name="source">The location of the file.</param>
        /// <param name="destination">The location destination directory.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <exception cref="FilesystemException"></exception>
        public void Move(string source, string destination, Config config);

        /// <summary>
        /// Copies the file at source to the destination path.
        /// </summary>
        /// <param name="source">The location of the file.</param>
        /// <param name="destination">The location destination directory.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <exception cref="FilesystemException"></exception>
        public void Copy(string source, string destination, Config config);
    }
}