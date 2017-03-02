using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Tera.Net.Cryptography
{
    /// <summary>
    ///     Provides methods to perform transformations following the TERA cryptographic algorithm.
    /// </summary>
    internal class TeraCryptoTransform
    {
        private readonly KeyBlockGenerator g1;
        private readonly KeyBlockGenerator g2;
        private readonly KeyBlockGenerator g3;
        private uint currentKeyBlock;
        private byte remaining;

        /// <summary>
        ///     Initializes a new instance of the <see cref="TeraCryptoTransform" /> class using the specified key.
        /// </summary>
        /// <param name="key">The key to initialize the transform with.</param>
        public TeraCryptoTransform(byte[] key)
        {
            const int aSize = 55;
            const int bSize = 57;
            const int cSize = 58;

            var repeatedKey = new byte[(aSize + bSize + cSize) * sizeof(uint)];
            for (var i = 0; i < repeatedKey.Length; i++) repeatedKey[i] = key[i % key.Length];
            repeatedKey[0] = (byte) key.Length;

            using (var sha1 = new SHA1Fake())
            {
                const int hashSize = 20;
                for (var i = 0; i < repeatedKey.Length; i += hashSize)
                {
                    var hash = sha1.ComputeHash(repeatedKey);
                    Buffer.BlockCopy(hash, 0, repeatedKey, i, hashSize);
                }
            }

            var uintsKey = new uint[repeatedKey.Length / 4];
            Buffer.BlockCopy(repeatedKey, 0, uintsKey, 0, repeatedKey.Length);
            g1 = new KeyBlockGenerator(uintsKey.Take(aSize).ToArray(), 31);
            g2 = new KeyBlockGenerator(uintsKey.Skip(aSize).Take(bSize).ToArray(), 50);
            g3 = new KeyBlockGenerator(uintsKey.Skip(aSize + bSize).Take(cSize).ToArray(), 39);
        }

        /// <summary>
        /// Transforms the specified region of the input byte array and copies the resulting transform to the specified region of the output byte array.
        /// </summary>
        /// <param name="inputBuffer">The input for which to compute the transform. </param>
        /// <param name="inputOffset">The offset into the input byte array from which to begin using data. </param>
        /// <param name="inputCount">The number of bytes in the input byte array to use as data. </param>
        /// <param name="outputBuffer">The output to which to write the transform. </param>
        /// <param name="outputOffset">The offset into the output byte array from which to begin writing data. </param>
        public void Transform(
            byte[] inputBuffer,
            int inputOffset,
            int inputCount,
            byte[] outputBuffer,
            int outputOffset)
        {
            for (var i = 0; i < inputCount; i++)
            {
                if (remaining == 0)
                {
                    var a = g1.InOverflow;
                    var b = g2.InOverflow;
                    var c = g3.InOverflow;
                    //True if at least two out of three are in overflow state.
                    var genOverflow = a && b || c && (a || b);
                    //Advance the ones with state equal to the predominant one.
                    if (a == genOverflow) g1.Advance();
                    if (b == genOverflow) g2.Advance();
                    if (c == genOverflow) g3.Advance();
                    currentKeyBlock = g1.Value ^ g2.Value ^ g3.Value;
                    remaining = 4;
                }
                outputBuffer[outputOffset + i] = (byte) (inputBuffer[inputOffset + i] ^ currentKeyBlock);
                currentKeyBlock >>= 8;
                remaining--;
            }
        }

        private class KeyBlockGenerator
        {
            private readonly uint[] subKey;
            private uint position1;
            private uint position2;

            public KeyBlockGenerator(uint[] subKey, uint position2)
            {
                this.subKey = subKey;
                this.position2 = position2;
            }

            public uint Value { get; private set; }

            public bool InOverflow { get; private set; }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Advance()
            {
                var n1 = subKey[position1++ % subKey.Length];
                var n2 = subKey[position2++ % subKey.Length];
                Value = n1 + n2;
                InOverflow = Math.Min(n1, n2) > Value;
            }
        }
    }
}