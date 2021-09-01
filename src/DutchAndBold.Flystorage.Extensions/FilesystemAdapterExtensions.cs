using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Extensions
{
    public static class FilesystemAdapterExtensions
    {
        /// <summary>
        /// Writes a string value to the given location.
        /// Because of it's memory inefficient nature, always use <see cref="IFilesystemAdapter.Write(string, Stream, Config)"/> unless you absolutely need a string.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="path">The location of the file.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToWriteFileException"></exception>
        public static void WriteString(
            this IFilesystemAdapter adapter,
            string path,
            string contents,
            Config config = null)
        {
            adapter.Write(path, new MemoryStream(Encoding.Default.GetBytes(contents)), config);
        }

        /// <summary>
        /// Writes a string value to the given location.
        /// Because of it's memory inefficient nature, always use <see cref="IFilesystemAdapter.Write(string, Stream, Config)"/> unless you absolutely need a string.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="path">The location of the file.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToWriteFileException"></exception>
        public static Task WriteString(
            this IFilesystemAdapterAsync adapter,
            string path,
            string contents,
            Config config = null,
            CancellationToken cancellationToken = default)
        {
            return adapter.Write(path, new MemoryStream(Encoding.Default.GetBytes(contents)), config, cancellationToken);
        }

        /// <summary>
        /// Reads the file at path as String.
        /// Because of it's memory inefficient nature, always use <see cref="IFilesystemAdapter.Read(string)"/> unless you absolutely need a string.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToReadFileException"></exception>
        /// <returns>File as stream.</returns>
        public static string ReadString(this IFilesystemAdapter adapter, string path)
        {
            using var stream = adapter.Read(path);
            using var streamReader = new StreamReader(stream);
            var text = streamReader.ReadToEnd();
            return text;
        }

        /// <summary>
        /// Reads the file at path as String.
        /// Because of it's memory inefficient nature, always use <see cref="IFilesystemAdapter.Read(string)"/> unless you absolutely need a string.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="path">The location of the file.</param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToReadFileException"></exception>
        /// <returns>File as stream.</returns>
        public static async Task<string> ReadString(
            this IFilesystemAdapterAsync adapter,
            string path,
            CancellationToken cancellationToken = default)
        {
            await using var stream = await adapter.Read(path, cancellationToken);
            using var streamReader = new StreamReader(stream);
            var text = await streamReader.ReadToEndAsync();
            return text;
        }
    }
}