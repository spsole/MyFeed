using System.Xml.Serialization;

namespace myFeed.FeedModels.Models
{
    /// <summary>
    /// Saved website model.
    /// </summary>
    [XmlType("Website")]
    public sealed class SourceItemModel
    {
        /// <summary>
        /// Website's feed Uri.
        /// </summary>
        [XmlElement("url")]
        public string Uri { get; set; }

        /// <summary>
        /// Should app's user receive notifications from the app or not?
        /// </summary>
        [XmlElement("notify")]
        public bool Notify { get; set; }
    }
}
