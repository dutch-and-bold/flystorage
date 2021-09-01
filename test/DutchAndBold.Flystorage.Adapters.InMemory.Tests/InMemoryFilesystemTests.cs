using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Shared;
using DutchAndBold.Flystorage.Extensions;
using FluentAssertions;
using Xunit;

namespace DutchAndBold.Flystorage.Adapters.InMemory.Tests
{
    public class InMemoryFilesystemTests
    {
        [Fact]
        public void getting_mimetype_on_a_non_existing_file()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.MimeType("path.txt");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void getting_last_modified_on_a_non_existing_file()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.LastModified("path.txt");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Fact]
        public void getting_file_size_on_a_non_existing_file()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.FileSize("path.txt");

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }

        [Theory]
        [InlineData("/path.txt", "/path.txt")]
        [InlineData("/path.txt", "path.txt")]
        public void deleting_a_file(string path, string pathToDelete)
        {
            // Arrange
            var files = new Dictionary<string, InMemoryFile> { { path, new InMemoryFile(new MemoryStream()) } };

            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            adapter.Delete(pathToDelete);

            // Assert
            files.Should().NotContainKey(path);
        }

        [Fact]
        public void deleting_a_directory()
        {
            // Arrange
            var files = new Dictionary<string, InMemoryFile>
            {
                { "/a/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/b.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/b/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/b/c/path.txt", new InMemoryFile(new MemoryStream()) }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            adapter.DeleteDirectory("a/b");

            // Assert
            files.Should().ContainKey("/a/path.txt");
            files.Should().ContainKey("/a/b.txt");
            files.Should().NotContainKey("/a/b/path.txt");
            files.Should().NotContainKey("/a/b/c/path.txt");
        }

        [Fact]
        public void creating_a_directory_does_nothing()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.CreateDirectory("something", new Config());

            // Assert
            action.Should().NotThrow();
        }

        [Fact]
        public void writing_with_a_stream_and_reading_a_file()
        {
            // Arrange
            const string path = "/test.txt";
            const string contents = "contents";
            var files = new Dictionary<string, InMemoryFile>();
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            adapter.Write(path, new MemoryStream(Encoding.UTF8.GetBytes(contents)), new Config());

            // Assert
            files.Should().ContainKey(path);
            new StreamReader(files[path].Contents).ReadToEnd().Should().Be(contents);
        }

        [Fact]
        public void reading_a_stream()
        {
            // Arrange
            const string path = "/test.txt";
            const string contents = "contents";
            var files = new Dictionary<string, InMemoryFile>()
            {
                { path, new InMemoryFile(new MemoryStream(Encoding.UTF8.GetBytes(contents))) }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            var stream = adapter.Read(path);

            // Assert
            new StreamReader(stream).ReadToEnd().Should().Be(contents);
        }

        [Fact]
        public void reading_a_non_existing_file()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.ReadString("path.txt");

            // Assert
            action.Should().Throw<UnableToReadFileException>();
        }

        [Fact]
        public void stream_reading_a_non_existing_file()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.Read("path.txt");

            // Assert
            action.Should().Throw<UnableToReadFileException>();
        }

        [Theory]
        [InlineData("/a")]
        [InlineData("/a/")]
        [InlineData("a")]
        public void listing_all_files(string listPath)
        {
            // Arrange
            var files = new Dictionary<string, InMemoryFile>
            {
                { "/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/b/c/path.txt", new InMemoryFile(new MemoryStream()) }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            var listing = adapter.ListContents(listPath, true).ToList();

            // Assert
            listing.Should().HaveCount(4);

            listing.Any(o => o.Path == "a/path.txt").Should().BeTrue();
            listing.First(o => o.Path == "a/path.txt").IsFile().Should().BeTrue();

            listing.Any(o => o.Path == "a/b/c/path.txt").Should().BeTrue();
            listing.First(o => o.Path == "a/b/c/path.txt").IsFile().Should().BeTrue();

            listing.Any(o => o.Path == "a/b").Should().BeTrue();
            listing.First(o => o.Path == "a/b").IsDirectory().Should().BeTrue();

            listing.Any(o => o.Path == "a/b/c").Should().BeTrue();
            listing.First(o => o.Path == "a/b/c").IsDirectory().Should().BeTrue();
        }

        [Theory]
        [InlineData("/a")]
        [InlineData("/a/")]
        [InlineData("a")]
        public void listing_non_recursive(string listPath)
        {
            // Arrange
            var files = new Dictionary<string, InMemoryFile>
            {
                { "/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/b/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/a/b/c/path.txt", new InMemoryFile(new MemoryStream()) }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            var listing = adapter.ListContents(listPath, false).ToList();

            // Assert
            listing.Should().HaveCount(2);

            listing.Any(o => o.Path == "a/path.txt").Should().BeTrue();
            listing.First(o => o.Path == "a/path.txt").IsFile().Should().BeTrue();

            listing.Any(o => o.Path == "a/b").Should().BeTrue();
            listing.First(o => o.Path == "a/b").IsDirectory().Should().BeTrue();
        }

        [Fact]
        public void moving_a_file_successfully()
        {
            // Arrange
            var files = new Dictionary<string, InMemoryFile>()
            {
                { "/path.txt", new InMemoryFile(new MemoryStream()) }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            adapter.Move("path.txt", "new-path.txt", new Config());

            // Assert
            files.Should().ContainKey("/new-path.txt");
        }

        [Fact]
        public void moving_a_file_with_collision()
        {
            // Arrange
            var files = new Dictionary<string, InMemoryFile>()
            {
                { "/path.txt", new InMemoryFile(new MemoryStream()) },
                { "/new-path.txt", new InMemoryFile(new MemoryStream()) }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            Action action = () => adapter.Move("path.txt", "new-path.txt", new Config());

            // Assert
            action.Should().Throw<UnableToMoveFileException>();
        }

        [Fact]
        public void trying_to_move_a_non_existing_file()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.Move("path.txt", "new-path.txt", new Config());

            // Assert
            action.Should().Throw<UnableToMoveFileException>();
        }

        [Fact]
        public void copying_a_file_successfully()
        {
            // Arrange
            var files = new Dictionary<string, InMemoryFile>()
            {
                { "/path.txt", new InMemoryFile(new MemoryStream()) }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            adapter.Copy("path.txt", "new-path.txt", new Config());

            // Assert
            files.Should().ContainKey("/path.txt");
            files.Should().ContainKey("/new-path.txt");
        }

        [Fact]
        public void trying_to_copy_a_non_existing_file()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            Action action = () => adapter.Copy("path.txt", "new-path.txt", new Config());

            // Assert
            action.Should().Throw<UnableToCopyFileException>();
        }

        [Fact]
        public void not_listing_directory_placeholders()
        {
            // Arrange
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter());

            // Act
            adapter.CreateDirectory("directory", new Config());
            var contents = adapter.ListContents("", true);

            // Assert
            contents.Should().ContainSingle();
        }

        [Fact]
        public void checking_for_metadata()
        {
            // Arrange
            var date = new DateTimeOffset(2021, 01, 01, 01, 01, 01, TimeSpan.Zero);
            const string path = "/flysystem.svg";
            var files = new Dictionary<string, InMemoryFile>
            {
                { path, new InMemoryFile(File.OpenRead("TestFiles/flysystem.svg")) { LastModified = date } }
            };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            var fileExists = adapter.FileExists(path);
            var fileSize = adapter.FileSize(path).FileSize;
            var lastModified = adapter.LastModified(path).LastModified;
            var mimeType = adapter.MimeType(path).MimeType;

            // Assert
            fileExists.Should().BeTrue();
            fileSize.Should().Be(754);
            lastModified.Should().Be(date);
            mimeType.Should().StartWith("image/svg");
        }

        [Fact]
        public void fetching_unknown_mime_type_of_a_file()
        {
            // Arrange
            const string path = "/test.000xyz";
            var files = new Dictionary<string, InMemoryFile> { { path, new InMemoryFile(new MemoryStream()) } };
            var adapter = new InMemoryFilesystemAdapter(new DirectorySplitter(), files);

            // Act
            Action action = () => adapter.MimeType(path);

            // Assert
            action.Should().Throw<UnableToRetrieveMetadataException>();
        }
    }
}