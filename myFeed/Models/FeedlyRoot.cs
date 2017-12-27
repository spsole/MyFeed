using System.Collections.Generic;
using Newtonsoft.Json;

namespace myFeed.Models
{
    [JsonObject]
    public sealed class FeedlyRoot
    {
        [JsonProperty("results")]
        public List<FeedlyItem> Results { get; set; }
    }
}