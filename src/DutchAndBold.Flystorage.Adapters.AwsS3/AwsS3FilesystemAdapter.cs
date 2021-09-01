using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;
using MimeTypes;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage.Adapters.AwsS3
{
    public class AwsS3FilesystemAdapter : IFilesystemAdapterAsync
    {
        private readonly IAmazonS3 _client;

        private readonly IPathPrefixer _pathPrefixer;

        private readonly IAclVisibilityConverter _visibilityConverter;

        private readonly string _bucketName;

        public AwsS3FilesystemAdapter(
            string bucketName,
            IAmazonS3 client,
            IPathPrefixer pathPrefixer,
            IAclVisibilityConverter visibilityConverter)
        {
            _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _pathPrefixer = pathPrefixer ?? throw new ArgumentNullException(nameof(pathPrefixer));
            _visibilityConverter = visibilityConverter;
        }

        public async Task<bool> FileExists(string path, CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixPath(path);
            try
            {
                await _client.GetObjectMetadataAsync(_bucketName, key, cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return false;
                }

                throw UnableToCheckFileExistenceException.AtLocation(path, e);
            }

            return true;
        }

        public async Task Write(
            string path,
            Stream contents,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixPath(path);
            var awsOptions = AwsOptions.Create(config ?? new Config());
            awsOptions.CannedACL ??= GetAcl(config);
            var shouldDetermineMimeType = contents.Length > 0 && awsOptions.ContentType == null;

            if (shouldDetermineMimeType && MimeTypeMap.TryGetMimeType(path, out var mimeType))
            {
                awsOptions.ContentType = mimeType;
            }

            try
            {
                await _client.UploadObjectFromStreamAsync(
                    _bucketName,
                    key,
                    contents,
                    awsOptions,
                    cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                throw UnableToWriteFileException.AtLocation(path, e);
            }
        }

        public async Task<Stream> Read(string path, CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixPath(path);
            try
            {
                var stream = await _client.GetObjectStreamAsync(_bucketName, key, null, cancellationToken);
                return AmazonS3Util.MakeStreamSeekable(stream);
            }
            catch (AmazonS3Exception e)
            {
                throw UnableToReadFileException.AtLocation(path, e);
            }
        }

        public async Task Delete(string path, CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixPath(path);

            try
            {
                await _client.DeleteObjectAsync(_bucketName, key, cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                throw UnableToDeleteFileException.AtLocation(path, e);
            }
        }

        public async Task DeleteDirectory(string path, CancellationToken cancellationToken = default)
        {
            var contents = ListContents(path, true, cancellationToken);
            var keys = new List<string>();

            Func<List<string>, Task> deleteKeys = async keysToDelete =>
            {
                await _client.DeleteObjectsAsync(
                    new DeleteObjectsRequest()
                    {
                        BucketName = _bucketName,
                        Objects = keysToDelete.Select(s => new KeyVersion() { Key = s }).ToList()
                    },
                    cancellationToken);

                keysToDelete.Clear();
            };

            await foreach (var content in contents.WithCancellation(cancellationToken))
            {
                keys.Add(content.Path);

                if (keys.Count != 1000) continue;

                await deleteKeys.Invoke(keys);
            }

            if (keys.Count > 0)
            {
                await deleteKeys.Invoke(keys);
            }
        }

        public async Task CreateDirectory(
            string path,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixDirectoryPath(path);
            var configDefaults = new Dictionary<string, object>()
            {
                { Config.OptionVisibility, _visibilityConverter.DefaultForDirectories() }
            };

            try
            {
                await _client.PutObjectAsync(
                    new PutObjectRequest()
                    {
                        BucketName = _bucketName,
                        Key = key
                    },
                    cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                throw UnableToCreateDirectoryException.AtLocation(path, e);
            }
        }

        public async Task SetVisibility(
            string path,
            Visibility visibility,
            CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixPath(path);

            try
            {
                await _client.PutACLAsync(
                    new PutACLRequest()
                    {
                        BucketName = _bucketName,
                        Key = key,
                        CannedACL = _visibilityConverter.VisibilityToAcl(visibility)
                    },
                    cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                throw UnableToSetVisibilityException.AtLocation(path, e);
            }
        }

        public async Task<FileAttributes> Visibility(string path, CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixPath(path);

            try
            {
                var acl = await _client.GetACLAsync(
                    new GetACLRequest()
                    {
                        BucketName = _bucketName,
                        Key = key
                    },
                    cancellationToken);

                return new FileAttributes(path)
                {
                    Visibility = _visibilityConverter.AclToVisibility(acl.AccessControlList.Grants)
                };
            }
            catch (AmazonS3Exception e)
            {
                throw UnableToRetrieveMetadataException.Visibility(path, e);
            }
        }

        public async Task<FileAttributes> MimeType(string path, CancellationToken cancellationToken = default)
        {
            const string defaultUnknownMimeType = "application/octet-stream";
            var fileAttributes = await GetFileMetaData(path, StorageAttributes.AttributeMimeType, cancellationToken);

            if (fileAttributes.MimeType == defaultUnknownMimeType && !MimeTypeMap.TryGetMimeType(path, out _))
            {
                throw UnableToRetrieveMetadataException.MimeType(path, new Exception("MimeType can not be determined."));
            }

            return fileAttributes;
        }

        public Task<FileAttributes> LastModified(string path, CancellationToken cancellationToken = default)
        {
            return GetFileMetaData(path, StorageAttributes.AttributeLastModified, cancellationToken);
        }

        public Task<FileAttributes> FileSize(string path, CancellationToken cancellationToken = default)
        {
            return GetFileMetaData(path, StorageAttributes.AttributeFileSize, cancellationToken);
        }

        public IAsyncEnumerable<StorageAttributes> ListContents(
            string path,
            bool deep,
            CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixDirectoryPath(path);
            var prefix = key == "" | key == "/" ? null : key;

            var paginator = _client.Paginators.ListObjectsV2(
                new ListObjectsV2Request()
                {
                    BucketName = _bucketName,
                    Delimiter = deep ? null : "/",
                    Prefix = prefix
                });

            return paginator.Responses
                .AsAsyncEnumerable()
                .SelectMany(
                    r =>
                    {
                        var list = new List<StorageAttributes>()
                            .Concat(
                                r.S3Objects.Select<S3Object, StorageAttributes>(
                                    o =>
                                    {
                                        var keyWithoutPrefix = _pathPrefixer.StripPrefix(o.Key);

                                        if (keyWithoutPrefix.EndsWith("/"))
                                        {
                                            return new DirectoryAttributes(keyWithoutPrefix)
                                            {
                                                LastModified = o.LastModified
                                            };
                                        }

                                        return new FileAttributes(keyWithoutPrefix)
                                        {
                                            FileSize = o.Size,
                                            LastModified = o.LastModified
                                        };
                                    }))
                            .Concat(
                                r.CommonPrefixes.Select(s => new DirectoryAttributes(_pathPrefixer.StripPrefix(s).TrimEnd('/'))));

                        return list.ToAsyncEnumerable();
                    });
        }

        public async Task Move(
            string source,
            string destination,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await Copy(source, destination, config, cancellationToken);
                await Delete(source, cancellationToken);
            }
            catch (FileSystemOperationFailedException e)
            {
                throw UnableToMoveFileException.FromLocationTo(source, destination, e);
            }
        }

        public async Task Copy(
            string source,
            string destination,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            var acl = _visibilityConverter.VisibilityToAcl(
                config?.Get<Visibility>(Config.OptionVisibility) ?? Abstractions.Models.Visibility.Public);
            var awsOptions = AwsOptions.Create(config ?? new Config());
            var request = awsOptions.CreateNew<CopyObjectRequest>();
            request.SourceBucket = request.DestinationBucket = _bucketName;
            request.SourceKey = _pathPrefixer.PrefixPath(source);
            request.DestinationKey = _pathPrefixer.PrefixPath(destination);
            request.CannedACL = awsOptions.CannedACL ?? acl;

            try
            {
                await _client.CopyObjectAsync(request, cancellationToken);
            }
            catch (AmazonS3Exception e)
            {
                throw UnableToCopyFileException.FromLocationTo(source, destination, e);
            }
        }

        private async Task<FileAttributes> GetFileMetaData(
            string path,
            string type,
            CancellationToken cancellationToken = default)
        {
            var key = _pathPrefixer.PrefixPath(path);

            try
            {
                var metadata = await _client.GetObjectMetadataAsync(_bucketName, key, cancellationToken);

                return new FileAttributes(path)
                {
                    LastModified = metadata.LastModified,
                    MimeType = metadata.Headers.ContentType,
                    FileSize = metadata.ContentLength
                };
            }
            catch (AmazonS3Exception e)
            {
                throw new UnableToRetrieveMetadataException(path, type, e);
            }
        }

        private S3CannedACL GetAcl(Config config)
        {
            var visibility = config.Get(Config.OptionVisibility, Abstractions.Models.Visibility.Private);

            return _visibilityConverter.VisibilityToAcl(visibility);
        }
    }
}