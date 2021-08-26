using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Local.Contracts;
using DutchAndBold.Flystorage.Adapters.Local.Models;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;
using MimeTypes;
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

            EnsureDirectoryExists(_prefixer.PrefixPath(string.Empty), DefaultVisibilityForDirectories);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public bool FileExists(string path)
        {
            var location = _prefixer.PrefixPath(path);

            return File.Exists(location);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Write(string path, Stream contents, Config config = null)
        {
            var location = _prefixer.PrefixPath(path);

            EnsureDirectoryExists(
                Path.GetDirectoryName(location),
                ResolveVisibilityForDirectories(config?.Get<Visibility?>(Config.OptionDirectoryVisibility)));

            try
            {
                using var fileStream = File.Create(location);
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
            var location = _prefixer.PrefixPath(path);

            try
            {
                return File.OpenRead(location);
            }
            catch (IOException e)
            {
                throw UnableToReadFileException.AtLocation(path, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Delete(string path)
        {
            if (!FileExists(path))
            {
                return;
            }

            var location = _prefixer.PrefixPath(path);

            try
            {
                File.Delete(location);
            }
            catch (IOException e)
            {
                throw UnableToDeleteFileException.AtLocation(location, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void DeleteDirectory(string path)
        {
            var location = _prefixer.PrefixPath(path);

            try
            {
                Directory.Delete(location, true);
            }
            catch (DirectoryNotFoundException)
            {
                // The Flystorage API tolerates deleting non existent directories.
            }
            catch (IOException e)
            {
                throw UnableToDeleteDirectoryException.AtLocation(location, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void CreateDirectory(string path, Config config = null)
        {
            var location = _prefixer.PrefixPath(path);
            var visibilityConfigValue = config?.Get<Visibility?>(Config.OptionDirectoryVisibility) ??
                                        config?.Get<Visibility?>(Config.OptionVisibility);
            var visibility = ResolveVisibilityForDirectories(visibilityConfigValue);
            EnsureDirectoryExists(location, visibility);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void SetVisibility(string path, Visibility visibility)
        {
            var location = _prefixer.PrefixPath(path);

            try
            {
                if (Directory.Exists(location))
                {
                    _filePermission.SetDirectoryPermissions(location, visibility);
                    return;
                }

                _filePermission.SetFilePermissions(location, visibility);
            }
            catch (IOException e)
            {
                throw UnableToSetVisibilityException.AtLocation(path, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes Visibility(string path)
        {
            var location = _prefixer.PrefixPath(path);

            if (Directory.Exists(location))
            {
                return new FileAttributes(path) { Visibility = _filePermission.GetDirectoryPermissions(location) };
            }

            if (File.Exists(location))
            {
                return new FileAttributes(path) { Visibility = _filePermission.GetFilePermissions(location) };
            }

            throw UnableToRetrieveMetadataException.Visibility(path);
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes MimeType(string path)
        {
            var location = _prefixer.PrefixPath(path);

            if (!FileExists(path) || !MimeTypeMap.TryGetMimeType(path, out var mimeType))
            {
                throw UnableToRetrieveMetadataException.MimeType(path);
            }

            return new FileAttributes(path) { MimeType = mimeType };
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes LastModified(string path)
        {
            var location = _prefixer.PrefixPath(path);

            if (!FileExists(path))
            {
                throw UnableToRetrieveMetadataException.LastModified(path);
            }

            try
            {
                return new FileAttributes(path) { LastModified = File.GetLastWriteTime(location) };
            }
            catch (IOException e)
            {
                throw UnableToRetrieveMetadataException.LastModified(path, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public FileAttributes FileSize(string path)
        {
            var location = _prefixer.PrefixPath(path);

            if (!FileExists(path))
            {
                throw UnableToRetrieveMetadataException.LastModified(path);
            }

            try
            {
                return new FileAttributes(path) { FileSize = new FileInfo(location).Length };
            }
            catch (IOException e)
            {
                throw UnableToRetrieveMetadataException.LastModified(path, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public IEnumerable<StorageAttributes> ListContents(string path, bool deep)
        {
            var location = _prefixer.PrefixPath(path);

            if (!Directory.Exists(location))
            {
                return Array.Empty<StorageAttributes>();
            }

            var enumerateOptions = new EnumerationOptions { RecurseSubdirectories = deep };

            if (_symbolicLinkPolicy == SymbolicLinkPolicy.SkipLinks)
            {
                enumerateOptions.AttributesToSkip |= System.IO.FileAttributes.ReparsePoint;
            }

            var directories = Directory
                .EnumerateDirectories(location)
                .Select(d => new DirectoryAttributes(_prefixer.StripPrefix(d)));

            var files = Directory
                .EnumerateFiles(location, "*", enumerateOptions)
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
        public void Move(string source, string destination, Config config = null)
        {
            var sourceLocation = _prefixer.PrefixPath(source);
            var destinationLocation = _prefixer.PrefixPath(destination);

            try
            {
                if (FileExists(destination))
                {
                    Delete(destination);
                }
                File.Move(sourceLocation, destinationLocation);
            }
            catch (IOException e)
            {
                throw UnableToMoveFileException.FromLocationTo(sourceLocation, destinationLocation, e);
            }
        }

        /// <inheritdoc cref="IFilesystemAdapter"/>
        public void Copy(string source, string destination, Config config = null)
        {
            var sourceLocation = _prefixer.PrefixPath(source);
            var destinationLocation = _prefixer.PrefixPath(destination);

            try
            {
                if (FileExists(destination))
                {
                    Delete(destination);
                }

                File.Copy(sourceLocation, destinationLocation);
            }
            catch (IOException e)
            {
                throw UnableToCopyFileException.FromLocationTo(source, destination, e);
            }
        }

        private void EnsureDirectoryExists(string location, Visibility visibility)
        {
            void SetPermissions() => _filePermission.SetDirectoryPermissions(location, visibility);

            if (Directory.Exists(location))
            {
                SetPermissions();
                return;
            }

            try
            {
                Directory.CreateDirectory(location);
            }
            catch (IOException e)
            {
                throw UnableToCreateDirectoryException.AtLocation(_prefixer.StripPrefix(location), e);
            }
            catch (UnauthorizedAccessException e)
            {
                throw UnableToCreateDirectoryException.AtLocation(_prefixer.StripPrefix(location), e);
            }

            SetPermissions();
        }

        private Visibility ResolveVisibilityForDirectories(Visibility? visibility)
        {
            return visibility ?? DefaultVisibilityForDirectories;
        }
    }
}