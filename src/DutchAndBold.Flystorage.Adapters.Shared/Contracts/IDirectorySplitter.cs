using System.Collections.Generic;

namespace DutchAndBold.Flystorage.Adapters.Shared.Contracts
{
    public interface IDirectorySplitter
    {
        public IEnumerable<string> SplitFromPath(string path);

        public IEnumerable<string> SplitFromPaths(params string[] paths);
    }
}