using System.Xml.Serialization;

namespace myFeed.Repositories.Entities.Opml
{
    [XmlType("head")]
    public sealed class Head
    {
        [XmlElement("title")]
        public string Title { get; set; }
    }
}