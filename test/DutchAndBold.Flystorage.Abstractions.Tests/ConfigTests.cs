using System.Collections.Generic;
using DutchAndBold.Flystorage.Abstractions.Models;
using FluentAssertions;
using Xunit;

namespace DutchAndBold.Flystorage.Abstractions.Tests
{
    public class ConfigTests
    {
        [Fact]
        public void a_config_object_exposes_passed_options()
        {
            // Arrange
            var config = new Config(new Dictionary<string, object>{{"option", "value"}});

            // Assert
            config.Get<string>("option").Should().Be("value");
        }

        [Fact]
        public void a_config_object_returns_a_default_value()
        {
            // Arrange
            var config = new Config();

            // Assert
            config.Get<string>("option").Should().BeNull();
            config.Get("option", "default").Should().Be("default");
        }

        [Fact]
        public void extending_a_config_with_options()
        {
            // Arrange
            var config = new Config(new Dictionary<string, object>{{"option", "value"}, {"first", 1}});

            // Act
            var extended = config.Extend(new Dictionary<string, object>{{"option", "overwritten"}, {"second", 2}});

            // Assert
            extended.Get<string>("option").Should().Be("overwritten");
            extended.Get<int>("first").Should().Be(1);
            extended.Get<int>("second").Should().Be(2);
        }

        [Fact]
        public void extending_with_defaults()
        {
            // Arrange
            var config = new Config(new Dictionary<string, object>{{"option", "set"}});

            // Act
            var withDefaults = config.WithDefaults(new Dictionary<string, object>{{"option", "default"}, {"other", "default"}});

            // Assert
            withDefaults.Get<string>("option").Should().Be("set");
            withDefaults.Get<string>("other").Should().Be("default");
        }
    }
}