using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DryIocAttributes;
using MyFeed.Interfaces;
using MyFeed.Models;
using Newtonsoft.Json;

namespace MyFeed.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(ISearchService))]
    public sealed class FeedlySearchService : ISearchService
    {
        private const string QueryUrl = @"http://cloud.feedly.com/v3/search/feeds?count=40&query=:";
        private readonly Lazy<HttpClient> _client = new Lazy<HttpClient>(() => new HttpClient());

        public async Task<FeedlyRoot> Search(string query)
        {
            var url = string.Concat(QueryUrl, query);
            using (var stream = await _client.Value.GetStreamAsync(url).ConfigureAwait(false))
            using (var streamReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(streamReader))
                return new JsonSerializer().Deserialize<FeedlyRoot>(jsonReader);
        }
    }
}