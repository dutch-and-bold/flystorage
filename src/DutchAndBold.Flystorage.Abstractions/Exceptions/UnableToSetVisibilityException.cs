using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToSetVisibilityException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationSetVisibility;

        public UnableToSetVisibilityException(string message, string location, Exception innerException = null)
            : base(message, location, innerException)
        {
        }

        public static UnableToSetVisibilityException AtLocation(string location, Exception innerException)
        {
            return new($"Unable to write file at location: {location}. {innerException.Message}",
                location,
                innerException);
        }
    }
}