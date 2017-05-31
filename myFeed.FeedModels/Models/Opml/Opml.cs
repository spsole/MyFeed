using System.Collections.Generic;
using System.Xml.Serialization;

namespace myFeed.FeedModels.Models.Opml
{
    /// <summary>
    /// Xml root OPML element.
    /// </summary>
    [XmlType("opml")]
    public sealed class Opml
    {
        /// <summary>
        /// Head element.
        /// </summary>
        [XmlElement("head")]
        public Head Head { get; set; }

        /// <summary>
        /// Body element.
        /// </summary>
        [XmlArray("body")]
        public List<Outline> Body { get; set; }
    }
}
