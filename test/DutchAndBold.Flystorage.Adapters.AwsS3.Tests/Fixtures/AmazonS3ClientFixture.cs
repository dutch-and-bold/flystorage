using System;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;

namespace DutchAndBold.Flystorage.Adapters.AwsS3.Tests.Fixtures
{
    public class AmazonS3ClientFixture : IDisposable
    {
        public readonly IAmazonS3 Client;

        private readonly string _awsRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "eu-west-1";

        public readonly string BucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET") ?? "flystorage-unit-tests";

        public AmazonS3ClientFixture()
        {
            Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(_awsRegion));
            DeleteAllObjects().Wait();
        }

        public void Dispose()
        {
            DeleteAllObjects().Wait();
            Client.Dispose();
        }

        private async Task DeleteAllObjects()
        {
            var objects = await Client.Paginators
                .ListObjectsV2(
                    new ListObjectsV2Request()
                    {
                        BucketName = BucketName,
                    })
                .S3Objects
                .ToListAsync();

            if (!objects.Any())
            {
                return;
            }

            await Client.DeleteObjectsAsync(
                new DeleteObjectsRequest()
                {
                    BucketName = BucketName,
                    Objects = objects.Select(s => new KeyVersion() { Key = s.Key }).ToList()
                });
        }
    }
}