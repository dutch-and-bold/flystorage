using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToDeleteFileException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationDelete;

        public UnableToDeleteFileException(string message, string location, Exception innerException)
            : base(message, location, innerException)
        {
        }

        public static UnableToDeleteFileException AtLocation(string location, Exception exception)
        {
            return new($"Unable to write file at location: {location}. {exception.Message}", location, exception);
        }
    }
}