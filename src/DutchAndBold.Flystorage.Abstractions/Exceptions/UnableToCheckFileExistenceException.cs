using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToCheckFileExistenceException : FileSystemOperationFailedException
    {
        public UnableToCheckFileExistenceException(string message, string location, Exception innerException = null)
            : base(message, location, innerException)
        {
        }

        public override FileSystemOperation Operation => FileSystemOperation.OperationFileExists;

        public static UnableToCheckFileExistenceException AtLocation(string location, Exception exception = null)
        {
            return new UnableToCheckFileExistenceException(
                $"Unable to check file existence for: {location}",
                location,
                exception);
        }
    }
}