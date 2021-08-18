using System;

namespace DutchAndBold.Flystorage.Abstractions.Exceptions
{
    public class SymbolicLinkEncounteredException : FilesystemException
    {
        public SymbolicLinkEncounteredException(string message, string location, Exception innerException = null)
            : base(message, location, innerException)
        {
        }

        public static SymbolicLinkEncounteredException AtLocation(string location)
        {
            return new($"Unsupported symbolic link encountered at location {location}.", location);
        }
    }
}