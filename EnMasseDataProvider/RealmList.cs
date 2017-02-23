using System.Collections.Generic;
using System.Xml.Serialization;

namespace Tera.EnMasse
{
    /// <summary>
    /// Represents a list of realms.
    /// </summary>
    [XmlRoot("serverlist")]
    public class RealmList
    {
        /// <summary>
        /// Gets or sets the realms.
        /// </summary>
        [XmlElement("server")]
        public List<RealmInfo> Items { get; set; }
    }
}