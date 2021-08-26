using System;
using System.IO;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Adapters.InMemory
{
    public class InMemoryFile
    {
        public Stream Contents { get; }

        public DateTimeOffset LastModified { get; init; }

        public Visibility Visibility { get; init; }

        public InMemoryFile(Stream contents)
        {
            Contents = contents;
            LastModified = DateTimeOffset.Now;
            Visibility = Visibility.Public;
        }

        /// <summary>
        /// Creates a new <see cref="InMemoryFile"/> with the same data.
        /// </summary>
        /// <param name="visibility">Will use given Visibility in copy if not null.</param>
        /// <param name="lastModified">Will use given LastModified in copy if not null.</param>
        /// <returns>Copy of <see cref="InMemoryFile"/></returns>
        public InMemoryFile Copy(Visibility? visibility = null, DateTimeOffset? lastModified = null)
        {
            return new InMemoryFile(Contents)
            {
                LastModified = lastModified ?? LastModified,
                Visibility = visibility ?? Visibility
            };
        }
    }
}