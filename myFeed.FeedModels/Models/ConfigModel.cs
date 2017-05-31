using System.Xml.Serialization;

namespace myFeed.FeedModels.Models
{
    /// <summary>
    /// Stores application settings.
    /// </summary>
    [XmlRoot("ConfigFile")]
    public sealed class ConfigModel
    {
        /// <summary>
        /// Empty constructor for serializer.
        /// </summary>
        public ConfigModel() { }

        /// <summary>
        /// Initializes a new settings model instance.
        /// </summary>
        public ConfigModel(int articleFontSize, uint notificationServiceCheckTime, 
            bool downloadImages, int applicationTheme, bool bannersEnabled) =>
            (ArticleFontSize, NotificationServiceCheckTime, DownloadImages, ApplicationTheme, BannersEnabled) =
            (articleFontSize, notificationServiceCheckTime, downloadImages, applicationTheme, bannersEnabled);

        /// <summary>
        /// Font size in articles.
        /// </summary>
        [XmlElement("FontSize")]
        public int ArticleFontSize { get; set; }

        /// <summary>
        /// How often app should browse new articles in background?
        /// </summary>
        [XmlElement("CheckTime")]
        public uint NotificationServiceCheckTime { get; set; }

        /// <summary>
        /// Should the app download images or not?
        /// </summary>
        [XmlElement("DownloadImages")]
        public bool DownloadImages { get; set; }

        /// <summary>
        /// Requested theme (dark, light, default). 0 = N/A, 1 = Light, 2 = Dark
        /// </summary>
        [XmlElement("RequestedTheme")]
        public int ApplicationTheme { get; set; }

        /// <summary>
        /// Are banners and sounds enabled or not.
        /// </summary>
        public bool BannersEnabled { get; set; }

        /// <summary>
        /// Set defaults if manually created.
        /// </summary>
        public static ConfigModel GetDefault() =>
            new ConfigModel(17, 60, true, 0, true);
    }
}
