using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using myFeed.Repositories.Entities.Feedly;
using myFeed.Services.Abstractions;
using Newtonsoft.Json;

namespace myFeed.Services.Implementations {
    public class FeedlySearchService : ISearchService {
        public async Task<SearchRootEntity> Search(string query) {
            try {
                var queryUrl = $@"http://cloud.feedly.com/v3/search/feeds?count=40&query=:{query}";
                using (var client = new HttpClient())
                using (var response = await client.GetAsync(queryUrl).ConfigureAwait(false)) {
                    var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return JsonConvert.DeserializeObject<SearchRootEntity>(responseString);
                }
            } catch (Exception) {
                return new SearchRootEntity {
                    Results = new List<SearchItemEntity>()
                };
            }
        }
    }
}
