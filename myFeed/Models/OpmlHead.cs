using System.Xml.Serialization;

namespace myFeed.Models
{
    [XmlType("head")]
    public sealed class OpmlHead
    {
        [XmlElement("title")]
        public string Title { get; set; }
    }
}