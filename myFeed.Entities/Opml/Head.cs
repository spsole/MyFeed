using System.Xml.Serialization;

namespace myFeed.Entities.Opml
{
    [XmlType("head")]
    public class Head
    {
        [XmlElement("title")]
        public string Title { get; set; }
    }
}