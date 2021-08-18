namespace DutchAndBold.Flystorage.Abstractions.Models
{
    public enum FileSystemOperation
    {
        OperationWrite = 0,

        OperationUpdate = 1,

        OperationFileExists = 2,

        OperationCreateDirectory = 3,

        OperationDelete = 4,

        OperationDeleteDirectory = 5,

        OperationMove = 6,

        OperationRetrieveMetadata = 7,

        OperationCopy = 8,

        OperationRead = 9,

        OperationSetVisibility = 9
    }
}