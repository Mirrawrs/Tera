using Lotus.Serialization.Attributes;
using Tera.Analytics.DirectiveSelectors;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents a typed collection. Its size in bytes is determined by <see cref="Capacity" /> multiplied by
    ///     <see cref="ValueSize" />. If <see cref="Count" /> is smaller than <see cref="Capacity" /> the memory region will be
    ///     zero-padded.
    /// </summary>
    public class Bucket<TItem>
    {
        /// <summary>
        ///     Gets the total number of elements that the bucket can hold.
        /// </summary>
        [Serialize(0)]
        public int Capacity { get; private set; }

        /// <summary>
        ///     Gets the number of elements contained in the bucket.
        /// </summary>
        [Serialize(1)]
        public int Count { get; private set; }

        /// <summary>
        ///     Gets the address of the first item.
        /// </summary>
        /// <remarks>
        ///     Instead of loading all the items in memory, this value acts as a base to calculate the address of an item given
        ///     its index, allowing lazy reading.
        /// </remarks>
        [CurrentAddress]
        [Serialize(2)]
        public long FirstValueAddress { get; private set; }

        /// <summary>
        ///     Gets or sets the size in bytes of one item. Set from the <see cref="DataCenterReader" />.
        /// </summary>
        public int ValueSize { get; internal set; }
    }
}