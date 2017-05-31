using System.Collections.Generic;
using System.Xml.Serialization;

namespace myFeed.FeedModels.Models.Opml
{
    /// <summary>
    /// Single Feed info.
    /// </summary>
    [XmlType("outline")]
    public sealed class Outline
    {
        /// <summary>
        /// Parameterless constructor for XML serialization.
        /// </summary>
        public Outline() { }

        /// <summary>
        /// Instantiates new Outline.
        /// </summary>
        public Outline(string title = null, string version = null,
            string htmlUrl = null, string xmlUrl = null, string type = null,
            string text = null, List<Outline> childOutlines = null) => 
            (Title, Version, HtmlUrl, XmlUrl, Type, Text, ChildOutlines) =
            (title, version, htmlUrl, xmlUrl, type, text, childOutlines);

        /// <summary>
        /// Feed title.
        /// </summary>
        [XmlAttribute("title")]
        public string Title { get; set; }

        /// <summary>
        /// Feed version.
        /// </summary>
        [XmlAttribute("version")]
        public string Version { get; set; }

        /// <summary>
        /// Url for website.
        /// </summary>
        [XmlAttribute("htmlUrl")]
        public string HtmlUrl { get; set; }

        /// <summary>
        /// Rss url for website.
        /// </summary>
        [XmlAttribute("xmlUrl")]
        public string XmlUrl { get; set; }

        /// <summary>
        /// Type of feed.
        /// </summary>
        [XmlAttribute("type")]
        public string Type { get; set; }

        /// <summary>
        /// Additional text (usually equals to title).
        /// </summary>
        [XmlAttribute("text")]
        public string Text { get; set; }

        /// <summary>
        /// List of child outline nodes.
        /// </summary>
        [XmlElement("outline")]
        public List<Outline> ChildOutlines { get; set; }
    }
}
