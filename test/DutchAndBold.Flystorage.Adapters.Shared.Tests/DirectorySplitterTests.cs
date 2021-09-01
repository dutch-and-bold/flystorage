using System.Linq;
using FluentAssertions;
using Xunit;

namespace DutchAndBold.Flystorage.Adapters.Shared.Tests
{
    public class DirectorySplitterTests
    {
        [Fact]
        public void SplitFromPath_WithHappyFlow_IsSuccessful()
        {
            // Arrange
            var directorySplitter = new DirectorySplitter();

            // Act
            var paths = directorySplitter.SplitFromPath("/test/is/foo/bar/").ToList();

            // Assert
            paths.Should().HaveCount(5);
            paths.Should().Contain("/test/is/foo/bar");
            paths.Should().Contain("/test/is/foo");
            paths.Should().Contain("/test/is");
            paths.Should().Contain("/test");
            paths.Should().Contain("/");
        }

        [Fact]
        public void SplitFromPath_WithNoRoot_IsSuccessful()
        {
            // Arrange
            var directorySplitter = new DirectorySplitter();

            // Act
            var paths = directorySplitter.SplitFromPath("test/is/foo/bar/").ToList();

            // Assert
            paths.Should().HaveCount(4);
            paths.Should().Contain("test/is/foo/bar");
            paths.Should().Contain("test/is/foo");
            paths.Should().Contain("test/is");
            paths.Should().Contain("test");
        }

        [Fact]
        public void SplitFromPath_WithFile_IsSuccessful()
        {
            // Arrange
            var directorySplitter = new DirectorySplitter();

            // Act
            var paths = directorySplitter.SplitFromPath("/test/is/foo/bar").ToList();

            // Assert
            paths.Should().HaveCount(4);
            paths.Should().Contain("/test/is/foo");
            paths.Should().Contain("/test/is");
            paths.Should().Contain("/test");
            paths.Should().Contain("/");
        }

        [Fact]
        public void SplitFromPath_WithSingleDirectoryPath_IsSuccessful()
        {
            // Arrange
            var directorySplitter = new DirectorySplitter();

            // Act
            var paths = directorySplitter.SplitFromPath("path").ToList();

            // Assert
            paths.Should().HaveCount(1);
            paths.Should().Contain("path");
        }

        [Fact]
        public void SplitFromPath_WithSingleDirectoryPathWithRoot_IsSuccessful()
        {
            // Arrange
            var directorySplitter = new DirectorySplitter();

            // Act
            var paths = directorySplitter.SplitFromPath("/path/").ToList();

            // Assert
            paths.Should().HaveCount(2);
            paths.Should().Contain("/path");
            paths.Should().Contain("/");
        }

        [Fact]
        public void SplitFromPath_WithSingleFile_IsSuccessful()
        {
            // Arrange
            var directorySplitter = new DirectorySplitter();

            // Act
            var paths = directorySplitter.SplitFromPath("/path").ToList();

            // Assert
            paths.Should().HaveCount(1);
            paths.Should().Contain("/");
        }

        [Fact]
        public void SplitFromPaths_WithHappyFlow_IsSuccessful()
        {
            // Arrange
            var directorySplitter = new DirectorySplitter();

            // Act
            var paths = directorySplitter.SplitFromPaths("/test/is/foo/bar/", "/test/is/bar/foo/").ToList();

            // Assert
            paths.Should().HaveCount(7);
            paths.Should().Contain("/test/is/foo/bar");
            paths.Should().Contain("/test/is/foo");
            paths.Should().Contain("/test/is/bar/foo");
            paths.Should().Contain("/test/is/bar");
            paths.Should().Contain("/test/is");
            paths.Should().Contain("/test");
            paths.Should().Contain("/");
        }
    }
}