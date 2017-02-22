using Lotus.Serialization.Attributes;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents information about a string.
    /// </summary>
    public struct StringMetadata
    {
        [Serialize(0)]
        public int Unknown { get; private set; }

        /// <summary>
        ///     Gets the length in characters of the string, including the null character.
        /// </summary>
        [Serialize(1)]
        public int Length { get; private set; }

        [Serialize(2)]
        public int Unknown1 { get; private set; }

        /// <summary>
        ///     Gets the address of the first character of the string.
        /// </summary>
        [Serialize(3)]
        public DataCenterIndex StringAddress { get; private set; }
    }
}