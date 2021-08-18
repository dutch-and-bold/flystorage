using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToWriteFileException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationWrite;

        public UnableToWriteFileException(string message, string location, Exception innerException = null)
            : base(message, location, innerException)
        {
        }

        public static UnableToWriteFileException AtLocation(string location, Exception exception)
        {
            return new($"Unable to write file at location: {location}. {exception.Message}", location, exception);
        }
    }
}