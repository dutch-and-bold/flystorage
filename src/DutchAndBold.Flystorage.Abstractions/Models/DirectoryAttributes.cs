using System;

namespace DutchAndBold.Flystorage.Abstractions.Models
{
    public class DirectoryAttributes : StorageAttributes
    {

        public string Visibility { get; init; }

        public int? LastModified { get; init; }

        public string[] ExtraMetadata { get; init; } = Array.Empty<string>();

        public override bool IsFile() => false;

        public DirectoryAttributes(string path)
            : base(path)
        {
        }
    }
}