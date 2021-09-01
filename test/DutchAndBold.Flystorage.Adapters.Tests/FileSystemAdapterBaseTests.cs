using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        protected abstract Func<IFilesystemAdapterAsync> FilesystemAdapterFactory { get; }

        [Fact]
        public virtual async Task writing_and_reading_with_string()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            await adapter.WriteString("path.txt", "contents", new Config());
            var contents = await adapter.ReadString("path.txt");

            // Assert
            contents.Should().Be("contents");
        }

        [Fact]
        public virtual async Task writing_a_file_with_a_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            await adapter.Write("path.txt", new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            var fileExists = await adapter.FileExists("path.txt");

            // Assert
            fileExists.Should().BeTrue();
        }

        [Theory]
        [InlineData("some/file[name].txt", 1)] // a path with square brackets in filename 1
        [InlineData("some/file[0].txt", 2)] // a path with square brackets in filename 2
        [InlineData("some/file[10].txt", 3)] // a path with square brackets in filename 3
        [InlineData("some[name]/file.txt", 4)] // a path with square brackets in dirname 1
        [InlineData("some[0]/file.txt", 5)] // a path with square brackets in dirname 2
        [InlineData("some[10]/file.txt", 6)] // a path with square brackets in dirname 3
        [InlineData("some/file{name}.txt", 7)] // a path with curly brackets in filename 1
        [InlineData("some/file{0}.txt", 8)] // a path with curly brackets in filename 2
        [InlineData("some/file{10}.txt", 9)] // a path with curly brackets in filename 3
        [InlineData("some{name}/filename.txt", 10)] // a path with curly brackets in dirname 1
        [InlineData("some{0}/filename.txt", 11)] // a path with curly brackets in dirname 2
        [InlineData("some{10}/filename.txt", 12)] // a path with curly brackets in dirname 3
        [InlineData("some dir/filename.txt", 13)] // a path with space in dirname
        [InlineData("somedir/file name.txt", 14)] // a path with space in filename
        [InlineData("some-dir/file.txt", 15)] // a path mixed backward slash
        public virtual async Task writing_and_reading_files_with_special_path(string path, int unique)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            await adapter.Write(path, new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            var stream = await adapter.Read(path);
            var contents = await new StreamReader(stream).ReadToEndAsync();

            // Assert
            contents.Should().Be("contents");
            unique.Should().BePositive(); // Work-around to get the test name to be different.
        }

        [Fact]
        public virtual async Task fetching_file_size()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, _) = await GivenWeHaveAnExistingFile(adapter);

            // Act
            var attributes = await adapter.FileSize(path);

            // Assert
            attributes.Should().BeOfType<FileAttributes>();
            attributes.FileSize.Should().Be(8);
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        public virtual async Task setting_visibility(Visibility visibilityToSet)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, _) = await GivenWeHaveAnExistingFile(
                adapter,
                withConfig: new Config { { Config.OptionVisibility, Visibility.Public } });

            // Act
            await adapter.SetVisibility(path, visibilityToSet);

            // Assert
            (await adapter.Visibility(path)).Visibility.Should().Be(visibilityToSet);
        }

        [Fact]
        public virtual async Task fetching_file_size_of_a_directory()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            await adapter.CreateDirectory("path");

            // Act
            Func<Task> action = () => adapter.FileSize("path");

            // Assert
            await action.Should().ThrowAsync<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public virtual async Task fetching_file_size_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.FileSize("non-existing-file.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public virtual async Task fetching_last_modified_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.LastModified("non-existing-file.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public virtual async Task fetching_visibility_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.Visibility("non-existing-file.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public virtual async Task fetching_the_mime_type_of_an_svg_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, _) = await GivenWeHaveAnExistingFile(adapter, "file.svg");

            // Act
            var fileAttributes = await adapter.MimeType(path);

            // Assert
            fileAttributes.MimeType.Should().StartWith("image/svg");
        }

        [Fact]
        public virtual async Task fetching_mime_type_of_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.MimeType("non-existing-file.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public virtual async Task fetching_unknown_mime_type_of_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, _) = await GivenWeHaveAnExistingFile(adapter, "file.000xyz");

            // Act
            Func<Task> action = () => adapter.MimeType(path);

            // Assert
            await action.Should().ThrowAsync<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public virtual async Task listing_a_toplevel_directory()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            await GivenWeHaveAnExistingFile(adapter, "path1.txt");
            await GivenWeHaveAnExistingFile(adapter, "path2.txt");

            // Act
            var contents = await adapter.ListContents("", true).ToListAsync();

            // Assert
            contents.Should().HaveCount(2);
        }

        [Fact]
        public virtual async Task writing_and_reading_with_streams()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            await adapter.Write("path.txt", new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            var stream = await adapter.Read("path.txt");
            var contents = await new StreamReader(stream).ReadToEndAsync();

            // Assert
            contents.Should().Be("contents");
        }

        [Fact]
        public virtual async Task setting_visibility_on_a_file_that_does_not_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.SetVisibility("path.txt", Visibility.Private);

            // Assert
            await action.Should().ThrowAsync<UnableToSetVisibilityException>();
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        public virtual async Task copying_a_file(Visibility visibility)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (sourcePath, contentsToAssert) = await GivenWeHaveAnExistingFile(
                adapter,
                "source.txt",
                "contents to be copied");
            const string destinationPath = "destination.txt";

            // Act
            await adapter.Copy(sourcePath, destinationPath, new Config { { Config.OptionVisibility, visibility } });

            // Assert
            (await adapter.FileExists(destinationPath)).Should().BeTrue();
            (await adapter.Visibility(destinationPath)).Visibility.Should().Be(visibility);
            (await adapter.ReadString(destinationPath)).Should().Be(contentsToAssert);
        }

        [Theory]
        [InlineData(Visibility.Public)]
        [InlineData(Visibility.Private)]
        public virtual async Task moving_a_file(Visibility visibility)
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (sourcePath, contentsToAssert) = await GivenWeHaveAnExistingFile(
                adapter,
                "source.txt",
                "contents to be moved");
            const string destinationPath = "destination.txt";

            // Act
            await adapter.Move(sourcePath, destinationPath, new Config { { Config.OptionVisibility, visibility } });

            // Assert
            (await adapter.FileExists(sourcePath)).Should().BeFalse();
            (await adapter.FileExists(destinationPath)).Should().BeTrue();
            (await adapter.Visibility(destinationPath)).Visibility.Should().Be(visibility);
            (await adapter.ReadString(destinationPath)).Should().Be(contentsToAssert);
        }

        [Fact]
        public virtual async Task reading_a_file_that_does_not_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.Read("path.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToReadFileException>();
        }

        [Fact]
        public virtual async Task moving_a_file_that_does_not_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.Move("source.txt", "destination.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToMoveFileException>();
        }

        [Fact]
        public virtual async Task trying_to_delete_a_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            await adapter.Delete("path.txt");

            // Assert
            (await adapter.FileExists("path.txt")).Should().BeFalse();
        }

        [Fact]
        public virtual async Task checking_if_files_exist()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "some/path.txt";

            // Act
            await adapter.Write(path, new MemoryStream(), new Config());

            // Assert
            (await adapter.FileExists(path)).Should().BeTrue();
        }

        [Fact]
        public virtual async Task fetching_last_modified()
        {
            // Arrange
            var startOfTest = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(1));
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "some/path.txt";

            // Act
            await adapter.Write(path, new MemoryStream(), new Config());

            // Assert
            (await adapter.LastModified(path)).LastModified.Should().BeAfter(startOfTest);
        }

        [Fact]
        public virtual async Task failing_to_read_a_non_existing_file_into_a_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.Read("source.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToReadFileException>();
        }

        [Fact]
        public virtual async Task failing_to_read_a_non_existing_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            Func<Task> action = () => adapter.ReadString("source.txt");

            // Assert
            await action.Should().ThrowAsync<UnableToReadFileException>();
        }

        [Fact]
        public virtual async Task writing_a_file_with_an_empty_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "some/path.txt";

            // Act
            await adapter.Write(path, new MemoryStream(), new Config());

            // Assert
            (await adapter.ReadString(path)).Should().Be(string.Empty);
        }

        [Fact]
        public virtual async Task reading_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, contentsToAssert) = await GivenWeHaveAnExistingFile(adapter, withContents: "contents");

            // Act
            var contents = await adapter.ReadString(path);

            // Assert
            contents.Should().Be(contentsToAssert);
        }

        [Fact]
        public virtual async Task reading_a_file_with_a_stream()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, contentsToAssert) = await GivenWeHaveAnExistingFile(adapter, withContents: "contents");

            // Act
            var stream = await adapter.Read(path);

            // Assert
            (await new StreamReader(stream).ReadToEndAsync()).Should().Be(contentsToAssert);
        }

        [Fact]
        public virtual async Task overwriting_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, _) = await GivenWeHaveAnExistingFile(
                adapter,
                "path.txt",
                "contents",
                new Config { { Config.OptionVisibility, Visibility.Public } });

            // Act
            await adapter.Write(
                path,
                new MemoryStream(Encoding.UTF8.GetBytes("new contents")),
                new Config { { Config.OptionVisibility, Visibility.Private } });

            // Assert
            (await adapter.ReadString(path)).Should().Be("new contents");
            (await adapter.Visibility(path)).Visibility.Should().Be(Visibility.Private);
        }

        [Fact]
        public virtual async Task deleting_a_file()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            var (path, _) = await GivenWeHaveAnExistingFile(adapter);

            // Act
            await adapter.Delete(path);

            // Assert
            (await adapter.FileExists(path)).Should().BeFalse();
        }

        [Fact]
        public virtual async Task listing_contents_shallow()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            await GivenWeHaveAnExistingFile(adapter, "some/0-path.txt");
            await GivenWeHaveAnExistingFile(adapter, "some/1-nested/path.txt");
            await GivenWeHaveAnExistingFile(adapter, "some/1-nested-2/2-nested/path.txt");

            // Act
            var contents = await adapter.ListContents("some", false).ToListAsync();

            // Assert
            contents.Should().HaveCount(3);

            contents.Should().Contain(o => o.Path == "some/1-nested");
            contents.Should().Contain(o => o.Path == "some/0-path.txt");

            contents.First(o => o.Path == "some/1-nested").IsDirectory().Should().BeTrue();
            contents.First(o => o.Path == "some/1-nested-2").IsDirectory().Should().BeTrue();
            contents.First(o => o.Path == "some/0-path.txt").IsFile().Should().BeTrue();
        }

        [Fact]
        public virtual async Task listing_contents_recursive()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            await GivenWeHaveAnExistingDirectory(adapter);
            await GivenWeHaveAnExistingFile(adapter, "path/file.txt");

            // Act
            var contents = await adapter.ListContents("", true).ToListAsync();

            // Assert
            contents.Should().HaveCount(2);
        }

        [Fact]
        public virtual async Task creating_a_directory()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();
            const string path = "path";

            // Act
            await adapter.CreateDirectory(path);

            // Assert
            var contents = await adapter.ListContents("", false).ToListAsync();
            contents.Should().HaveCount(1);
            contents.First().IsDirectory().Should().BeTrue();
            contents.First().Path.Should().Be(path);
        }

        [Fact]
        public virtual async Task copying_a_file_with_collision()
        {
            // Arrange
            var adapter = FilesystemAdapterFactory.Invoke();

            // Act
            await adapter.Write("path.txt", new MemoryStream(Encoding.UTF8.GetBytes("new contents")), new Config());
            await adapter.Write("new-path.txt", new MemoryStream(Encoding.UTF8.GetBytes("contents")), new Config());
            await adapter.Copy("path.txt", "new-path.txt", new Config());

            // Assert
            (await adapter.ReadString("path.txt")).Should().Be("new contents");
        }

        private static async Task<Tuple<string, string>> GivenWeHaveAnExistingFile(
            IFilesystemAdapterAsync adapter,
            string withPath = "path.txt",
            string withContents = "contents",
            Config withConfig = null)
        {
            await adapter.Write(
                withPath,
                new MemoryStream(Encoding.UTF8.GetBytes(withContents)),
                withConfig ?? new Config());

            return new Tuple<string, string>(withPath, withContents);
        }

        private static async Task<Tuple<string>> GivenWeHaveAnExistingDirectory(
            IFilesystemAdapterAsync adapter,
            string withPath = "path",
            Config withConfig = null)
        {
            await adapter.CreateDirectory(withPath, withConfig ?? new Config());

            return new Tuple<string>(withPath);
        }
    }
}