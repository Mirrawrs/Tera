using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Tera.Net
{
    internal static class ExtensionMethods
    {
        /// <summary>
        ///     Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the
        ///     number of bytes read. The task completes when the exact number of bytes has been read or when the end of the stream
        ///     has been reached.
        /// </summary>
        /// <param name="value">The stream to read the data from.</param>
        /// <param name="count">The exact number of bytes to read.</param>
        /// <param name="cancellationToken">
        ///     The token to monitor for cancellation requests. The default value is
        ///     <see cref="CancellationToken.None" />.
        /// </param>
        public static async Task<byte[]> ReadExactly(this Stream value, int count,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new byte[count];
            await value.ReadExactly(buffer, 0, count, cancellationToken);
            return buffer;
        }

        /// <summary>
        ///     Asynchronously reads a sequence of bytes from the current stream and advances the position within the stream by the
        ///     number of bytes read. The task completes when the exact number of bytes has been read or when the end of the stream
        ///     has been reached.
        /// </summary>
        /// <param name="value">The stream to read the data from.</param>
        /// <param name="buffer">The buffer to write the data into.</param>
        /// <param name="offset">The byte offset in <paramref name="buffer" /> at which to begin writing data from the stream.</param>
        /// <param name="count">The exact number of bytes to read.</param>
        /// <param name="cancellationToken">
        ///     The token to monitor for cancellation requests. The default value is
        ///     <see cref="CancellationToken.None" />.
        /// </param>
        public static async Task ReadExactly(this Stream value, byte[] buffer, int offset, int count,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var total = 0;
            while (total < count)
            {
                var read = await value.ReadAsync(buffer, offset + total, count - total, cancellationToken);
                if (read == 0) throw new EndOfStreamException();
                total += read;
            }
        }
    }
}