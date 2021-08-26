using System;
using System.IO;
using System.Linq;
using System.Text;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Extensions;
using FluentAssertions;
using Xunit;
using FileAttributes = DutchAndBold.Flystorage.Abstractions.Models.FileAttributes;

namespace DutchAndBold.Flystorage.Adapters.Tests
{
    public abstract class FileSystemAdapterBaseTests
    {
        protected abstract Func<IFilesystemAdapter> FilesystemAdapterFactory { get; }

        [Fact]
        public void writing_and_reading_with_string()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            adapter.WriteString("path.txt", "contents", new Config());
            var contents = adapter.ReadString("path.txt");

            // Assert
            contents.Should().Be("contents");
        }

        [Fact]
        public void writing_a_file_with_a_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            adapter.Write("path.txt", new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            var fileExists = adapter.FileExists("path.txt");

            // Assert
            fileExists.Should().BeTrue();
        }

        [Theory]
        [InlineData("some/file[name].txt")] // a path with square brackets in filename 1
        [InlineData("some/file[0].txt")] // a path with square brackets in filename 2
        [InlineData("some/file[10].txt")] // a path with square brackets in filename 3
        [InlineData("some[name]/file.txt")] // a path with square brackets in dirname 1
        [InlineData("some[0]/file.txt")] // a path with square brackets in dirname 2
        [InlineData("some[10]/file.txt")] // a path with square brackets in dirname 3
        [InlineData("some/file{name}.txt")] // a path with curly brackets in filename 1
        [InlineData("some/file{0}.txt")] // a path with curly brackets in filename 2
        [InlineData("some/file{10}.txt")] // a path with curly brackets in filename 3
        [InlineData("some{name}/filename.txt")] // a path with curly brackets in dirname 1
        [InlineData("some{0}/filename.txt")] // a path with curly brackets in dirname 2
        [InlineData("some{10}/filename.txt")] // a path with curly brackets in dirname 3
        [InlineData("some dir/filename.txt")] // a path with space in dirname
        [InlineData("somedir/file name.txt")] // a path with space in filename
        [InlineData("some-dir/file.txt")] // a path mixed backward slash
        public void writing_and_reading_files_with_special_path(string path)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            adapter.Write(path, new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            var stream = adapter.Read(path);
            var contents = new StreamReader(stream).ReadToEnd();
            var listedContents = adapter.ListContents("", true);

            // Assert
            listedContents.Should().HaveCount(2);
            contents.Should().Be("contents");
        }

