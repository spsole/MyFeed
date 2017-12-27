using System.Collections.Generic;
using System.Xml.Serialization;

namespace myFeed.Models
{
    [XmlType("outline")]
    public sealed class OpmlOutline
    {
        [XmlAttribute("title")]
        public string Title { get; set; }

        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlAttribute("htmlUrl")]
        public string HtmlUrl { get; set; }

        [XmlAttribute("xmlUrl")]
        public string XmlUrl { get; set; }

        [XmlAttribute("type")]
        public string Type { get; set; }

        [XmlAttribute("text")]
        public string Text { get; set; }

        [XmlElement("outline")]
        public List<OpmlOutline> ChildOutlines { get; set; }
    }
}