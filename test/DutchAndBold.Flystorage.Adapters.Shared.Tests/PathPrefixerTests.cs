using FluentAssertions;
using Xunit;

namespace DutchAndBold.Flystorage.Adapters.Shared.Tests
{
    public class PathPrefixerTests
    {
        [Fact]
        public void path_prefixing_with_a_prefix()
        {
            // Arrange
            var prefixer = new PathPrefixer("prefix");

            // Act
            var prefixedPath = prefixer.PrefixPath("some/path.txt");

            // Assert
            prefixedPath.Should().Be("prefix/some/path.txt");
        }

        [Fact]
        public void path_stripping_with_a_prefix()
        {
            // Arrange
            var prefixer = new PathPrefixer("prefix");

            // Act
            var strippedPath = prefixer.StripPrefix("prefix/some/path.txt");

            // Assert
            strippedPath.Should().Be("some/path.txt");
        }

        [Theory]
        [InlineData("/", '/', "path.txt", "/path.txt")] // Unix
        [InlineData("\\", '\\', "path.txt", "\\path.txt")] // Windows
        public void an_absolute_root_path_is_supported(
            string rootPath,
            char separator,
            string path,
            string expectedPath)
        {
            // Arrange
            var prefixer = new PathPrefixer(rootPath, separator);

            // Act
            var prefixedPath = prefixer.PrefixPath(path);

            // Assert
            prefixedPath.Should().Be(expectedPath);
        }

        [Fact]
        public void path_stripping_is_reversable()
        {
            // Arrange
            var prefixer = new PathPrefixer("prefix");

            // Act
            var strippedPath = prefixer.StripPrefix("prefix/some/path.txt");
            var prefixedPath = prefixer.PrefixPath("some/path.txt");

            // Assert
            prefixer.PrefixPath(strippedPath).Should().Be("prefix/some/path.txt");
            prefixer.StripPrefix(prefixedPath).Should().Be("some/path.txt");
        }

        [Fact]
        public void prefixing_without_a_prefix()
        {
            // Arrange
            var prefixer = new PathPrefixer("");

            // Act
            var path = prefixer.PrefixPath("path/to/prefix.txt");
            var pathFromRoot = prefixer.PrefixPath("/path/to/prefix.txt");

            // Assert
            path.Should().Be("path/to/prefix.txt");
            pathFromRoot.Should().Be("path/to/prefix.txt");
        }

        [Fact]
        public void prefixing_for_a_directory()
        {
            // Arrange
            var prefixer = new PathPrefixer("/prefix");

            // Act
            var path = prefixer.PrefixDirectoryPath("something");
            var emptyPath = prefixer.PrefixDirectoryPath("");

            // Assert
            path.Should().Be("/prefix/something/");
            emptyPath.Should().Be("/prefix/");
        }

        [Fact]
        public void prefixing_for_a_directory_without_a_prefix()
        {
            // Arrange
            var prefixer = new PathPrefixer("");

            // Act
            var path = prefixer.PrefixDirectoryPath("something");
            var emptyPath = prefixer.PrefixDirectoryPath("");

            // Assert
            path.Should().Be("something/");
            emptyPath.Should().Be("");
        }

        [Fact]
        public void stripping_a_directory_prefix()
        {
            // Arrange
            var prefixer = new PathPrefixer("/something/");

            // Act
            var path = prefixer.StripDirectoryPrefix("/something/this/");
            var pathWithEndingBackslash = prefixer.StripDirectoryPrefix("/something/and-this\\");

            // Assert
            path.Should().Be("this");
            pathWithEndingBackslash.Should().Be("and-this");
        }

        [Fact]
        public void stripping_a_root_directory_prefix()
        {
            // Arrange
            var prefixer = new PathPrefixer("/something/");

            // Act
            var path = prefixer.StripDirectoryPrefix("/something/this/");
            var pathWithEndingBackslash = prefixer.StripDirectoryPrefix("/something/and-this\\");

            // Assert
            path.Should().Be("this");
            pathWithEndingBackslash.Should().Be("and-this");
        }
    }
}