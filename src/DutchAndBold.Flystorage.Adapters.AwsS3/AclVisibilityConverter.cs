using System.Collections.Generic;
using System.Linq;
using Amazon.S3;
using Amazon.S3.Model;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Adapters.AwsS3
{
    public class AclVisibilityConverter : IAclVisibilityConverter
    {
        private const string PublicGranteeUri = "http://acs.amazonaws.com/groups/global/AllUsers";

        private const string PublicGrantsPermission = "READ";

        private readonly Visibility _defaultForDirectories;

        public AclVisibilityConverter(Visibility defaultForDirectories = Visibility.Public)
        {
            _defaultForDirectories = defaultForDirectories;
        }

        public S3CannedACL VisibilityToAcl(Visibility visibility)
        {
            return visibility == Visibility.Public ? S3CannedACL.PublicRead : S3CannedACL.Private;
        }

        public Visibility AclToVisibility(List<S3Grant> grants)
        {
            return grants.Any(
                grant => grant.Grantee.URI == PublicGranteeUri && grant.Permission == PublicGrantsPermission)
                ? Visibility.Public
                : Visibility.Private;
        }

        public Visibility DefaultForDirectories()
        {
            return _defaultForDirectories;
        }
    }
}