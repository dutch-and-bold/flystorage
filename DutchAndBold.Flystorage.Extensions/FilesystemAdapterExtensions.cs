using System.IO;
using System.Text;
using DutchAndBold.Flystorage.Abstractions;
using DutchAndBold.Flystorage.Abstractions.Exceptions;
using DutchAndBold.Flystorage.Abstractions.Models;

namespace DutchAndBold.Flystorage.Extensions
{
    public static class FilesystemAdapterExtensions
    {
        /// <summary>
        /// Writes a string value to the given location.
        /// Warning: Only use when using a Stream is not possible.
        /// </summary>
        /// <param name="adapter"></param>
        /// <param name="path">The location of the file.</param>
        /// <param name="contents">The string to write to the file.</param>
        /// <param name="config">An optional configuration array.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToWriteFileException"></exception>
        public static void WriteString(this IFilesystemAdapter adapter, string path, string contents, Config config = null)
        {
            adapter.Write(path, new MemoryStream(Encoding.Default.GetBytes(contents)), config);
        }


        /// <summary>
        /// Reads the file at path as String.
        /// Because of it's memory inefficient nature, always use <see cref="Read(string)"/> unless you need a string.
        /// </summary>
        /// <param name="path">The location of the file.</param>
        /// <exception cref="FilesystemException"></exception>
        /// <exception cref="UnableToReadFileException"></exception>
        /// <returns>File as stream.</returns>
        public static string ReadString(this IFilesystemAdapter adapter, string path)
        {
            var stream = new StreamReader(adapter.Read(path));
            var text = stream.ReadToEnd();
            stream.Close();
            return text;
        }
    }
}