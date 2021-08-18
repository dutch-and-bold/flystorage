using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Adapters.Shared.Contracts
{
    public interface IVisibilityConverter
    {
        public int ForFile(Visibility visibility);

        public int ForDirectory(Visibility visibility);

        public Visibility InverseForFile(int visibility);

        public Visibility InverseForDirectory(int visibility);

        public int DefaultForDirectories();
    }
}