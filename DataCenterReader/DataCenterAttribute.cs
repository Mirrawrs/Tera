using Lotus.Serialization.Attributes;
using Tera.Analytics.DirectiveSelectors;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents an attribute that describes a <see cref="DataCenterElement" />.
    /// </summary>
    public class DataCenterAttribute
    {
        /// <summary>
        ///     Gets the attribute's name.
        /// </summary>
        [Serialize(0)]
        [NameString]
        public string Name { get; private set; }

        /// <summary>
        ///     Gets the attribute's value.
        /// </summary>
        [Serialize(1)]
        public object Value { get; private set; }
    }
}