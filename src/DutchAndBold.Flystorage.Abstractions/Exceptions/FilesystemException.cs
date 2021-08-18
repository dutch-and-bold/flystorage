using System;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public abstract class FilesystemException : Exception
    {
        public string Location { get; }

        public FilesystemException()
        {
        }

        public FilesystemException(string message, string location)
            : base(message)
        {
            Location = location;
        }

        public FilesystemException(string message, string location, Exception innerException)
            : base(message, innerException)
        {
            Location = location;
        }
    }
}