using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToMoveFileException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationMove;

        public string SourcePath => Location;

        public string DestinationPath { get; }

        public UnableToMoveFileException(
            string message,
            string sourcePath,
            string destinationPath,
            Exception innerException = null)
            : base(message, sourcePath, innerException)
        {
            DestinationPath = destinationPath;
        }

        public static UnableToMoveFileException FromLocationTo(
            string sourcePath,
            string destinationPath,
            Exception exception)
        {
            return new($"Unable to move file from {sourcePath} to {destinationPath}", sourcePath, destinationPath,
                exception);
        }
    }
}