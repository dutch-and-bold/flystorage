using System;
using Amazon.S3;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Adapters.AwsS3.Tests.Fixtures;
using DutchAndBold.Flystorage.Adapters.Shared;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;
using DutchAndBold.Flystorage.Adapters.Tests;
using Xunit;
using Xunit.Abstractions;

namespace DutchAndBold.Flystorage.Adapters.AwsS3.Tests
{
    [Collection(nameof(AmazonS3ClientFixtureCollection))]
    public class AwsS3FilesystemBaseTests : FileSystemAdapterBaseTests
    {
        private readonly string _bucketName;

        private readonly IAmazonS3 _amazonS3Client;

        private readonly IPathPrefixer _pathPrefixer;

        public AwsS3FilesystemBaseTests(ITestOutputHelper testOutputHelper, AmazonS3ClientFixture amazonS3ClientFixture)
        {
            _bucketName = amazonS3ClientFixture.BucketName;
            _amazonS3Client = amazonS3ClientFixture.Client;
            _pathPrefixer = new PathPrefixer($"{Environment.ProcessId}/{nameof(AwsS3FilesystemBaseTests)}/" + testOutputHelper.GetTestName());
        }

        protected override Func<IFilesystemAdapterAsync> FilesystemAdapterFactory =>
            () => new AwsS3FilesystemAdapter(
                _bucketName,
                _amazonS3Client,
                _pathPrefixer,
                new AclVisibilityConverter());
    }
}