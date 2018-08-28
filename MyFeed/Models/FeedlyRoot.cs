using System.Collections.Generic;
using Newtonsoft.Json;

namespace MyFeed.Models
{
    [JsonObject]
    public sealed class FeedlyRoot
    {
        [JsonProperty("results")]
        public List<FeedlyItem> Results { get; set; }
    }
}