using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.InMemory.Exceptions;
using MimeTypes;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage.Adapters.InMemory
{
    public class InMemoryFilesystemAdapter : IFilesystemAdapter
    {
        private readonly Dictionary<string, InMemoryFile> _files;

        private const char Root = DirectorySeparator;

        private const char DirectorySeparator = '/';

        public InMemoryFilesystemAdapter(Dictionary<string, InMemoryFile> files = null)
        {
            _files = files ?? new Dictionary<string, InMemoryFile>();
        }

        public bool FileExists(string path)
        {
            return _files.ContainsKey(GetInMemoryLocation(path));
        }

        public void Write(string path, Stream contents, Config config = null)
        {
            _files[GetInMemoryLocation(path)] = new InMemoryFile(contents)
            {
                Visibility = config?.Get<Visibility?>(Config.OptionVisibility) ??
                             Abstractions.Models.Visibility.Public
            };
        }

        public Stream Read(string path)
        {
            try
            {
                EnsureFileExists(path, out var file);
                return file.Contents;
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToReadFileException.AtLocation(path, e);
            }
        }

        public void Delete(string path)
        {
            _files.Remove(GetInMemoryLocation(path));
        }

        public void DeleteDirectory(string path)
        {
            foreach (var keyToRemove in _files.Keys.Where(k => k.StartsWith(GetInMemoryDirectoryLocation(path))))
            {
                _files.Remove(keyToRemove);
            }
        }

        public void CreateDirectory(string path, Config config = null)
        {
            var location = GetInMemoryLocation(path) + DirectorySeparator + ".";
            _files.Add(location, new InMemoryDirectoryPlaceholder());
        }

        public void SetVisibility(string path, Visibility visibility)
        {
            try
            {
                EnsureFileExists(path, out var file);
                var location = GetInMemoryLocation(path);
                _files.Remove(location);
                _files.Add(location, file.Copy(visibility));
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToSetVisibilityException.AtLocation(path, e);
            }
        }

        public FileAttributes Visibility(string path)
        {
            try
            {
                EnsureFileExists(path, out var file);
                return new FileAttributes(path) { Visibility = file.Visibility };
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToRetrieveMetadataException.MimeType(path, e);
            }
        }

        public FileAttributes MimeType(string path)
        {
            try
            {
                EnsureFileExists(path);
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToRetrieveMetadataException.MimeType(path, e);
            }

            if (!MimeTypeMap.TryGetMimeType(path, out var mimeType))
            {
                throw UnableToRetrieveMetadataException.MimeType(
                    path,
                    new Exception("Mime type could not be identified."));
            }

            return new FileAttributes(path) { MimeType = mimeType };
        }

        public FileAttributes LastModified(string path)
        {
            try
            {
                EnsureFileExists(path, out var file);
                return new FileAttributes(path) { LastModified = file.LastModified };
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToRetrieveMetadataException.MimeType(path, e);
            }
        }

        public FileAttributes FileSize(string path)
        {
            try
            {
                EnsureFileExists(path, out var file);
                return new FileAttributes(path) { FileSize = file.Contents.Length };
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToRetrieveMetadataException.MimeType(path, e);
            }
        }

        public IEnumerable<StorageAttributes> ListContents(string path, bool deep)
        {
            var location = GetInMemoryLocation(path);

            bool IsPathRequested(string p)
            {
                var isContainedWithinLocation = p.StartsWith(location);
                var isDeepPath = p[location.Length..]
                    .TrimStart(DirectorySeparator)
                    .Contains(DirectorySeparator);

                return isContainedWithinLocation && (deep || !isDeepPath);
            }

            IEnumerable<StorageAttributes> files = _files
                .Where(f => IsPathRequested(f.Key))
                .Where(f => f.Value.GetType() != typeof(InMemoryDirectoryPlaceholder))
                .Select(f => new FileAttributes(f.Key.TrimStart(Root)))
                .ToList();

            var greatestPath = _files
                .Keys
                .OrderByDescending(p => p.Length)
                .FirstOrDefault();

            var directories = WalkStringPath(greatestPath, location)
                .Where(IsPathRequested)
                .Select(d => new DirectoryAttributes(d.TrimStart(Root)))
                .ToList();

            return files.Concat(directories);
        }

        public void Move(string source, string destination, Config config = null)
        {
            var sourceLocation = GetInMemoryLocation(source);
            var destinationLocation = GetInMemoryLocation(destination);

            if (_files.ContainsKey(destinationLocation))
            {
                throw UnableToMoveFileException.FromLocationTo(
                    source,
                    destination,
                    new Exception($"Destination [{destination}] already exists"));
            }

            try
            {
                EnsureFileExists(source, out var file);
                _files.Remove(sourceLocation);
                _files.Add(destinationLocation, file);
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToMoveFileException.FromLocationTo(source, destination, e);
            }
        }

        public void Copy(string source, string destination, Config config = null)
        {
            var destinationLocation = GetInMemoryLocation(destination);

            try
            {
                EnsureFileExists(source, out var file);

                if (_files.ContainsKey(destinationLocation))
                {
                    _files.Remove(destinationLocation);
                }

                _files.Add(destinationLocation, file.Copy());
            }
            catch (InMemoryFileDoesNotExistException e)
            {
                throw UnableToCopyFileException.FromLocationTo(source, destination, e);
            }
        }

        private void EnsureFileExists(string path, out InMemoryFile inMemoryFile)
        {
            if (_files.TryGetValue(GetInMemoryLocation(path), out inMemoryFile))
            {
                return;
            }

            throw new InMemoryFileDoesNotExistException();
        }

        private void EnsureFileExists(string path)
        {
            if (_files.ContainsKey(GetInMemoryLocation(path)))
            {
                return;
            }

            throw new InMemoryFileDoesNotExistException();
        }

        private static string GetInMemoryLocation(string path) => Root + path.TrimStart(Root);


        private static string GetInMemoryDirectoryLocation(string path) =>
            GetInMemoryLocation(path).TrimEnd(DirectorySeparator) +
            DirectorySeparator;

        private static string SubstractDirectoryPath(string filePath) => Path.GetDirectoryName(filePath);

        private List<string> WalkStringPath(string path, string stopAtPath = null)
        {
            var directories = new List<string>();

            while (!string.IsNullOrEmpty(path))
            {
                path = path[..path.LastIndexOf('/')];
                if (stopAtPath != null && path == stopAtPath.TrimEnd(DirectorySeparator))
                {
                    break;
                }

                directories.Add(path);
            }

            return directories;
        }
    }
}