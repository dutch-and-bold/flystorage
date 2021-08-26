using DutchAndBold.Flystorage.Adapters.Local.Contracts;

namespace DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies
{
    public interface IFilePermissionStrategyFactory
    {
        /// <summary>
        /// Creates a <see cref="IFilePermissionStrategy"/> for current OS.
        /// </summary>
        /// <returns><see cref="IFilePermissionStrategy"/></returns>
        IFilePermissionStrategy CreateForOS();
    }
}