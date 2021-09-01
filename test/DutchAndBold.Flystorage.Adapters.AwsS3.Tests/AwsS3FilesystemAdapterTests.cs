using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.AwsS3.Tests.Fixtures;
using DutchAndBold.Flystorage.Adapters.Shared;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;
using DutchAndBold.Flystorage.Adapters.Tests;
using FluentAssertions;
using Moq;
using Xunit;
using Xunit.Abstractions;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage.Adapters.AwsS3.Tests
{
    [Collection(nameof(AmazonS3ClientFixtureCollection))]
    public class AwsS3FilesystemAdapterTests
    {
        private readonly string _bucketName;

        private readonly IAmazonS3 _amazonS3Client;

        private readonly IPathPrefixer _pathPrefixer;

        public AwsS3FilesystemAdapterTests(
            ITestOutputHelper testOutputHelper,
            AmazonS3ClientFixture amazonS3ClientFixture)
        {
            _bucketName = amazonS3ClientFixture.BucketName;
            _amazonS3Client = amazonS3ClientFixture.Client;
            _pathPrefixer = new PathPrefixer($"{Environment.ProcessId}/{nameof(AwsS3FilesystemBaseTests)}/" + testOutputHelper.GetTestName());
        }

        [Fact]
        public async Task writing_with_a_specific_mime_type()
        {
            // Arrange
            var adapter = CreateAdapter();

            // Act
            await adapter.Write(
                "path.txt",
                new MemoryStream(),
                new Config(new Dictionary<string, object>() { { "ContentType", "text/plain+special" } }));

            // Assert
            (await adapter.MimeType("path.txt")).MimeType.Should().Be("text/plain+special");
        }

        [Fact]
        public async Task listing_contents_recursive()
        {
            // Arrange
            var adapter = CreateAdapter();
            var (firstPath, _) = await GivenWeHaveAnExistingFile("something/0/here.txt");
            var (secondPath, _) = await GivenWeHaveAnExistingFile("something/1/also/here.txt");

            // Act
            var contents = await adapter.ListContents("", true).ToListAsync();

            // Assert
            contents.Should().HaveCount(2);
            contents.Should().AllBeOfType<FileAttributes>();
            contents[0].Path.Should().Be(firstPath);
            contents[1].Path.Should().Be(secondPath);
        }

        [Fact]
        public async Task failing_to_delete_while_moving()
        {
            // Arrange
            var (path, _) = await GivenWeHaveAnExistingFile();

            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(m => m.CopyObjectAsync(It.IsAny<CopyObjectRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Test"));

            var adapter = CreateAdapter(s3ClientMock.Object);

            // Act
            Func<Task> action = () => adapter.Move(path, "destination.txt", new Config());

            // Assert
            await action.Should().ThrowAsync<UnableToMoveFileException>();
        }

        [Fact]
        public async Task failing_to_write_a_file()
        {
            // Arrange
            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(
                    m => m.UploadObjectFromStreamAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<Stream>(),
                        It.IsAny<Dictionary<string, object>>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Oh no"));

            var adapter = CreateAdapter(s3ClientMock.Object);

            // Act
            Func<Task> action = () => adapter.Write("path.txt", Stream.Null, new Config());

            // Assert
            await action.Should().ThrowAsync<UnableToWriteFileException>();
        }

        [Fact]
        public async Task failing_to_delete_a_file()
        {
            // Arrange
            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(
                    m => m.DeleteObjectAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Oh no"));

            var adapter = CreateAdapter(s3ClientMock.Object);

            // Act
            Func<Task> action = () => adapter.Delete("path.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToDeleteFileException>();
        }

        [Fact]
        public async Task failing_to_retrieve_metadata()
        {
            // Arrange
            var adapter = CreateAdapter();

            // Act
            Func<Task> getMimeType = () => adapter.MimeType("filename.txt");
            Func<Task> getLastModified = () => adapter.LastModified("filename.txt");
            Func<Task> getFileSize = () => adapter.FileSize("filename.txt");

            // Assert
            (await getMimeType.Should().ThrowAsync<UnableToRetrieveMetadataException>()).And.MetadataType.Should()
                .Be(StorageAttributes.AttributeMimeType);

            (await getLastModified.Should().ThrowAsync<UnableToRetrieveMetadataException>()).And.MetadataType.Should()
                .Be(StorageAttributes.AttributeLastModified);

            (await getFileSize.Should().ThrowAsync<UnableToRetrieveMetadataException>()).And.MetadataType.Should()
                .Be(StorageAttributes.AttributeFileSize);
        }

        [Fact]
        public async Task failing_to_check_for_file_existence()
        {
            // Arrange
            var s3ClientMock = new Mock<IAmazonS3>();
            s3ClientMock
                .Setup(
                    m => m.GetObjectMetadataAsync(
                        It.IsAny<string>(),
                        It.IsAny<string>(),
                        It.IsAny<CancellationToken>()))
                .ThrowsAsync(new AmazonS3Exception("Oh no"));

            var adapter = CreateAdapter(s3ClientMock.Object);

            // Act
            Func<Task> action = () => adapter.FileExists("path.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToCheckFileExistenceException>();
        }

        [Fact]
        public async Task streaming_reads_are_not_seekable_and_non_streaming_are()
        {
            // Arrange
            var (path, _) = await GivenWeHaveAnExistingFile();
            var adapter = CreateAdapter();

            // Act
            var stream = await adapter.Read(path);

            // Assert
            stream.CanSeek.Should().BeTrue();
        }

        [Fact]
        public async Task moving_with_updated_metadata()
        {
            // Arrange
            var (path, _) = await GivenWeHaveAnExistingFile();
            const string destinationPath = "destination.txt";
            const string newMimeType = "text/plain+special";
            var adapter = CreateAdapter();

            // Act
            await adapter.Move(
                path,
                destinationPath,
                new Config
                {
                    { "ContentType", newMimeType },
                    { "MetadataDirective", "REPLACE" }
                });

            // Assert
            (await adapter.MimeType(destinationPath)).MimeType.Should().Be(newMimeType);
        }

        [Fact]
        public async Task setting_acl_via_options()
        {
            // Arrange
            var adapter = CreateAdapter();

            // Act
            await adapter.Write("path.txt", Stream.Null, new Config { { "ACL", "bucket-owner-full-control" } });

            // Assert
            var acl = await _amazonS3Client.GetACLAsync(
                new GetACLRequest()
                {
                    BucketName = _bucketName,
                    Key = _pathPrefixer.PrefixPath("path.txt")
                });

            acl.AccessControlList.Grants.Should().Contain(g => g.Permission.Value == "FULL_CONTROL");
        }

        private async Task<Tuple<string, string>> GivenWeHaveAnExistingFile(
            string withPath = "path.txt",
            string withContents = "contents",
            Visibility visibility = Visibility.Public,
            CancellationToken cancellationToken = default)
        {
            await _amazonS3Client.PutObjectAsync(
                new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = _pathPrefixer.PrefixPath(withPath),
                    InputStream = new MemoryStream(Encoding.UTF8.GetBytes(withContents))
                },
                cancellationToken);

            return new Tuple<string, string>(withPath, withContents);
        }

        private AwsS3FilesystemAdapter CreateAdapter(IAmazonS3 client = null)
        {
            return new AwsS3FilesystemAdapter(
                _bucketName,
                client ?? _amazonS3Client,
                _pathPrefixer,
                new AclVisibilityConverter());
        }
    }
}