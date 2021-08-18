using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToRetrieveMetadataException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationRetrieveMetadata;

        public string MetadataType { get; }

        public UnableToRetrieveMetadataException(string location, string metadataType, Exception innerException = null)
            : base(
                $"Unable to retrieve the {metadataType} for file at location: {location}. {innerException?.Message}",
                location,
                innerException)
        {
            MetadataType = metadataType;
        }

        public static UnableToRetrieveMetadataException MimeType(string location, Exception innerException = null)
        {
            return new(location, StorageAttributes.AttributeMimeType, innerException);
        }

        public static UnableToRetrieveMetadataException Visibility(string location, Exception innerException = null)
        {
            return new(location, StorageAttributes.AttributeVisibility, innerException);
        }

        public static UnableToRetrieveMetadataException FileSize(string location, Exception innerException = null)
        {
            return new(location, StorageAttributes.AttributeFileSize, innerException);
        }

        public static UnableToRetrieveMetadataException LastModified(string location, Exception innerException = null)
        {
            return new(location, StorageAttributes.AttributeLastModified, innerException);
        }
    }
}