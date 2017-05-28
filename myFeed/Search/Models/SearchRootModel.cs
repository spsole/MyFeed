using Newtonsoft.Json;
using System.Collections.Generic;

namespace myFeed.Search.Models
{
    /// <summary>
    /// Represents feedly search results object.
    /// </summary>
    public class SearchRootModel
    {
        /// <summary>
        /// List of items that were found by a feedly search engine.
        /// </summary>
        [JsonProperty("results")]
        public List<SearchItemModel> Results { get; set; }
    }
}
