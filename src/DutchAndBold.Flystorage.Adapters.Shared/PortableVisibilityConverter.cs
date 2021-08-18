using System.Collections.Generic;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;

namespace DutchAndBold.Flystorage.Adapters.Shared
{
    public class PortableVisibilityConverter : IVisibilityConverter
    {
        private readonly int _filePublic;

        private readonly int _filePrivate;

        private readonly int _directoryPublic;

        private readonly int _directoryPrivate;

        private readonly Visibility _defaultForDirectories;

        public PortableVisibilityConverter(
            int filePublic = 0644,
            int filePrivate = 0600,
            int directoryPublic = 0755,
            int directoryPrivate = 0700,
            Visibility defaultForDirectories = Visibility.Private)
        {
            _filePublic = filePublic;
            _filePrivate = filePrivate;
            _directoryPublic = directoryPublic;
            _directoryPrivate = directoryPrivate;
            _defaultForDirectories = defaultForDirectories;
        }

        public int ForFile(Visibility visibility)
        {
            return visibility == Visibility.Public
                ? _filePublic
                : _filePrivate;
        }

        public int ForDirectory(Visibility visibility)
        {
            return visibility == Visibility.Public
                ? _directoryPublic
                : _directoryPrivate;
        }

        public Visibility InverseForFile(int visibility)
        {
            if (visibility == _filePublic)
            {
                return Visibility.Public;
            }
            else if (visibility == _filePrivate)
            {
                return Visibility.Private;
            }

            return Visibility.Public; // default
        }

        public Visibility InverseForDirectory(int visibility)
        {
            if (visibility == _directoryPublic)
            {
                return Visibility.Public;
            }
            else if (visibility == _directoryPrivate)
            {
                return Visibility.Private;
            }

            return Visibility.Public; // default
        }

        public int DefaultForDirectories()
        {
            return _defaultForDirectories == Visibility.Public ? _directoryPublic : _directoryPrivate;
        }

        public static PortableVisibilityConverter FromArray(
            Dictionary<string, Dictionary<string, int>> permissionMap,
            Visibility defaultForDirectories = Visibility.Private)
        {
            return new PortableVisibilityConverter(
                permissionMap["file"]?["public"] ?? 0644,
                permissionMap["file"]?["private"] ?? 0600,
                permissionMap["dir"]?["public"] ?? 0755,
                permissionMap["dir"]?["private"] ?? 0700,
                defaultForDirectories);
        }
    }
}