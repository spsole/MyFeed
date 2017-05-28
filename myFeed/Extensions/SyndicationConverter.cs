using System;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Web.Syndication;

namespace myFeed.Extensions
{
    /// <summary>
    /// Sends and retrieves requests from websites.
    /// </summary>
    public class SyndicationConverter
    {
        #region Singleton implementation

        private static SyndicationConverter _instance;
        private readonly HttpClient _httpClient;

        private SyndicationConverter()
        {
            // Instantiate client and add browser headers.
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add(
                "accept", 
                "text/html, application/xhtml+xml, */*"
            );
            _httpClient.DefaultRequestHeaders.Add(
                "user-agent", 
                "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)"
            );
        }

        /// <summary>
        /// Returns an instance of syndication converter.
        /// </summary>
        public static SyndicationConverter GetInstance() => _instance ?? (_instance = new SyndicationConverter());

        #endregion

        /// <summary>
        /// Retrieves feed using static http client and fake headers.
        /// </summary>
        /// <param name="website">Website absolute url.</param>
        public async Task<SyndicationFeed> RetrieveFeedAsync(string website)
        {
            var response = await _httpClient.GetStringAsync(new Uri(website, UriKind.Absolute));
            var syndicationFeed = new SyndicationFeed();
            syndicationFeed.Load(response);
            return syndicationFeed;
        }
    }
}
