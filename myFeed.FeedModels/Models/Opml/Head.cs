using System.Xml.Serialization;

namespace myFeed.FeedModels.Models.Opml
{
    /// <summary>
    /// Head root.
    /// </summary>
    [XmlType("head")]
    public sealed class Head
    {
        /// <summary>
        /// Feed title.
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }
    }
}
