using System;

namespace DutchAndBold.Flystorage.Abstractions.Models
{
    public class DirectoryAttributes : StorageAttributes
    {
        public string Path { get; }

        public string Visibility { get; init; }

        public int? LastModified { get; init; }

        public string[] ExtraMetadata { get; init; } = Array.Empty<string>();

        public DirectoryAttributes(string path)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
        }

        public override bool IsFile() => false;
    }
}