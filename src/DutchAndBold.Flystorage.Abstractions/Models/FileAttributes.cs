using System;

namespace DutchAndBold.Flystorage.Abstractions.Models
{
    public class FileAttributes : StorageAttributes
    {
        public long? FileSize { get; init; }

        public Visibility Visibility { get; init; }

        public DateTimeOffset LastModified { get; init; }

        public string MimeType { get; init; }

        public string[] ExtraMetadata { get; init; } = Array.Empty<string>();

        public override bool IsFile() => true;

        public FileAttributes(string path)
            : base(path)
        {
        }
    }
}