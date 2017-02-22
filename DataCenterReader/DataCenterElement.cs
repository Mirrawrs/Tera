using System.Collections.Generic;
using System.Linq;
using Lotus.Serialization.Attributes;
using Tera.Analytics.DirectiveSelectors;

namespace Tera.Analytics
{
    /// <summary>
    ///     Represents a vertex in the Data Center.
    /// </summary>
    public class DataCenterElement
    {
        /// <summary>
        ///     Gets the element's name.
        /// </summary>
        [Serialize(0)]
        [NameString]
        public string Name { get; private set; }

        [Serialize(1)]
        public ushort Zero { get; private set; }

        /// <summary>
        ///     Gets a collection that represents the attributes for the element.
        /// </summary>
        public IEnumerable<DataCenterAttribute> Attributes { get; internal set; }

        /// <summary>
        ///     Gets a collection that represents the element's children.
        /// </summary>
        public IEnumerable<DataCenterElement> Children { get; internal set; }

        /// <summary>
        ///     Gets the value of the attribute with the specified name or null if the attribute doesn't exist.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <returns>The attribute's value or null if the attribute doesn't exist.</returns>
        public object this[string name] => Attributes
            .SingleOrDefault(attribute => name == attribute.Name)
            ?.Value;
    }
}