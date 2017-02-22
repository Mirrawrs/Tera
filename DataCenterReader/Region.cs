using System.Collections.Generic;
using Lotus.Serialization.Attributes;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents a collection of items partitioned in buckets.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public class Region<TItem> : LazyCollection<TItem>
    {
        /// <summary>
        ///     Gets the buckets contained in this region.
        /// </summary>
        [Serialize(0)]
        public List<Bucket<TItem>> Buckets { get; private set; }

        /// <summary>
        ///     Gets the address of an item given its index.
        /// </summary>
        /// <param name="regionIndex">The item's index.</param>
        /// <returns>The item's address.</returns>
        public override int GetAddress(DataCenterIndex regionIndex)
        {
            var bucket = Buckets[regionIndex.BucketIndex];
            return (int) bucket.FirstValueAddress + regionIndex.ItemIndex * bucket.ValueSize;
        }
    }
}