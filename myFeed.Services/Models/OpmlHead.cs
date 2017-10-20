using System.Xml.Serialization;

namespace myFeed.Services.Models
{
    [XmlType("head")]
    public sealed class OpmlHead
    {
        [XmlElement("title")]
        public string Title { get; set; }
    }
}