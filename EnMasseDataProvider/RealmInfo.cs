using System.Xml.Serialization;
using Tera.Interfaces;

namespace Tera.EnMasse
{
    /// <summary>
    ///     Represents the connection information for a realm.
    /// </summary>
    public class RealmInfo : IRealmInfo
    {
        /// <summary>
        ///     Gets the host of the realm's server.
        /// </summary>
        [XmlElement("ip")]
        public string Host { get; set; }

        /// <summary>
        ///     Gets the port the realm's server is listening on.
        /// </summary>
        [XmlElement("port")]
        public int Port { get; set; }

        /// <summary>
        ///     Gets the realm's name.
        /// </summary>
        [XmlElement("name")]
        public string Name { get; set; }
    }
}