using DutchAndBold.Flystorage.Adapters.Shared.Contracts;

namespace DutchAndBold.Flystorage.Adapters.Shared
{
    public class PathPrefixer : IPathPrefixer
    {
        private readonly string _prefix;

        private readonly char _separator;

        public PathPrefixer(string prefix = "", char separator = '/')
        {
            _prefix = prefix.TrimEnd('\\', '/');
            _separator = separator;

            if (_prefix != "" || prefix == _separator.ToString())
            {
                _prefix += _separator;
            }
        }

        public string PrefixPath(string path)
        {
            return _prefix + path.TrimStart('\\', '/');
        }

        public string StripPrefix(string path)
        {
            return path[_prefix.Length..];
        }

        public string StripDirectoryPrefix(string path)
        {
            return StripPrefix(path).TrimEnd('\\', '/');
        }

        public string PrefixDirectoryPath(string path)
        {
            var prefixedPath = PrefixPath(path.TrimEnd('\\', '/'));

            if (prefixedPath.EndsWith(_separator) || string.IsNullOrEmpty(prefixedPath))
            {
                return prefixedPath;
            }

            return prefixedPath + _separator;
        }
    }
}