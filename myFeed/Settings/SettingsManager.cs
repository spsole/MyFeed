using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using myFeed.FeedModels.Models;
using myFeed.FeedModels.Serialization;

namespace myFeed.Settings
{
    /// <summary>
    /// Responsible for settings loading and saving for the App lifetime.
    /// </summary>
    public class SettingsManager
    {
        #region Singleton implementation

        private ConfigModel _settingsModel;
        private static SettingsManager _instance;
        private SettingsManager() { }

        /// <summary>
        /// Returns instance of app-wide settings manager.
        /// </summary>
        public static SettingsManager GetInstance() => 
            _instance ?? (_instance = new SettingsManager());

        #endregion
        
        /// <summary>
        /// Reads settings from disk. If no file is stored, saves default settings.
        /// </summary>
        public async Task<ConfigModel> ReadSettingsAsync()
        {
            // Create or open configuration file.
            var configFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                "config", CreationCollisionOption.OpenIfExists
            );

            // Deserialize object.
            var configModel = await GenericXmlSerializer.DeSerializeObject<ConfigModel>(configFile);
            if (configModel == null)
            {
                // Set defaults if serializer fails to serialize object.
                configModel = ConfigModel.GetDefault();
                GenericXmlSerializer.SerializeObject(configModel, configFile);
            }

            // Assign config as global variable.
            _settingsModel = configModel;
            return configModel;
        }

        /// <summary>
        /// Returns config model for current app.
        /// </summary>
        public ConfigModel GetSettings() => _settingsModel;

        /// <summary>
        /// Updates settings with new value.
        /// </summary>
        /// <param name="updateAction">Action to set new value to settings.</param>
        public void UpdateSettings(Action updateAction)
        {
            // Update settings and save them.
            updateAction.Invoke();
            SaveSettings();
        }

        /// <summary>
        /// Save settings to hard disk and returns updated config model.
        /// </summary>
        /// <returns></returns>
        public async void SaveSettings() => 
            GenericXmlSerializer.SerializeObject(_settingsModel, 
                await ApplicationData.Current.LocalFolder.GetFileAsync("config"));

        #region Static methods

        /// <summary>
        /// Returns currently install app version code.
        /// </summary>
        /// <returns>App version</returns>
        public static string GetAppVersion()
        {
            var package = Package.Current;
            var version = package.Id.Version;
            return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }

        #endregion
    }
}
