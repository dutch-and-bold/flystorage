using System;

namespace DutchAndBold.Flystorage.Adapters.InMemory.Exceptions
{
    public class InMemoryFileDoesNotExistException : Exception
    {
        public InMemoryFileDoesNotExistException() : base("File does not exist.")
        {
        }
    }
}