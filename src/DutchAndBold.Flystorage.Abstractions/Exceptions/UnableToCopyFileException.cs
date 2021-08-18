using System;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class UnableToCopyFileException : FileSystemOperationFailedException
    {
        public override FileSystemOperation Operation => FileSystemOperation.OperationCopy;

        public string SourcePath => Location;

        public string DestinationPath { get; }

        public UnableToCopyFileException(
            string message,
            string sourcePath,
            string destinationPath,
            Exception innerException = null)
            : base(message, sourcePath, innerException)
        {
            DestinationPath = destinationPath;
        }

        public static UnableToCopyFileException FromLocationTo(
            string sourcePath,
            string destinationPath,
            Exception innerException)
        {
            return new($"Unable to copy file from {sourcePath} to {destinationPath}", sourcePath, destinationPath,
                innerException);
        }
    }
}