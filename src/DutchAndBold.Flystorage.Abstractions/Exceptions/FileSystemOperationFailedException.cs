using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public abstract class FileSystemOperationFailedException : FilesystemException
    {
        public abstract FileSystemOperation Operation { get; }

        protected FileSystemOperationFailedException(string message, string location, Exception innerException = null)
            : base(message, location, innerException)
        {
        }
    }
}