using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Models;
using DutchAndBold.Flystorage.Adapters.Local.FilePermissionStrategies;
using DutchAndBold.Flystorage.Adapters.Shared;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Xunit;

namespace DutchAndBold.Flystorage.Adapters.Local.WebServer.Tests
{
    public class WebserverFileAccessTests : IDisposable
    {
        private const string Url = "http://*:5599";

        private static readonly string StaticFilesRoot =
            $"{Environment.CurrentDirectory}{Path.DirectorySeparatorChar}wwwroot";

        private readonly IWebHostBuilder _hostBuilder;

        private readonly IFilesystemAdapter _adapter;

        private readonly HttpClient _client;

        public WebserverFileAccessTests()
        {
            DeleteDirectoryIfExists(StaticFilesRoot);

            var webBuilder = new WebHostBuilder();

            _hostBuilder = webBuilder
                .Configure(c => c.UseStaticFiles())
                .UseKestrel();

            _adapter = new LocalFilesystemAdapter(
                new PathPrefixer(StaticFilesRoot, Path.DirectorySeparatorChar),
                new UnixFilePermissionStrategy(filePermissionsPrivate: 0));

            _client = new HttpClient() { BaseAddress = new Uri(Url.Replace("*", "127.0.0.1")) };
        }

        public void Dispose()
        {
            DeleteDirectoryIfExists(StaticFilesRoot);
        }

        [Fact]
        public async Task Get_FileWithDefaultVisibility_ShouldReturnContents()
        {
            // Act
            _adapter.Write("test.txt", "Used to test file access.");
            using var host = _hostBuilder.Start(Url);
            var result = await _client.GetAsync("test.txt");

            // Assert
            result.StatusCode.Should().Be(HttpStatusCode.OK);
            (await result.Content.ReadAsStringAsync()).Should().Be("Used to test file access.");
        }

        [Fact()]
        public async Task Get_FileWithPrivateVisibility_ShouldNotReturnContents()
        {
            // Act
            _adapter.Write(
                "test.txt",
                "Used to test file access.",
                new Config(new Dictionary<string, object> { { "visibility", Visibility.Private } }));
            using var host = _hostBuilder.Start(Url);
            var result = await _client.GetAsync("test.txt");

            // Assert
            (await result.Content.ReadAsStringAsync()).Should().Be(string.Empty);
        }

        private static void DeleteDirectoryIfExists(string fullPath)
        {
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, true);
            }
        }
    }
}