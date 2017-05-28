using System.Collections.Generic;
using System.Xml.Serialization;

namespace myFeed.FeedModels.Models
{
    /// <summary>
    /// Feed category model.
    /// </summary>
    [XmlType("Category")]
    public sealed class FeedCategoryModel
    {
        /// <summary>
        /// Category title.
        /// </summary>
        [XmlElement("title")]
        public string Title { get; set; }

        /// <summary>
        /// Websites list.
        /// </summary>
        [XmlArray("websites")]
        public List<SourceItemModel> Websites { get; set; }
    }
}
