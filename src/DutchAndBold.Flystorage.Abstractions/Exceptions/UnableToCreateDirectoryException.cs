using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToCreateDirectoryException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationCreateDirectory;

        public UnableToCreateDirectoryException(string message, string location, Exception innerException)
            : base(message, location, innerException)
        {
        }

        public static UnableToCreateDirectoryException AtLocation(string location, Exception exception = null)
        {
            return new(exception?.Message ?? "Unable to create directory.", location, exception);
        }
    }
}