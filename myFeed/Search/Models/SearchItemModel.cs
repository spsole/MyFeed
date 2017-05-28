using Newtonsoft.Json;
using System.Collections.Generic;

namespace myFeed.Search.Models
{
    /// <summary>
    /// Represents a single search result model.
    /// </summary>
    public class SearchItemModel
    {
        /// <summary>
        /// Item tags.
        /// </summary>
        [JsonProperty("deleciousTags")]
        public List<string> DeliciousTags { get; set; }

        /// <summary>
        /// Feed id.
        /// </summary>
        [JsonProperty("feedId")]
        public string FeedId { get; set; }

        /// <summary>
        /// Item title.
        /// </summary>
        [JsonProperty("title")]
        public string Title { get; set; }

        /// <summary>
        /// Language info.
        /// </summary>
        [JsonProperty("language")]
        public string Language { get; set; }

        /// <summary>
        /// Info when feed was last updated.
        /// </summary>
        [JsonProperty("lastUpdated")]
        public long LastUpdated { get; set; }

        /// <summary>
        /// Subscribers count.
        /// </summary>
        [JsonProperty("subscribers")]
        public int Subscribers { get; set; }

        /// <summary>
        /// Full website uri.
        /// </summary>
        [JsonProperty("website")]
        public string Website { get; set; }

        /// <summary>
        /// Item description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        /// Icon url.
        /// </summary>
        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }
    }
}
