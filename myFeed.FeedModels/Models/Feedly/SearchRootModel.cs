using System.Collections.Generic;
using Newtonsoft.Json;

namespace myFeed.FeedModels.Models.Feedly
{
    /// <summary>
    /// Represents feedly search results object.
    /// </summary>
    public sealed class SearchRootModel
    {
        /// <summary>
        /// List of items that were found by a feedly search engine.
        /// </summary>
        [JsonProperty("results")]
        public List<SearchItemModel> Results { get; set; }
    }
}
