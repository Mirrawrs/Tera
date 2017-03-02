using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Tera.Net.Cryptography
{
    /// <summary>
    ///     Defines a stream that transforms data following TERA's cryptographic algorithm.
    /// </summary>
    /// <remarks>
    ///     The reason why <see cref="TeraCryptoStream" /> doesn't derive from (or internally use)
    ///     <see cref="System.Security.Cryptography.CryptoStream" /> is that data is never partitioned in blocks larger than
    ///     one byte. Therefore, the two <see cref="System.Security.Cryptography.ICryptoTransform" /> instances would operate
    ///     on single-byte buffers, resulting in decreased performance. Instead, defining an arbitrarily large block would
    ///     cause the stream to block until the whole block is processed (which may never happen). Alternatively, using
    ///     <see cref="System.Security.Cryptography.CryptoStream.FlushFinalBlock" /> to manually indicate that the final block
    ///     should be processed will cause the underlying stream to close.
    /// </remarks>
    internal class TeraCryptoStream : Stream
    {
        private readonly TeraCryptoTransform decryptor;
        private readonly TeraCryptoTransform encryptor;
        private readonly Stream stream;
        private byte[] outputBuffer = new byte[ushort.MaxValue];

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeraCryptoStream" /> class with the specified underlying stream and
        ///     keys.
        /// </summary>
        /// <param name="stream">The cipher stream to read from and write to.</param>
        /// <param name="clientKey1">The first client key.</param>
        /// <param name="clientKey2">The second client key.</param>
        /// <param name="serverKey1">The first server key.</param>
        /// <param name="serverKey2">The second server key.</param>
        public TeraCryptoStream(
            Stream stream,
            byte[] clientKey1,
            byte[] clientKey2,
            byte[] serverKey1,
            byte[] serverKey2)
        {
            this.stream = stream;

            byte[] Xor(byte[] key1, byte[] key2)
            {
                return key1.Zip(key2, (b1, b2) => (byte) (b1 ^ b2)).ToArray();
            }

            byte[] Rol(byte[] key, int positions)
            {
                return key.Skip(positions).Concat(key.Take(positions)).ToArray();
            }

            var decryptKey = Xor(Rol(clientKey2, 29), Xor(Rol(serverKey1, 61), clientKey1));
            encryptor = new TeraCryptoTransform(decryptKey);
            var encryptKey = new byte[decryptKey.Length];
            encryptor.Transform(Rol(serverKey2, 87), 0, encryptKey.Length, encryptKey, 0);
            decryptor = new TeraCryptoTransform(encryptKey);
        }

        public override bool CanRead => stream.CanRead;
        public override bool CanSeek => stream.CanSeek;
        public override bool CanWrite => stream.CanWrite;
        public override long Length => stream.Length;

        public override long Position
        {
            get => stream.Position;
            set => stream.Position = value;
        }

        public override void Flush()
        {
            stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            stream.SetLength(value);
        }

        /// <summary>
        ///     Initializes a new <see cref="TeraCryptoStream" /> with two empty client keys and two server keys following the TERA
        ///     handshake protocol.
        /// </summary>
        /// <param name="stream">The stream on which the keys will be shared.</param>
        /// <returns>The initialized <see cref="TeraCryptoStream" />.</returns>
        public static async Task<TeraCryptoStream> Initialize(Stream stream)
        {
            const int keyLength = 128;
            var clientKey1 = new byte[keyLength];
            var clientKey2 = new byte[keyLength];
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            var cancellationToken = cts.Token;
            try
            {
                var hello = await stream.ReadExactly(sizeof(int), cancellationToken);
                if (hello[0] != 1) throw new Exception("Not a TERA server.");
                await stream.WriteAsync(clientKey1, 0, clientKey1.Length, cancellationToken);
                var serverKey1 = await stream.ReadExactly(keyLength, cancellationToken);
                await stream.WriteAsync(clientKey2, 0, clientKey2.Length, cancellationToken);
                var serverKey2 = await stream.ReadExactly(keyLength, cancellationToken);
                return new TeraCryptoStream(stream, clientKey1, clientKey2, serverKey1, serverKey2);
            }
            catch (TaskCanceledException)
            {
                throw new Exception("Timeout in the handshake.");
            }
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = stream.Read(buffer, offset, count);
            decryptor.Transform(buffer, offset, read, buffer, offset);
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (outputBuffer.Length < count) outputBuffer = new byte[outputBuffer.Length * 2];
            encryptor.Transform(buffer, offset, count, outputBuffer, 0);
            stream.Write(outputBuffer, 0, count);
        }

#if NET462

        //Providing custom asynchronous read/write implementation to prevent deadlocks with duplex stream.
        //https://www.codeproject.com/Tips/575618/Avoiding-Deadlocks-with-System-IO-Stream-BeginRead
        //TODO: find a solution compatible with .NET Standard.

        public override IAsyncResult BeginRead(
            byte[] buffer,
            int offset,
            int count,
            AsyncCallback callback,
            object state)
        {
            ReadDelegate read = Read;
            return read.BeginInvoke(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            var result = (System.Runtime.Remoting.Messaging.AsyncResult) asyncResult;
            var caller = (ReadDelegate) result.AsyncDelegate;
            return caller.EndInvoke(asyncResult);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset,
            int count, AsyncCallback callback, object state)
        {
            WriteDelegate write = Write;
            return write.BeginInvoke(buffer, offset, count, callback, state);
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            var result = (System.Runtime.Remoting.Messaging.AsyncResult) asyncResult;
            var caller = (WriteDelegate) result.AsyncDelegate;
            caller.EndInvoke(asyncResult);
        }

        private delegate int ReadDelegate(byte[] buffer, int offset, int count);

        private delegate void WriteDelegate(byte[] buffer, int offset, int count);

#endif
    }
}