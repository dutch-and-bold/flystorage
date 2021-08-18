using System;

namespace DutchAndBold.Flystorage.Abstractions.Models
{
    public class FileAttributes : StorageAttributes
    {
        public string Path { get; }

        public int? FileSize { get; init; }

        public Visibility Visibility { get; init; }

        public DateTimeOffset LastModified { get; init; }

        public string MimeType { get; init; }

        public string[] ExtraMetadata { get; init; } = Array.Empty<string>();

        public FileAttributes(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override bool IsFile() => true;
    }
}