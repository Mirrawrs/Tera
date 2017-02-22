using Lotus.Serialization.Attributes;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents an index used to access elements in a region.
    /// </summary>
    public struct DataCenterIndex
    {
        /// <summary>
        ///     Gets or sets the index of the bucket.
        /// </summary>
        [Serialize(0)]
        public ushort BucketIndex { get; set; }

        /// <summary>
        ///     Gets or sets the index of the item.
        /// </summary>
        [Serialize(1)]
        public ushort ItemIndex { get; set; }
    }
}