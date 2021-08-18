using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Local.Contracts;
using DutchAndBold.Flystorage.Adapters.Local.Models;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;
using HeyRed.Mime;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage.Adapters.Local
{
    public class LocalFilesystemAdapter : IFilesystemAdapter
    {
        private const Visibility DefaultVisibilityForDirectories = Abstractions.Models.Visibility.Private;

        private readonly IPathPrefixer _prefixer;

        private readonly IFilePermissionStrategy _filePermission;

        private readonly SymbolicLinkPolicy _symbolicLinkPolicy;

        public LocalFilesystemAdapter(
            IPathPrefixer prefixer,
            IFilePermissionStrategy filePermission,
            SymbolicLinkPolicy symbolicLinkPolicy = SymbolicLinkPolicy.DisallowLinks)
        {
            _prefixer = prefixer;
            _filePermission = filePermission;
            _symbolicLinkPolicy = symbolicLinkPolicy;

            EnsureDirectoryExists(string.Empty, DefaultVisibilityForDirectories);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public bool FileExists(string path)
        {
            var fullPath = _prefixer.PrefixPath(path);

            return File.Exists(fullPath);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Write(string path, string contents, Config config = null)
        {
            Write(path, new MemoryStream(Encoding.Default.GetBytes(contents)), config);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Write(string path, Stream contents, Config config = null)
        {
            var fullPath = _prefixer.PrefixPath(path);

            EnsureDirectoryExists(
                _prefixer.StripPrefix(Path.GetDirectoryName(fullPath)),
                ResolveVisibilityForDirectories(config?.Get<Visibility?>(Config.OptionDirectoryVisibility)));

            try
            {
                using var fileStream = File.Create(fullPath);
                contents.Seek(0, SeekOrigin.Begin);
                contents.CopyTo(fileStream);
                fileStream.Close();
            }
            catch (IOException e)
            {
                throw UnableToWriteFileException.AtLocation(path, e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw UnableToWriteFileException.AtLocation(path, e);
            }

            if (config?.Get<Visibility?>(Config.OptionVisibility) is { } visibility)
            {
                SetVisibility(path, visibility);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public Stream Read(string path)
        {
            var fullPath = _prefixer.PrefixPath(path);

            try
            {
                return File.OpenRead(fullPath);
            }
            catch (IOException e)
            {
                throw UnableToReadFileException.AtLocation(path, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public string ReadString(string path)
        {
            return new StreamReader(Read(path)).ReadToEnd();
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Delete(string path)
        {
            if (!FileExists(path))
            {
                return;
            }

            var fullPath = _prefixer.PrefixPath(path);

            try
            {
                File.Delete(fullPath);
            }
            catch (IOException e)
            {
                throw UnableToDeleteFileException.AtLocation(fullPath, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void DeleteDirectory(string path)
        {
            var fullPath = _prefixer.PrefixPath(path);

            try
            {
                Directory.Delete(fullPath, true);
            }
            catch (DirectoryNotFoundException)
            {
                // The Flystorage API tolerates deleting non existent directories.
            }
            catch (IOException e)
            {
                throw UnableToDeleteDirectoryException.AtLocation(fullPath, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void CreateDirectory(string path, Config config)
        {
            var visibilityConfigValue = config?.Get<Visibility?>(Config.OptionDirectoryVisibility) ??
                                        config?.Get<Visibility?>(Config.OptionVisibility);
            var visibility = ResolveVisibilityForDirectories(visibilityConfigValue);
            EnsureDirectoryExists(path, visibility);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void SetVisibility(string path, Visibility visibility)
        {
            var fullPath = _prefixer.PrefixPath(path);

            if (Directory.Exists(fullPath))
            {
                _filePermission.SetDirectoryPermissions(fullPath, visibility);
                return;
            }

            _filePermission.SetFilePermissions(fullPath, visibility);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes Visibility(string path)
        {
            var fullPath = _prefixer.PrefixPath(path);

            if (Directory.Exists(fullPath))
            {
                return new FileAttributes(path) { Visibility = _filePermission.GetDirectoryPermissions(fullPath) };
            }

            if (File.Exists(fullPath))
            {
                return new FileAttributes(path) { Visibility = _filePermission.GetFilePermissions(fullPath) };
            }

            throw UnableToRetrieveMetadataException.Visibility(path);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes MimeType(string path)
        {
            var fullPath = _prefixer.PrefixPath(path);

            var mimeType = MimeTypesMap.GetMimeType(fullPath);

            if (!FileExists(path) || string.IsNullOrEmpty(mimeType))
            {
                throw UnableToRetrieveMetadataException.MimeType(path);
            }

            return new FileAttributes(path) { MimeType = mimeType };
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes LastModified(string path)
        {
            var fullPath = _prefixer.PrefixPath(path);

            if (!FileExists(path))
            {
                throw UnableToRetrieveMetadataException.LastModified(path);
            }

            try
            {
                return new FileAttributes(path) { LastModified = File.GetLastWriteTime(fullPath) };
            }
            catch (IOException e)
            {
                throw UnableToRetrieveMetadataException.LastModified(path, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes FileSize(string path)
        {
            var fullPath = _prefixer.PrefixPath(path);

            if (!FileExists(path))
            {
                throw UnableToRetrieveMetadataException.LastModified(path);
            }

            try
            {
                return new FileAttributes(path) { FileSize = (int)new FileInfo(fullPath).Length };
            }
            catch (IOException e)
            {
                throw UnableToRetrieveMetadataException.LastModified(path, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public IEnumerable<StorageAttributes> ListContents(string path, bool deep)
        {
            var fullPath = _prefixer.PrefixPath(path);

            if (!Directory.Exists(fullPath))
            {
                return Array.Empty<StorageAttributes>();
            }

            var enumerateOptions = new EnumerationOptions { RecurseSubdirectories = deep };

            if (_symbolicLinkPolicy == SymbolicLinkPolicy.SkipLinks)
            {
                enumerateOptions.AttributesToSkip |= System.IO.FileAttributes.ReparsePoint;
            }

            var directories = Directory
                .EnumerateDirectories(fullPath)
                .Select(d => new DirectoryAttributes(_prefixer.StripPrefix(d)));

            var files = Directory
                .EnumerateFiles(fullPath, "*", enumerateOptions)
                .Select(
                    f =>
                    {
                        var fileInfo = new FileInfo(f);

                        if ((fileInfo.Attributes & System.IO.FileAttributes.ReparsePoint) != 0)
                        {
                            throw SymbolicLinkEncounteredException.AtLocation(f);
                        }

                        return new FileAttributes(_prefixer.StripPrefix(f));
                    });

            return directories.Cast<StorageAttributes>().Concat(files);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Move(string source, string destination, Config config)
        {
            var sourceFullPath = _prefixer.PrefixPath(source);
            var destinationFullPath = _prefixer.PrefixPath(destination);

            try
            {
                File.Move(sourceFullPath, destinationFullPath);
            }
            catch (IOException e)
            {
                throw UnableToMoveFileException.FromLocationTo(sourceFullPath, destinationFullPath, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Copy(string source, string destination, Config config)
        {
            var sourceFullPath = _prefixer.PrefixPath(source);
            var destinationFullPath = _prefixer.PrefixPath(destination);

            try
            {
                File.Copy(sourceFullPath, destinationFullPath);
            }
            catch (IOException e)
            {
                throw UnableToCopyFileException.FromLocationTo(sourceFullPath, destinationFullPath, e);
            }
        }

        private void EnsureDirectoryExists(string path, Visibility visibility)
        {
            var fullPath = _prefixer.PrefixPath(path);

            void SetPermissions() => _filePermission.SetDirectoryPermissions(fullPath, visibility);

            if (Directory.Exists(fullPath))
            {
                SetPermissions();
                return;
            }

            try
            {
                Directory.CreateDirectory(fullPath);
            }
            catch (IOException e)
            {
                throw UnableToCreateDirectoryException.AtLocation(path, e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw UnableToCreateDirectoryException.AtLocation(path, e);
            }

            SetPermissions();
        }

        private Visibility ResolveVisibilityForDirectories(Visibility? visibility)
        {
            return visibility ?? DefaultVisibilityForDirectories;
        }
    }
}