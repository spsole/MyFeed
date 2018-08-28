using System.Collections.Generic;
using System.Xml.Serialization;

namespace MyFeed.Models
{
    [XmlType("opml")]
    public sealed class Opml
    {
        [XmlElement("head")]
        public OpmlHead Head { get; set; }

        [XmlArray("body")]
        public List<OpmlOutline> Body { get; set; }
    }
}