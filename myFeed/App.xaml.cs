using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using myFeed.Article;
using myFeed.Extensions;
using myFeed.Feed;
using myFeed.Navigation;
using myFeed.Settings;

namespace myFeed
{
    /// <summary>
    /// Application main class.
    /// </summary>
    public sealed partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private static void OnNavigationFailed(object sender, NavigationFailedEventArgs e) =>
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private static void OnSuspending(object sender, SuspendingEventArgs e) =>
            e.SuspendingOperation.GetDeferral().Complete(); // Reserved for future updates.

        /// <summary>
        /// Invoked when the application is launched normally by the end user. Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            Tools.Log("OnLaunched called. Tile id: " + e.TileId);
            var rootFrame = await CreateRootFrameAsync();

            //if (e.PrelaunchActivated) return;
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(NavigationPage));

            // Register notifications.
            BackgroundTasksManager.RegisterNotifier(SettingsManager
                .GetInstance().GetSettings().NotificationServiceCheckTime);

            // Do stuff for navigation from fav tile.
            switch (e.TileId)
            {
                case "App":
                    break;
                case "Favorites":
                    await Task.Delay(300);
                    if (NavigationPage.NavigationFrame.CurrentSourcePageType != typeof(Fave.FavePage))
                        NavigationPage.NavigationFrame.Navigate(
                            typeof(Fave.FavePage));
                    break;
                default:
                    await Task.Delay(300);
                    if (NavigationPage.NavigationFrame.CurrentSourcePageType != typeof(Fave.FavePage))
                        NavigationPage.NavigationFrame.Navigate(
                            typeof(Fave.FavePage));
                    var articles = await Fave.FaveManager.GetInstance().LoadArticles();
                    var target = articles.FirstOrDefault(i => i.GetModel().GetTileId() == e.TileId);
                    if (target != null)
                        Fave.FavePage.NavigationFrame.Navigate(
                            typeof(ArticlePage), target);
                    break;
            }
        }

        /// <summary>
        /// Occurs when application is activated in an abnormal way
        /// (for ex. by toast notification)
        /// </summary>
        /// <param name="args">Activation arguments</param>
        protected override async void OnActivated(IActivatedEventArgs args)
        {
            Tools.Log("OnActivated called.");
            if (args.Kind != ActivationKind.ToastNotification) return;

            var arraySplit = ((ToastNotificationActivatedEventArgs)args).Argument.Split(';');
            (var id, var categoryTitle) = (arraySplit[0], arraySplit[1]);
            Tools.Log($"Toast arguments: {id} & {categoryTitle}");

            // Build content frame.
            var rootFrame = await CreateRootFrameAsync();
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(NavigationPage));

            // Navigate NavigationPage to FeedCategoriesPage if it's not present.
            if (NavigationPage.NavigationFrame.CurrentSourcePageType != typeof(FeedCategoriesPage))
                NavigationPage.NavigationFrame.Navigate(typeof(FeedCategoriesPage));

            // Find appropriate category.
            var manager = FeedManager.GetInstance();
            var categories = await manager.ReadCategories();
            var category = categories.Categories.FirstOrDefault(i => i.Title == categoryTitle);
            if (category == null) return;

            // Retrieve data for category.
            var orderedFeed = await manager.RetrieveFeedAsync(category);
            var targetItem = orderedFeed.FirstOrDefault(i => i.GetModel().GetTileId() == id);
            if (targetItem == null) return;

            // Navigate and mark as read.
            FeedCategoriesPage.NavigationFrame.Navigate(
                typeof(ArticlePage), targetItem);
            targetItem.MarkAsRead();
        }

        #region Helper methods

        /// <summary>
        /// Creates a new RootFrame for this application or returns existing one.
        /// </summary>
        private static async Task<Frame> CreateRootFrameAsync()
        {
            // Return existing frame if already set.
            if (Window.Current.Content is Frame rootFrame) return rootFrame;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 300));

            // Read settings.
            var settings = await SettingsManager.GetInstance().ReadSettingsAsync();
            await ProcessFilesAsync();

            // Create frame.
            rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;
            var theme = (ElementTheme)settings.ApplicationTheme;
            rootFrame.RequestedTheme = theme;

            // Set content and activate window.
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
            return rootFrame;
        }

        /// <summary>
        /// Processes files, creates new ones if needed.
        /// </summary>
        private static async Task ProcessFilesAsync()
        {
            // Get dummy files.
            const CreationCollisionOption op = CreationCollisionOption.OpenIfExists;
            var folder = ApplicationData.Current.LocalFolder;
            await folder.CreateFolderAsync("favorites", op);
            await folder.CreateFileAsync("saved_cache", op);

            // Update last launch date time for notifications.
            var dateOffsetFile = await folder.CreateFileAsync("datecutoff", op);
            await FileIO.WriteTextAsync(dateOffsetFile,
                DateTime.Now.ToString(CultureInfo.InvariantCulture));

            // Do stuff with read.txt.
            var readFile = await folder.CreateFileAsync("read.txt", op);
            var contents = await FileIO.ReadTextAsync(readFile);
            if (string.IsNullOrEmpty(contents)) return;

            // Clear too many values.
            var readSplit = contents.Split(';').ToList();
            if (readSplit.Count > 180)
            {
                readSplit.RemoveAt(readSplit.Count - 1);
                readSplit = readSplit.Skip(Math.Max(0, readSplit.Count - 180)).ToList();
                await FileIO.WriteTextAsync(readFile, string.Join(";", readSplit));
            }
        }

        #endregion
    }
}
