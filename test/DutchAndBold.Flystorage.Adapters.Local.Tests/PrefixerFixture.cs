using System;
using System.IO;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;
using Moq;

namespace DutchAndBold.Flystorage.Adapters.Local.Tests
{
    public class PrefixerFixture
    {
        public PrefixerFixture()
        {
            var prefixerMock = new Mock<IPathPrefixer>();

            prefixerMock.Setup(p => p.PrefixPath(It.IsAny<string>()))
                .Returns<string>(p => Root + (string.IsNullOrEmpty(p) ? "" : Path.DirectorySeparatorChar + p));

            prefixerMock.Setup(p => p.StripPrefix(It.IsAny<string>()))
                .Returns<string>(p => p.Replace(Root, ""));

            Prefixer = prefixerMock.Object;
        }

        public IPathPrefixer Prefixer { get; }

        public static string Root => Environment.CurrentDirectory +
                                     Path.DirectorySeparatorChar +
                                     ".test" +
                                     Path.DirectorySeparatorChar +
                                     "test-root";
    }
}