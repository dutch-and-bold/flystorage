using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToDeleteDirectoryException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationDelete;

        public UnableToDeleteDirectoryException(string message, string location, Exception innerException = null)
            : base(message, location, innerException)
        {
        }

        public static UnableToDeleteDirectoryException AtLocation(string location, Exception innerException)
        {
            return new($"Unable to write file at location: {location}. {innerException.Message}",
                location,
                innerException);
        }
    }
}