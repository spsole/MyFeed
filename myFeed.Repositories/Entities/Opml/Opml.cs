using System.Collections.Generic;
using System.Xml.Serialization;

namespace myFeed.Repositories.Entities.Opml {
    [XmlType("opml")]
    public sealed class Opml {
        [XmlElement("head")]
        public Head Head { get; set; }

        [XmlArray("body")]
        public List<Outline> Body { get; set; }
    }
}
