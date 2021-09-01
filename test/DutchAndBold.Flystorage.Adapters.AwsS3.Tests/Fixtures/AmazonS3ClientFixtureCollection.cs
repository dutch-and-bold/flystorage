using Xunit;

namespace DutchAndBold.Flystorage.Adapters.AwsS3.Tests.Fixtures
{
    [CollectionDefinition(nameof(AmazonS3ClientFixtureCollection))]
    public class AmazonS3ClientFixtureCollection : ICollectionFixture<AmazonS3ClientFixture>
    {
    }
}