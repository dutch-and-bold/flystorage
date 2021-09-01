using System.Collections.Generic;
using Amazon.S3;
using Amazon.S3.Model;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Adapters.AwsS3
{
    public interface IAclVisibilityConverter
    {
        /// <summary>
        /// Get Acl value for visibility.
        /// </summary>
        /// <param name="visibility"></param>
        /// <returns></returns>
        public S3CannedACL VisibilityToAcl(Visibility visibility);

        /// <summary>
        /// Get Visibility value for Acl.
        /// </summary>
        /// <param name="grants"></param>
        /// <returns></returns>
        public Visibility AclToVisibility(List<S3Grant> grants);

        /// <summary>
        /// Get default visibility for directories.
        /// </summary>
        /// <returns></returns>
        public Visibility DefaultForDirectories();
    }
}