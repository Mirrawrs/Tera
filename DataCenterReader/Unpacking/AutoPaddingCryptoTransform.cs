using System;
using System.Security.Cryptography;

namespace Tera.Analytics.Unpacking
{
    /// <summary>
    ///     Grows the final block by padding it with zeros if its size is smaller than the block size.
    /// </summary>
    /// <remarks>
    ///     The Data Center isn't padded and its size is rarely (if ever) a multiple of the key length.
    /// </remarks>
    public class AutoPaddingCryptoTransform : ICryptoTransform
    {
        private readonly ICryptoTransform transform;

        public AutoPaddingCryptoTransform(ICryptoTransform symmetricAlgoTransform)
        {
            transform = symmetricAlgoTransform;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            Array.Resize(ref inputBuffer, transform.InputBlockSize);
            return transform.TransformFinalBlock(inputBuffer, 0, inputBuffer.Length);
        }

        public void Dispose() => transform.Dispose();
        public bool CanReuseTransform => transform.CanReuseTransform;
        public bool CanTransformMultipleBlocks => transform.CanTransformMultipleBlocks;
        public int InputBlockSize => transform.InputBlockSize;
        public int OutputBlockSize => transform.OutputBlockSize;

        public int TransformBlock(
            byte[] inputBuffer,
            int inputOffset,
            int inputCount,
            byte[] outputBuffer,
            int outputOffset)
            => transform.TransformBlock(inputBuffer, inputOffset, inputCount, outputBuffer, outputOffset);
    }
}