        [Fact]
        public void fetching_file_size()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out var path, out _);

            // Act
            var attributes = adapter.FileSize(path);

            // Assert
            attributes.Should().BeOfType<FileAttributes>();
            attributes.FileSize.Should().Be(8);
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        public void setting_visibility(Visibility visibilityToSet)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(
                adapter,
                out var path,
                out _,
                withConfig: new Config { { Config.OptionVisibility, Visibility.Public } });

            // Act
            adapter.SetVisibility(path, visibilityToSet);

            // Assert
            adapter.Visibility(path).Visibility.Should().Be(visibilityToSet);
        }

        [Fact]
        public void fetching_file_size_of_a_directory()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            adapter.CreateDirectory("path");

            // Act
            Action action = () => adapter.FileSize("path");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void fetching_file_size_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.FileSize("non-existing-file.txt");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void fetching_last_modified_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.LastModified("non-existing-file.txt");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void fetching_visibility_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.Visibility("non-existing-file.txt");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void fetching_the_mime_type_of_an_svg_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out var path, out _, withPath: "file.svg");

            // Act
            var fileAttributes = adapter.MimeType(path);

            // Assert
            fileAttributes.MimeType.Should().StartWith("image/svg");
        }

        [Fact]
        public void fetching_mime_type_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.MimeType("non-existing-file.txt");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void fetching_unknown_mime_type_of_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out var path, out _, withPath: "file.000xyz");

            // Act
            Action action = () => adapter.MimeType(path);

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void listing_a_toplevel_directory()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out _, out _, withPath: "path1.txt");
            GivenWeHaveAnExistingFile(adapter, out _, out _, withPath: "path2.txt");

            // Act
            var contents = adapter.ListContents("", true);

            // Assert
            contents.Should().HaveCount(2);
        }

        [Fact]
        public void writing_and_reading_with_streams()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            adapter.Write("path.txt", new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            var stream = adapter.Read("path.txt");
            var contents = new StreamReader(stream).ReadToEnd();

            // Assert
            contents.Should().Be("contents");
        }

        [Fact]
        public void setting_visibility_on_a_file_that_does_not_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.SetVisibility("path.txt", Visibility.Private);

            // Assert
            action.Should().Throw<UnableToSetVisibilityException>();
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        public void copying_a_file(Visibility visibility)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(
                adapter,
                out var sourcePath,
                out var contentsToAssert,
                withPath: "source.txt",
                withContents: "contents to be copied",
                withConfig: new Config { { Config.OptionVisibility, visibility } });
            const string destinationPath = "destination.txt";

            // Act
            adapter.Copy(sourcePath, destinationPath);

            // Assert
            adapter.FileExists(destinationPath).Should().BeTrue();
            adapter.Visibility(destinationPath).Visibility.Should().Be(visibility);
            adapter.ReadString(destinationPath).Should().Be(contentsToAssert);
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        public void moving_a_file(Visibility visibility)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(
                adapter,
                out var sourcePath,
                out var contentsToAssert,
                withPath: "source.txt",
                withContents: "contents to be moved",
                withConfig: new Config { { Config.OptionVisibility, visibility } });
            const string destinationPath = "destination.txt";

            // Act
            adapter.Move(sourcePath, destinationPath, new Config());

            // Assert
            adapter.FileExists(sourcePath).Should().BeFalse();
            adapter.FileExists(destinationPath).Should().BeTrue();
            adapter.Visibility(destinationPath).Visibility.Should().Be(visibility);
            adapter.ReadString(destinationPath).Should().Be(contentsToAssert);
        }

        [Fact]
        public void reading_a_file_that_does_not_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.Read("path.txt");

            // Assert
            action.Should().Throw<UnableToReadFileException>();
        }

        [Fact]
        public void moving_a_file_that_does_not_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.Move("source.txt", "destination.txt");

            // Assert
            action.Should().Throw<UnableToMoveFileException>();
        }

        [Fact]
        public void trying_to_delete_a_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            adapter.Delete("path.txt");

            // Assert
            adapter.FileExists("path.txt").Should().BeFalse();
        }

        [Fact]
        public void checking_if_files_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "some/path.txt";

            // Act
            adapter.Write(path, new MemoryStream(), new Config());

            // Assert
            adapter.FileExists(path).Should().BeTrue();
        }

        [Fact]
        public void fetching_last_modified()
        {
            // Arrange
            var startOfTest = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(1));
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "some/path.txt";

            // Act
            adapter.Write(path, new MemoryStream(), new Config());

            // Assert
            adapter.LastModified(path).LastModified.Should().BeAfter(startOfTest);
        }

        [Fact]
        public void failing_to_read_a_non_existing_file_into_a_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.Read("source.txt");

            // Assert
            action.Should().Throw<UnableToReadFileException>();
        }

        [Fact]
        public void failing_to_read_a_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Action action = () => adapter.ReadString("source.txt");

            // Assert
            action.Should().Throw<UnableToReadFileException>();
        }

        [Fact]
        public void writing_a_file_with_an_empty_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "some/path.txt";

            // Act
            adapter.Write(path, new MemoryStream(), new Config());

            // Assert
            adapter.ReadString(path).Should().Be(string.Empty);
        }

        [Fact]
        public void reading_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out var path, out var contentsToAssert, withContents: "contents");

            // Act
            var contents = adapter.ReadString(path);

            // Assert
            contents.Should().Be(contentsToAssert);
        }

        [Fact]
        public void reading_a_file_with_a_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out var path, out var contentsToAssert, withContents: "contents");

            // Act
            var stream = adapter.Read(path);

            // Assert
            new StreamReader(stream).ReadToEnd().Should().Be(contentsToAssert);
        }

        [Fact]
        public void overwriting_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(
                adapter,
                out var path,
                out _,
                "path.txt",
                "contents",
                new Config { { Config.OptionVisibility, Visibility.Public } });

            // Act
            adapter.Write(
                path,
                new MemoryStream(Encoding.UTF8.GetBytes("new contents")),
                new Config { { Config.OptionVisibility, Visibility.Private } });

            // Assert
            adapter.ReadString(path).Should().Be("new contents");
            adapter.Visibility(path).Visibility.Should().Be(Visibility.Private);
        }

        [Fact]
        public void deleting_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out var path, out _);

            // Act
            adapter.Delete(path);

            // Assert
            adapter.FileExists(path).Should().BeFalse();
        }

        [Fact]
        public void listing_contents_shallow()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingFile(adapter, out _, out _, "some/0-path.txt");
            GivenWeHaveAnExistingFile(adapter, out _, out _, "some/1-nested/path.txt");

            // Act
            var contents = adapter.ListContents("some", false).ToList();

            // Assert
            contents.Should().HaveCount(2);

            contents.Should().Contain(o => o.Path == "some/1-nested");
            contents.Should().Contain(o => o.Path == "some/0-path.txt");

            contents.First(o => o.Path == "some/1-nested").IsDirectory().Should().BeTrue();
            contents.First(o => o.Path == "some/0-path.txt").IsFile().Should().BeTrue();
        }

        [Fact]
        public void listing_contents_recursive()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            GivenWeHaveAnExistingDirectory(adapter, out _);
            GivenWeHaveAnExistingFile(adapter, out _, out _, withPath: "path/file.txt");

            // Act
            var contents = adapter.ListContents("", true);

            // Assert
            contents.Should().HaveCount(2);
        }

        [Fact]
        public void creating_a_directory()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "path";

            // Act
            adapter.CreateDirectory(path);

            // Assert
            var contents = adapter.ListContents("", false).ToList();
            contents.Should().HaveCount(1);
            contents.First().IsDirectory().Should().BeTrue();
            contents.First().Path.Should().Be(path);
        }

        [Fact]
        public void copying_a_file_with_collision()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            adapter.Write("path.txt", new MemoryStream(Encoding.UTF8.GetBytes("new contents")), new Config());
            adapter.Write("new-path.txt", new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            adapter.Copy("path.txt", "new-path.txt", new Config());

            // Assert
            adapter.ReadString("path.txt").Should().Be("new contents");
        }

        private static void GivenWeHaveAnExistingFile(
            IFilesystemAdapter adapter,
            out string path,
            out string contents,
            string withPath = "path.txt",
            string withContents = "contents",
            Config withConfig = null)
        {
            adapter.Write(withPath, new MemoryStream(Encoding.UTF8.GetBytes(withContents)), withConfig ?? new Config());

            path = withPath;
            contents = withContents;
        }

        private static void GivenWeHaveAnExistingDirectory(
            IFilesystemAdapter adapter,
            out string path,
            string withPath = "path",
            Config withConfig = null)
        {
            adapter.CreateDirectory(withPath, withConfig ?? new Config());

            path = withPath;
        }
    }
}