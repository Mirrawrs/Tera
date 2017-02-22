using System.Collections.Generic;
using Lotus.Serialization.Attributes;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents a collection in which the elements are lazily loaded and cached.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public abstract class LazyCollection<TItem>
    {
        private readonly Dictionary<DataCenterIndex, TItem> cachedItems = new Dictionary<DataCenterIndex, TItem>();

        [Serialize(0)]
        internal DataCenterReader.ReadAtAddressDelegate<TItem> ReadItem { get; set; }

        /// <summary>
        ///     Tries to get the item associated with the specified index from the cached and, if it's not found, reads it through
        ///     <see cref="ReadItem" /> and caches it.
        /// </summary>
        /// <param name="index">The item's index.</param>
        /// <returns>The read item.</returns>
        public TItem this[DataCenterIndex index]
        {
            get
            {
                if (cachedItems.TryGetValue(index, out var value)) return value;
                var valueAddress = GetAddress(index);
                return cachedItems[index] = ReadItem(valueAddress);
            }
        }

        /// <summary>
        ///     When overridden in a derived class, retrieves the absolute address of an item given its index.
        /// </summary>
        /// <param name="regionIndex">The item's index.</param>
        /// <returns>The item's address.</returns>
        public abstract int GetAddress(DataCenterIndex regionIndex);
    }
}