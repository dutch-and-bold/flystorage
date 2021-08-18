using System.Collections.Generic;
using DutchAndBold.Flystorage.Abstractions.Models;
using FluentAssertions;
using Xunit;

namespace DutchAndBold.Flystorage.Adapters.Shared.Tests
{
    public class PortableVisibilityConverterTests
    {
        [Fact]
        public void determining_visibility_for_a_file()
        {
            // Arrange
            var interpreter = new PortableVisibilityConverter();
            interpreter.ForFile(Visibility.Public).Should().Be(0644);
            interpreter.ForFile(Visibility.Private).Should().Be(0600);
        }

        [Fact]
        public void determining_visibility_for_a_directory()
        {
            // Arrange
            var interpreter = new PortableVisibilityConverter();
            interpreter.ForDirectory(Visibility.Public).Should().Be(0755);
            interpreter.ForDirectory(Visibility.Private).Should().Be(0700);
        }

        [Fact]
        public void inversing_for_a_file()
        {
            // Arrange
            var interpreter = new PortableVisibilityConverter();
            interpreter.InverseForFile(0644).Should().Be(Visibility.Public);
            interpreter.InverseForFile(0600).Should().Be(Visibility.Private);
            interpreter.InverseForFile(0404).Should().Be(Visibility.Public);
        }

        [Fact]
        public void inversing_for_a_directory()
        {
            // Arrange
            var interpreter = new PortableVisibilityConverter();
            interpreter.InverseForDirectory(0755).Should().Be(Visibility.Public);
            interpreter.InverseForDirectory(0700).Should().Be(Visibility.Private);
            interpreter.InverseForDirectory(0404).Should().Be(Visibility.Public);
        }

        [Fact]
        public void determining_default_for_directories()
        {
            // Arrange
            var interpreter = new PortableVisibilityConverter();
            interpreter.DefaultForDirectories().Should().Be(0700);

            var interpreter2 = new PortableVisibilityConverter(0644, 0600, 0755, 0700, Visibility.Public);
            interpreter2.DefaultForDirectories().Should().Be(0755);
        }

        [Fact]
        public void creating_from_array()
        {
            var interpreter = PortableVisibilityConverter.FromArray(
                new Dictionary<string, Dictionary<string, int>>()
                {
                    {
                        "file", new Dictionary<string, int>()
                        {
                            { "public", 0640 },
                            { "private", 0604 }
                        }
                    },
                    {
                        "dir", new Dictionary<string, int>()
                        {
                            { "public", 0740 },
                            { "private", 7604 }
                        }
                    }
                });

            interpreter.ForFile(Visibility.Public).Should().Be(0640);
            interpreter.ForFile(Visibility.Private).Should().Be(0604);

            interpreter.ForDirectory(Visibility.Public).Should().Be(0740);
            interpreter.ForDirectory(Visibility.Private).Should().Be(7604);
        }
    }
}