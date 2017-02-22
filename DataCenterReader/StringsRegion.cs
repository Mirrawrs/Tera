using Lotus.Serialization.Attributes;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents a region of characters which can be read into strings.
    /// </summary>
    public class StringsRegion : LazyCollection<string>
    {
        [Serialize(0)]
        public Region<char> Characters { get; private set; }

        /// <summary>
        ///     Gets the address of the first character of the string at the specified index.
        /// </summary>
        /// <param name="regionIndex">The index of the first character of the string.</param>
        /// <returns>The address of the first character of the string.</returns>
        public override int GetAddress(DataCenterIndex regionIndex)
        {
            return Characters.GetAddress(regionIndex);
        }
    }
}