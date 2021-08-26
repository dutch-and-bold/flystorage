using System.IO;

namespace DutchAndBold.Flystorage.Adapters.InMemory
{
    public class InMemoryDirectoryPlaceholder : InMemoryFile
    {
        public InMemoryDirectoryPlaceholder()
            : base(new MemoryStream())
        {
        }
    }
}