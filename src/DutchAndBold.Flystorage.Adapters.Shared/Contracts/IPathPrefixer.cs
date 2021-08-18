namespace DutchAndBold.Flystorage.Adapters.Shared.Contracts
{
    public interface IPathPrefixer
    {
        public string PrefixPath(string path);

        public string StripPrefix(string path);

        public string StripDirectoryPrefix(string path);

        public string PrefixDirectoryPath(string path);
    }
}