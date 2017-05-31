using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.Web.Syndication;
using myFeed.FeedModels.Models;
using myFeed.FeedModels.Serialization;

namespace myFeed.FeedUpdater
{
    /// <summary>
    /// Responsible for checking for new posts and sending 
    /// user-friendly toast notifications.
    /// </summary>
    public sealed class FeedUpdaterTask : IBackgroundTask
    {
        private BackgroundTaskDeferral _deferral;
        private HttpClient _longLivingClient;
        
        /// <summary>
        /// BackgroundTask runner entry point.
        /// </summary>
        /// <param name="taskInstance"></param>
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            // Get deferral.
            _deferral = taskInstance.GetDeferral();

            // Get files and folders.
            var localFolder = ApplicationData.Current.LocalFolder;
            var categoriesFile = await localFolder.GetFileAsync("sites");
            var dateOffsetFile = await localFolder.GetFileAsync("datecutoff");
            var configFile = await localFolder.GetFileAsync("config");

            // Deserialize info from files and folders.
            var categoriesModel = await GenericXmlSerializer.DeSerializeObject<FeedCategoriesModel>(categoriesFile);
            var cutOffText = await FileIO.ReadTextAsync(dateOffsetFile);
            var cutOffDate = DateTime.Parse(cutOffText, CultureInfo.InvariantCulture);
            var configModel = await GenericXmlSerializer.DeSerializeObject<ConfigModel>(configFile);

            // Update last-try date-time.
            await FileIO.WriteTextAsync(dateOffsetFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));

            // Loop thru categories and websites.
            var feedModels = new List<FeedItemModel>();
            foreach (var category in categoriesModel.Categories)
            foreach (var i in category.Websites)
            {
                // User disallows notifications.
                if (!i.Notify) continue;

                // Send request.
                Log($"Sending request to this: {i.Uri}");
                var httpClient = GetHttpClient();
                using (var response = await TryAsync(httpClient.GetAsync(new Uri(i.Uri)), null))
                {
                    // Skip if error
                    if (response == null) continue;

                    // Load into syndication feed.
                    Log($"Got string for: {i.Uri}");
                    var responseString = await response.Content.ReadAsStringAsync();
                    var syndicationFeed = new SyndicationFeed();

                    // Try load feed, skip feed channel on failure.
                    try { syndicationFeed.Load(responseString); } catch (Exception) { continue; }
                    Log($"Loaded feed for: {syndicationFeed.Title.Text}");

                    // Add only fresh data to collection.
                    feedModels.AddRange(
                    syndicationFeed.Items
                        .Where(x => x.PublishedDate > cutOffDate)
                        .Select(x => FeedItemModel.FromSyndicationItem(x, syndicationFeed.Title.Text))
                        .Select(x => { x.Content = category.Title; return x; })
                    );
                    Log($"Added range for: {syndicationFeed.Title.Text}");
                }
            }

            // Add items as Toast notifications.
            foreach (var model in feedModels.OrderBy(i => i.PublishedDate).Take(15))
            {
                // Here 'content' means related category name.
                Log($"Sending Toast: {model.Title}");
                await SendToastNotification(model, model.Content, 
                    configModel.DownloadImages, configModel.BannersEnabled);
            }

            // Indicate that this task can be 
            // killed or suspended by system.
            _longLivingClient.Dispose();
            _deferral.Complete();
        }

        /// <summary>
        /// Logs info to DEBUG output.
        /// </summary>
        /// <param name="o">Object to log.</param>
        private static void Log(object o)
        {
#if DEBUG
            Debug.WriteLine($"[NOTIFY TASK]: {o}");
#endif
        }

        /// <summary>
        /// Sends toast notification to Windows notification center.
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="categoryName">Related category name</param>
        /// <param name="needImages">Should manager load images</param>
        /// <param name="needSound">Should banners be shown</param>
        private static async Task SendToastNotification(FeedItemModel model,
            string categoryName, bool needImages, bool needSound)
        {
            await Task.Delay(300);
            var command = $"{model.GetTileId()};{categoryName}";

            var imageString = 
                Uri.IsWellFormedUriString(model.ImageUri, UriKind.Absolute) && needImages
                ? $@"<image src='{model.ImageUri}' placement='appLogoOverride' hint-crop='circle'/>"
                : string.Empty;

            var tileXmlString = $@"
                <toast launch='{command}'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{model.Title}</text>
                            <text>{model.FeedTitle}</text>
                            {imageString}
                        </binding>
                    </visual>
                    <actions>
                        <action activationType='foreground' content='Read more' arguments='{command}'/>
                    </actions>
                </toast>";

            var xmlDocument = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDocument.LoadXml(tileXmlString);
            var notification = new ToastNotification(xmlDocument) { SuppressPopup = !needSound };
            ToastNotificationManager
                .CreateToastNotifier()
                .Show(notification);
        }

        /// <summary>
        /// Builds http client.
        /// </summary>
        /// <returns>Http client</returns>
        private HttpClient GetHttpClient()
        {
            // Construct browser-fake http client.
            if (_longLivingClient != null)
                return _longLivingClient;

            _longLivingClient = new HttpClient();
            _longLivingClient.DefaultRequestHeaders.Add("accept", "text/html, application/xhtml+xml, */*");
            _longLivingClient.DefaultRequestHeaders.Add("user-agent",
                "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");
            return _longLivingClient;
        }

        /// <summary>
        /// Tries to execute a task and returns default value onFail.
        /// </summary>
        /// <typeparam name="TResult">Generic typedef</typeparam>
        /// <param name="task">Task to execute</param>
        /// <param name="defaultResult">Default result</param>
        /// <returns></returns>
        private static async Task<TResult> TryAsync<TResult>(Task<TResult> task, TResult defaultResult)
        {
            try
            {
                return await task;
            }
            catch (Exception)
            {
                return defaultResult;
            }
        }
    }
}
