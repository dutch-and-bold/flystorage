using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToReadFileException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationRead;

        public UnableToReadFileException(string message, string location, Exception innerException = null)
            : base(message, location, innerException)
        {
        }

        public static UnableToReadFileException AtLocation(string location, Exception exception)
        {
            return new($"Unable to read file at location: {location}. {exception.Message}", location, exception);
        }
    }
}