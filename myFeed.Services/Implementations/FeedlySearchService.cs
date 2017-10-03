using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using myFeed.Entities.Feedly;
using myFeed.Services.Abstractions;
using Newtonsoft.Json;

namespace myFeed.Services.Implementations
{
    public sealed class FeedlySearchService : ISearchService
    {
        private static readonly Lazy<HttpClient> Client = new Lazy<HttpClient>(() => new HttpClient());
        private const string QueryUrl = @"http://cloud.feedly.com/v3/search/feeds?count=40&query=:";

        public Task<SearchRootEntity> Search(string query) => Task.Run(async () =>
        {
            try
            {
                var requestUrl = string.Concat(QueryUrl, query);
                var fetch = Client.Value.GetStreamAsync(requestUrl);
                using (var stream = await fetch.ConfigureAwait(false))
                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer();
                    var response = serializer.Deserialize<SearchRootEntity>(jsonReader);
                    return response;
                }
            }
            catch (Exception)
            {
                return new SearchRootEntity
                {
                    Results = new List<SearchItemEntity>()
                };
            }
        });
    }
}