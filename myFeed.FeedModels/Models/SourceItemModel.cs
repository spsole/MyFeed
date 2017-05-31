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
        /// Empty constructor for serialization.
        /// </summary>
        public SourceItemModel() { }

        /// <summary>
        /// Initializes new source item model.
        /// </summary>
        public SourceItemModel(
            string uri, bool notify) =>
            (Uri, Notify) = (uri, notify);

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
