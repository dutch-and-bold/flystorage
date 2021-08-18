namespace DutchAndBold.Flystorage.Abstractions.Models
{
    public abstract class StorageAttributes
    {
        public const string AttributePath = "path";

        public const string AttributeType = "type";

        public const string AttributeFileSize = "file_size";

        public const string AttributeVisibility = "visibility";

        public const string AttributeLastModified = "last_modified";

        public const string AttributeMimeType = "mime_type";

        public const string AttributeExtraMetadata = "extra_metadata";

        public abstract bool IsFile();

        public bool IsDirectory() => !IsFile();
    }
}