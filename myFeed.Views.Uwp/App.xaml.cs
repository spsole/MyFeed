using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Views.Uwp.Views;

namespace myFeed.Views.Uwp
{
    public sealed partial class App : Application
    {
        public App()
        {
            UnhandledException += (_, args) => Debug.WriteLine(args.Message);
            InitializeComponent();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = await CreateRootFrameAsync();
            if (rootFrame.Content == null) rootFrame.Navigate(typeof(MenuView));
            var systemManager = SystemNavigationManager.GetForCurrentView();
            systemManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            /*
            // Register notifications.
            var settings = SettingsService.GetInstance().GetSettings();
            BackgroundTasksManager.RegisterNotifier(settings.NotificationServiceCheckTime);

            // Do stuff for navigation from fav tile.
            switch (e.TileId)
            {
                case "App":
                    break;
                case "Favorites":
                    await Task.Delay(300);
                    if (NavigationPage.NavigationFrame.CurrentSourcePageType != typeof(Fave.FavePage))
                        NavigationPage.NavigationFrame.Navigate(typeof(Fave.FavePage));
                    break;
                default:
                    await Task.Delay(300);
                    if (NavigationPage.NavigationFrame.CurrentSourcePageType != typeof(Fave.FavePage))
                        NavigationPage.NavigationFrame.Navigate(typeof(Fave.FavePage));
                    var articles = await Fave.FaveManager.GetInstance().LoadArticles();
                    var target = articles.FirstOrDefault(i => i.GetModel().GetTileId() == e.TileId);
                    if (target != null)
                        Fave.FavePage.NavigationFrame.Navigate(
                            typeof(ArticlePage), target);
                    break;
            }
            */
        }

        protected override async void OnActivated(IActivatedEventArgs args)
        {
            /*
            if (args.Kind != ActivationKind.ToastNotification) return;
            var arraySplit = ((ToastNotificationActivatedEventArgs)args).Argument.Split(';');
            (var id, var categoryTitle) = (arraySplit[0], arraySplit[1]);

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
            */
        }

        private static Task<Frame> CreateRootFrameAsync()
        {
            if (Window.Current.Content is Frame rootFrame) return Task.FromResult(rootFrame);
            rootFrame = new Frame();
            Window.Current.Content = rootFrame;
            Window.Current.Activate();
            return Task.FromResult(rootFrame);
            /*
            // Return existing frame if already set.
            if (Window.Current.Content is Frame rootFrame) return rootFrame;
            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(300, 300));

            // Read settings.
            var settings = await SettingsService.GetInstance().ReadSettingsAsync();
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
            */
        }

        private static async Task ProcessFilesAsync()
        {
            /*
            // Get dummy files.
            const CreationCollisionOption op = CreationCollisionOption.OpenIfExists;
            var folder = ApplicationData.Current.LocalFolder;
            await folder.CreateFolderAsync("favorites", op);
            await folder.CreateFileAsync("saved_cache", op);

            // Update last launch date time for notifications.
            var dateOffsetFile = await folder.CreateFileAsync("datecutoff", op);
            await FileIO.WriteTextAsync(dateOffsetFile, DateTime.Now.ToString(CultureInfo.InvariantCulture));

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
            */
        }
    }
}