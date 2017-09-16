using System.Collections.Generic;
using Newtonsoft.Json;

namespace myFeed.Entities.Feedly
{
    public sealed class SearchRootEntity
    {
        [JsonProperty("results")]
        public List<SearchItemEntity> Results { get; set; }
    }
}