using System.Collections.Generic;
using Newtonsoft.Json;

namespace myFeed.Services.Models
{
    [JsonObject]
    public sealed class FeedlyItem
    {
        [JsonProperty("deleciousTags")]
        public List<string> DeliciousTags { get; set; }

        [JsonProperty("feedId")]
        public string FeedId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("lastUpdated")]
        public long LastUpdated { get; set; }

        [JsonProperty("subscribers")]
        public int Subscribers { get; set; }

        [JsonProperty("website")]
        public string Website { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("iconUrl")]
        public string IconUrl { get; set; }
    }
}