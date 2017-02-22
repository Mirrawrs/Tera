using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;

namespace Tera.Analytics.Unpacking
{
    /// <summary>
    ///     Unpacks the Data Center's stream.
    /// </summary>
    public class DataCenterUnpacker
    {
        private readonly byte[] iv;
        private readonly byte[] key;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DataCenterUnpacker" /> class.
        /// </summary>
        /// <param name="key">The key that will be used to decrypt the stream.</param>
        /// <param name="iv">The initialization vector that will be used to decrypt the stream.</param>
        public DataCenterUnpacker(byte[] key, byte[] iv)
        {
            this.key = key;
            this.iv = iv;
        }

        /// <summary>
        ///     Decrypts and unpack the Data Center's stream.
        /// </summary>
        /// <param name="stream">The Data Center's stream.</param>
        /// <returns>The unpacked Data Center's stream.</returns>
        public DeflateStream Unpack(Stream stream)
        {
            var rijndael = new RijndaelManaged
            {
                Mode = CipherMode.CFB,
                Key = key,
                IV = iv,
                Padding = PaddingMode.Zeros
            };
            var decryptor = new AutoPaddingCryptoTransform(rijndael.CreateDecryptor());
            var cryptoStream = new CryptoStream(stream, decryptor, CryptoStreamMode.Read);
            ReadZlibHeader(cryptoStream);
            return new DeflateStream(cryptoStream, CompressionMode.Decompress, true);
        }

        private static void ReadZlibHeader(Stream cryptoStream)
        {
            var reader = new BinaryReader(cryptoStream);
            var decompressedSize = reader.ReadInt32();
            const int zlibMagicHeader = 0x9C78;
            if (reader.ReadUInt16() != zlibMagicHeader) throw new Exception("Incorrect key/IV.");
        }
    }
}