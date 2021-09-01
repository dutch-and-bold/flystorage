using System.Collections.Generic;
using System.Linq;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;

namespace DutchAndBold.Flystorage.Adapters.Shared
{
    public class DirectorySplitter : IDirectorySplitter
    {
        public IEnumerable<string> SplitFromPath(string path)
        {
            if (!path.Contains("/"))
            {
                return new List<string> { path };
            }

            if (path.Length > 1 && !path.EndsWith('/'))
            {
                path = path[..(path.LastIndexOf('/')+1)];
            }

            var directories = new List<string>();

            while (!string.IsNullOrEmpty(path))
            {
                var lastIndex = path.LastIndexOf('/');

                if (lastIndex < 0)
                {
                    return directories;
                }

                path = path[..lastIndex];
                var root = string.IsNullOrEmpty(path);

                directories.Add(root ? "/" : path);
            }

            return directories;
        }

        public IEnumerable<string> SplitFromPaths(params string[] paths)
        {
            return paths.SelectMany(SplitFromPath).Distinct();
        }
    }
}