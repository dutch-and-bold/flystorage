using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Local.Contracts;
using DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies;
using DutchAndBold.Flystorage.Adapters.Local.Models;
using DutchAndBold.Flystorage.Adapters.Shared;
using DutchAndBold.Flystorage.Adapters.Shared.Contracts;
using FluentAssertions;
using Mono.Unix;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace DutchAndBold.Flystorage.Adapters.Local.Tests
{
    public class LocalFilesystemAdapterTests : IDisposable
    {
        private readonly string _root;

        //[SupportedOSPlatformGuard("linux")] TODO: Enable in .NET6.0
        //[SupportedOSPlatformGuard("macos")] TODO: Enable in .NET6.0
        private readonly bool _isRunningUnderUnix = OperatingSystem.IsMacOS() || OperatingSystem.IsLinux();

        //[SupportedOSPlatformGuard("windows")] TODO: Enable in .NET6.0
        private readonly bool _isRunningUnderWindows = OperatingSystem.IsWindows();

        private readonly IFilePermissionStrategy _filePermissionStrategy;

        public LocalFilesystemAdapterTests(ITestOutputHelper testOutputHelper)
        {
            _root = $"{Environment.CurrentDirectory}/.test/{testOutputHelper.GetTestName()}";

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                _filePermissionStrategy = new UnixFilePermissionStrategy();
            }

            if (OperatingSystem.IsWindows())
            {
                _filePermissionStrategy = new WindowsFilePermissionsStrategy();
            }

            if (_filePermissionStrategy == null)
            {
                throw new InvalidOperationException("Invalid operating system. Linux, MacOs and Windows are supported");
            }

            DeleteDirectoryIfExists(_root);
        }

        public void Dispose()
        {
            DeleteDirectoryIfExists(_root);
        }

        [Fact]
        public void creating_a_local_filesystem_creates_a_root_directory()
        {
            // Act
            new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());

            // Assert
            Directory.Exists(_root).Should().BeTrue();
        }

        [Fact]
        public void not_being_able_to_create_a_root_directory_results_in_an_exception()
        {
            // Arrange
            var prefixer = Mock.Of<IPathPrefixer>(o =>
                o.PrefixPath(string.Empty) == InaccessiblePath + "cannot-create/this-directory/");
            Action action = () => new LocalFilesystemAdapter(prefixer, Mock.Of<IFilePermissionStrategy>());

            // Assert
            action.Should().Throw<UnableToCreateDirectoryException>();
        }

        [Fact]
        public void writing_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());

            // Act
            adapter.Write("/file.txt", "contents", new Config());

            // Assert
            Directory.Exists(_root).Should().BeTrue();
            File.Exists(_root + "/file.txt").Should().BeTrue();
            File.ReadAllText(_root + "/file.txt").Should().Be("contents");
        }

        [Fact]
        public void writing_a_file_with_a_stream()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            var stream = new MemoryStream(Encoding.Default.GetBytes("contents"));

            // Act
            adapter.Write("/file.txt", stream, new Config());

            // Assert
            Directory.Exists(_root).Should().BeTrue();
            File.Exists(_root + "/file.txt").Should().BeTrue();
            File.ReadAllText(_root + "/file.txt").Should().Be("contents");
        }

        [Fact]
        public void writing_a_file_with_a_stream_and_visibility()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, _filePermissionStrategy);
            var stream = new MemoryStream(Encoding.Default.GetBytes("something"));

            // Act
            adapter.Write(
                "/file.txt",
                stream,
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Private}}));

            // Assert
            File.ReadAllText(_root + Path.DirectorySeparatorChar + "file.txt").Should().Be("something");
            AssertFilePermissions(_root + Path.DirectorySeparatorChar + "file.txt", Visibility.Private);
        }

        [Fact]
        public void writing_a_file_with_visibility()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, _filePermissionStrategy);

            // Act
            adapter.Write(
                "/file.txt",
                "contents",
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Private}}));

            // Assert
            File.ReadAllText(_root + "/file.txt").Should().Be("contents");
            AssertFilePermissions(_root + Path.DirectorySeparatorChar + "file.txt", Visibility.Private);
        }

        [Fact]
        public void failing_to_set_visibility()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, _filePermissionStrategy);

            // Act
            Action action = () => adapter.SetVisibility("/file.txt", Visibility.Public);

            // Assert
            action.Should().Throw<UnableToSetVisibilityException>();
        }

        [Fact]
        public void failing_to_write_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(
                PrefixerWithInaccessibleLocation,
                Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.Write("/cannot-create-a-file-here", "contents", new Config());

            // Act & Assert
            action.Should().Throw<UnableToWriteFileException>();
        }

        [Fact]
        public void failing_to_write_a_file_using_a_stream()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(
                PrefixerWithInaccessibleLocation,
                Mock.Of<IFilePermissionStrategy>());
            var stream = new MemoryStream(Encoding.Default.GetBytes("something"));
            Action action = () => adapter.Write("/cannot-create-a-file-here", stream, new Config());

            // Act & Assert
            action.Should().Throw<UnableToWriteFileException>();
        }

        [Fact]
        public void deleting_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            var file = "file.txt";
            var fullPath = _root + Path.DirectorySeparatorChar + "file.txt";

            // Act
            File.WriteAllText(fullPath, "contents");
            adapter.Delete(file);

            // Assert
            File.Exists(fullPath).Should().BeFalse();
        }

        [Fact]
        public void deleting_a_file_that_does_not_exist()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.Delete("/file.txt");

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void deleting_a_file_that_cannot_be_deleted()
        {
            //     this.givenWeHaveAnExistingFile("here.txt");
            //     mock_function("unlink", false);

            //     this.expectException(UnableToDeleteFile::class);

            //     this.adapter().delete("here.txt");
        }

        [Fact]
        public void checking_if_a_file_exists()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            File.WriteAllText(_root + Path.DirectorySeparatorChar + "file.txt", "contents");

            // Act
            var fileExists = adapter.FileExists("/file.txt");

            // Assert
            fileExists.Should().BeTrue();
        }

        [Fact]
        public void checking_if_a_file_exists_that_does_not_exsist()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());

            // Act
            var fileExists = adapter.FileExists("/file.txt");

            // Assert
            fileExists.Should().BeFalse();
        }

        [Fact]
        public void listing_contents()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("directory/filename.txt", "content", new Config());
            adapter.Write("filename.txt", "content", new Config());

            // Act
            var contentListing = adapter.ListContents("/", false);
            var contents = contentListing.ToArray();

            // Assert
            contents.Length.Should().Be(2);
            contents.Should().ContainItemsAssignableTo<StorageAttributes>();
        }

        [Fact]
        public void listing_contents_recursively()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("directory/filename.txt", "content", new Config());
            adapter.Write("filename.txt", "content", new Config());

            // Act
            var contentListing = adapter.ListContents("/", true);
            var contents = contentListing.ToArray();

            // Assert
            contents.Length.Should().Be(3);
            contents.Should().ContainItemsAssignableTo<StorageAttributes>();
        }

        [Fact]
        public void listing_a_non_existing_directory()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());

            // Act
            var contentListing = adapter.ListContents("/directory/", false);
            var contents = contentListing.ToArray();

            // Assert
            contents.Should().BeEmpty();
        }

        [Fact]
        public void listing_directory_contents_with_link_skipping()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(
                PrefixerWithTestRoot,
                Mock.Of<IFilePermissionStrategy>(),
                SymbolicLinkPolicy.SkipLinks);

            File.WriteAllText(_root + Path.DirectorySeparatorChar + "file.txt", "content");
            CreateSymbolicLink(
                _root + Path.DirectorySeparatorChar + "file.txt",
                _root + Path.DirectorySeparatorChar + "link.txt");

            // Act
            var contentListing = adapter.ListContents("/", true);
            var contents = contentListing.ToArray();

            // Assert
            contents.Should().ContainSingle();
        }

        [Fact]
        public void listing_directory_contents_with_disallowing_links()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(
                PrefixerWithTestRoot,
                Mock.Of<IFilePermissionStrategy>(),
                SymbolicLinkPolicy.DisallowLinks);

            File.WriteAllText(_root + Path.DirectorySeparatorChar + "file.txt", "content");
            CreateSymbolicLink(
                _root + Path.DirectorySeparatorChar + "file.txt",
                _root + Path.DirectorySeparatorChar + "link.txt");

            // Act & Assert
            Action action = () => adapter.ListContents("/", true).ToArray();
            action.Should().Throw<SymbolicLinkEncounteredException>();
        }

        [Fact]
        public void deleting_a_directory()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            var directoryPath = _root +
                                Path.DirectorySeparatorChar +
                                "directory" +
                                Path.DirectorySeparatorChar +
                                "subdir" +
                                Path.DirectorySeparatorChar;
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(directoryPath + Path.DirectorySeparatorChar + "file.txt", "content");
            CreateSymbolicLink(
                directoryPath + Path.DirectorySeparatorChar + "file.txt",
                directoryPath + Path.DirectorySeparatorChar + "link.txt");

            // Act
            adapter.DeleteDirectory("directory/subdir");

            // Assert
            Directory.Exists(directoryPath).Should().BeFalse();

            // Act
            adapter.DeleteDirectory("directory");

            // Assert
            Directory.Exists(_root + Path.DirectorySeparatorChar + "directory").Should().BeFalse();
        }

        [Fact]
        public void deleting_directories_with_other_directories_in_it()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            var directoryPath = _root +
                                Path.DirectorySeparatorChar +
                                "a" +
                                Path.DirectorySeparatorChar +
                                "b" +
                                Path.DirectorySeparatorChar +
                                "c" +
                                Path.DirectorySeparatorChar +
                                "d" +
                                Path.DirectorySeparatorChar;
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(directoryPath + Path.DirectorySeparatorChar + "e.txt", "contents");

            // Act
            adapter.DeleteDirectory("a/b");

            // Assert
            Directory.Exists(_root + Path.DirectorySeparatorChar + "a").Should().BeTrue();
            Directory.Exists(_root + Path.DirectorySeparatorChar + "directory" + Path.DirectorySeparatorChar + "b")
                .Should()
                .BeFalse();
        }

        [Fact]
        public void deleting_a_non_existing_directory()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.DeleteDirectory("/non-existing-directory/");

            // Act & Assert
            action.Should().NotThrow();
        }

        [Fact(Skip = "Not able to mock System.IO.Directory to a failure at this point in time.")]
        public void not_being_able_to_delete_a_directory()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.DeleteDirectory("/non-existing-directory/");

            // TODO: Mock System.IO.Directory.Delete failure

            // Act & Assert
            action.Should().Throw<UnableToDeleteDirectoryException>();
        }

        [Fact(Skip = "Not able to mock System.IO.Directory to a failure at this point in time.")]
        public void not_being_able_to_delete_a_sub_directory()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            var directoryPath = _root +
                                Path.DirectorySeparatorChar +
                                "a" +
                                Path.DirectorySeparatorChar +
                                "b";
            Directory.CreateDirectory(directoryPath);

            // TODO: Mock System.IO.Directory.Delete failure

            Action action = () => adapter.DeleteDirectory("a/b");

            // Act & Assert
            action.Should().Throw<UnableToDeleteDirectoryException>();
        }

        [Fact]
        public void creating_a_directory()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, _filePermissionStrategy);

            // Act
            adapter.CreateDirectory(
                "public",
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Public}}));
            adapter.CreateDirectory(
                "private",
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Private}}));
            adapter.CreateDirectory(
                "also_private",
                new Config(new Dictionary<string, object> {{"directory_visibility", Visibility.Private}}));

            // Assert
            AssertDirectoryPermissions(_root + Path.DirectorySeparatorChar + "public", Visibility.Public);
            AssertDirectoryPermissions(_root + Path.DirectorySeparatorChar + "private", Visibility.Private);
            AssertDirectoryPermissions(_root + Path.DirectorySeparatorChar + "also_private", Visibility.Private);
        }

        [Fact]
        public void not_being_able_to_create_a_directory()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(
                PrefixerWithInaccessibleLocation,
                Mock.Of<IFilePermissionStrategy>());

            Action action = () => adapter.CreateDirectory("/something/", new Config());

            // Assert
            action.Should().Throw<UnableToCreateDirectoryException>();
        }

        [Fact]
        public void creating_a_directory_is_idempotent()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, _filePermissionStrategy);

            // Act
            adapter.CreateDirectory(
                "/something/",
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Private}}));
            adapter.CreateDirectory(
                "/something/",
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Public}}));

            // Assert
            AssertDirectoryPermissions(_root + Path.DirectorySeparatorChar + "/something", Visibility.Public);
        }

        [Fact]
        public void retrieving_visibility()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, _filePermissionStrategy);
            adapter.Write(
                "public.txt",
                "contents",
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Public}}));
            adapter.Write(
                "private.txt",
                "contents",
                new Config(new Dictionary<string, object> {{"visibility", Visibility.Private}}));

            // Act
            var visibilityPublic = adapter.Visibility("public.txt");
            var visibilityPrivate = adapter.Visibility("private.txt");

            // Assert
            visibilityPublic.Visibility.Should().Be(Visibility.Public);
            visibilityPrivate.Visibility.Should().Be(Visibility.Private);
        }

        [Fact]
        public void not_being_able_to_retrieve_visibility()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.Visibility("something.txt");

            // Act & Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void moving_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("first.txt", "contents", new Config());

            // Act
            adapter.Move("first.txt", "second.txt", new Config());

            // Assert
            File.Exists(_root + "/second.txt").Should().BeTrue();
            File.Exists(_root + "/first.txt").Should().BeFalse();
        }

        [Fact]
        public void not_being_able_to_move_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.Move("first.txt", "second.txt", new Config());

            // Act & Assert
            action.Should().Throw<UnableToMoveFileException>();
        }

        [Fact]
        public void copying_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("first.txt", "contents", new Config());

            // Act
            adapter.Copy("first.txt", "second.txt", new Config());

            // Assert
            File.Exists(_root + "/first.txt").Should().BeTrue();
            File.Exists(_root + "/second.txt").Should().BeTrue();
        }

        [Fact]
        public void not_being_able_to_copy_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.Copy("first.txt", "second.txt", new Config());

            // Act & Assert
            action.Should().Throw<UnableToCopyFileException>();
        }

        [Fact]
        public void getting_mimetype()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("flysystem.svg", File.ReadAllText("TestFiles/flysystem.svg"), new Config());

            // Act
            var fileAttributes = adapter.MimeType("flysystem.svg");

            // Assert
            fileAttributes.MimeType.Should().StartWith("image/svg");
        }

        [Fact]
        public void fetching_unknown_mime_type_of_a_file()
        {
            //     this.useAdapter(new LocalFilesystemAdapter(self::ROOT, null, LOCK_EX, LocalFilesystemAdapter::DISALLOW_LINKS, new ExtensionMimeTypeDetector(new EmptyExtensionToMimeTypeMap())));

            //     parent::fetching_unknown_mime_type_of_a_file();
        }

        [Fact]
        public void not_being_able_to_get_mimetype()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.MimeType("flysystem.svg");

            // Act & Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void getting_last_modified()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            var theTimeJustBeforeCreation = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(1));
            adapter.Write("first.txt", "contents", new Config());

            // Act
            var fileAttributes = adapter.LastModified("first.txt");

            // Assert
            fileAttributes.LastModified.Should().BeAfter(theTimeJustBeforeCreation);
        }

        [Fact]
        public void not_being_able_to_get_last_modified()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.LastModified("first.txt");

            // Act & Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void getting_file_size()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("first.txt", "contents", new Config());

            // Act
            var fileAttributes = adapter.FileSize("first.txt");

            // Assert
            fileAttributes.FileSize.Should().Be(8);
        }

        [Fact]
        public void not_being_able_to_get_file_size()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.FileSize("first.txt");

            // Act & Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void reading_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("path.txt", "contents", new Config());

            // Act
            var contents = adapter.ReadString("path.txt");

            // Assert
            contents.Should().Be("contents");
        }

        [Fact]
        public void not_being_able_to_read_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.ReadString("path.txt");

            // Act & Assert
            action.Should().Throw<UnableToReadFileException>();
        }

        [Fact]
        public void reading_a_stream()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            adapter.Write("path.txt", "contents", new Config());

            // Act
            var stream = adapter.Read("path.txt");
            var contents = new StreamReader(stream).ReadToEnd();
            stream.Close();

            // Assert
            contents.Should().Be("contents");
        }

        [Fact]
        public void not_being_able_to_stream_read_a_file()
        {
            // Arrange
            var adapter = new LocalFilesystemAdapter(PrefixerWithTestRoot, Mock.Of<IFilePermissionStrategy>());
            Action action = () => adapter.Read("path.txt");

            // Act & Assert
            action.Should().Throw<UnableToReadFileException>();
        }

        // /* //////////////////////
        // // These are the utils //
        // ////////////////////// */

        private static void DeleteDirectoryIfExists(string fullPath)
        {
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }

        private void AssertFilePermissions(string path, Visibility visibility)
        {
            _filePermissionStrategy.GetDirectoryPermissions(path).Should().Be(visibility);
        }

        private void AssertDirectoryPermissions(string path, Visibility visibility)
        {
            _filePermissionStrategy.GetDirectoryPermissions(path).Should().Be(visibility);
        }

        private void CreateSymbolicLink(string sourcePath, string linkPath)
        {
            if (_isRunningUnderWindows)
            {
                WindowsTestUtilities.CreateSymbolicLink(linkPath, sourcePath);
                return;
            }

            if (_isRunningUnderUnix)
            {
                var unixFileInfo = new UnixFileInfo(sourcePath);
                unixFileInfo.CreateSymbolicLink(linkPath);
                return;
            }

            throw new InvalidOperationException("Operating system not supported.");
        }

        private string InaccessiblePath =>
            Path.GetPathRoot(Environment.CurrentDirectory) + (_isRunningUnderWindows ? "Windows\\" : "");

        private IPathPrefixer PrefixerWithInaccessibleLocation => new PathPrefixer(
            InaccessiblePath,
            Path.DirectorySeparatorChar);

        private IPathPrefixer PrefixerWithTestRoot => new PathPrefixer(_root, Path.DirectorySeparatorChar);
    }
}