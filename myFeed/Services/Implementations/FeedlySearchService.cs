using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Models;
using myFeed.Services.Abstractions;
using Newtonsoft.Json;

namespace myFeed.Services.Implementations
{
    [Reuse(ReuseType.Singleton)]
    [Export(typeof(ISearchService))]
    public sealed class FeedlySearchService : ISearchService
    {
        private const string QueryUrl = @"http://cloud.feedly.com/v3/search/feeds?count=40&query=:";
        private readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(() => new HttpClient());

        public Task<FeedlyRoot> SearchAsync(string query) => Task.Run(async () =>
        {
            try
            {
                var requestUrl = string.Concat(QueryUrl, query);
                var fetch = _client.Value.GetStreamAsync(requestUrl);
                using (var stream = await fetch.ConfigureAwait(false))
                using (var streamReader = new StreamReader(stream))
                using (var jsonReader = new JsonTextReader(streamReader))
                {
                    var serializer = new JsonSerializer();
                    var response = serializer.Deserialize<FeedlyRoot>(jsonReader);
                    return response;
                }
            }
            catch (Exception)
            {
                return new FeedlyRoot
                {
                    Results = new List<FeedlyItem>()
                };
            }
        });
    }
}