using System.Collections.Generic;
using System.Xml.Serialization;

namespace myFeed.FeedModels.Models
{
    /// <summary>
    /// Feed categories view model.
    /// </summary>
    [XmlType("Categories")]
    public sealed class FeedCategoriesModel
    {
        /// <summary>
        /// Categories XML array.
        /// </summary>
        [XmlArray("categories")]
        public List<FeedCategoryModel> Categories { get; set; }
    }
}
