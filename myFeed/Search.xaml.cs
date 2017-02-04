using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media.Animation;

namespace myFeed
{
    public sealed partial class Search : Page
    {
        public Search()
        {
            this.InitializeComponent();
            App.CanNavigate = true;
            App.ChosenIndex = 4;
        }

        private async void FindAll_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SearchInput.Text))
            {
                WelcomeDisabling.Begin();
                NetworkError.Visibility = Visibility.Collapsed;
                LoadStatus.Visibility = Visibility.Visible;
                LoadStatus.IsIndeterminate = true;
                StatusBarEnabling.Begin();

                await SearchForFeeds(
                    $"http://cloud.feedly.com/v3/search/feeds?count=20&query=:{SearchInput.Text}"
                );

                LoadStatus.IsIndeterminate = false;
                StatusBarDisabling.Begin();
            }
        }

        private async Task SearchForFeeds(string query)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                using (HttpResponseMessage responseMessage = await httpClient.GetAsync(query))
                {
                    string responseString = await responseMessage.Content.ReadAsStringAsync();
                    JObject stuff = JObject.Parse(responseString);
                    Display.Items.Clear();

                    foreach (var item in stuff["results"])
                    {
                        ListFeed listFeed = new ListFeed()
                        {
                            feedid = ((string)item["feedId"]).Substring(5),
                            feedimg = (string)item["iconUrl"],
                            feedtitle = (string)item["title"],
                            feedlink = (string)item["website"],
                            feedsubtitle = (string)item["description"]
                        };
                        Display.Items.Add(listFeed);
                    }
                    
                    if (Display.Items.Count == 0) throw new Exception();
                }
            }
            catch (Exception ex)
            {
#if DEBUG
                Debug.WriteLine(ex);
#endif
                NetworkError.Opacity = 0;
                NetworkError.Visibility = Visibility.Visible;
                ErrorEnabling.Begin();
            }
        }

        private void Image_ImageOpened(object sender, RoutedEventArgs e)
        {
            Image img = sender as Image;
            img.Opacity = 0;
            if (img.Source.ToString() == string.Empty) return;
            DoubleAnimation fade = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.2),
                EnableDependentAnimation = true
            };
            Storyboard.SetTarget(fade, img);
            Storyboard.SetTargetProperty(fade, "Opacity");
            Storyboard openpane = new Storyboard();
            openpane.Children.Add(fade);
            openpane.Begin();
        }

        private void Grid_Holding(object sender, RoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((Grid)sender);
        }

        private void SearchItemCopy_Click(object sender, RoutedEventArgs e)
        {
            ListFeed feed = (ListFeed)((FrameworkElement)sender).DataContext;
            DataPackage dataPackage = new DataPackage();
            dataPackage.SetText(feed.feedid);
            Clipboard.SetContent(dataPackage);
        }

        private async void SearchItemOpen_Click(object sender, RoutedEventArgs e)
        {
            ListFeed feed = (ListFeed)((FrameworkElement)sender).DataContext;
            try
            {
                await Windows.System.Launcher.LaunchUriAsync(new Uri(feed.feedlink));
            }
            catch { }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            ListFeed feed = (ListFeed)((FrameworkElement)sender).DataContext;
            ResourceLoader rl = new ResourceLoader();
            bool isEmpty = false;

            SearchAddDialog addsource = new SearchAddDialog();
            Categories cats = new Categories();
            ComboBox box = new ComboBox();

            addsource.Loaded += async (s, a) =>
            {
                cats = await SerializerExtensions.DeSerializeObject<Categories>(
                    await ApplicationData.Current.LocalFolder.GetFileAsync("sites"));
                box = addsource.CategoriesComboBox;

                foreach (Category cat in cats.categories) box.Items.Add(cat.title);
                if (box.Items.Count < 1)
                {
                    addsource.CategoriesTextBlock.Visibility = Visibility.Collapsed;
                    addsource.CategoriesComboBox.Visibility = Visibility.Collapsed;
                    addsource.Title = rl.GetString("SearchCreateNewCatTitleFL");
                    addsource.CategoriesTextBlockBox.Text = rl.GetString("SearchCreateNewCatTextFL");
                    isEmpty = true;
                    return;
                }

                box.SelectedIndex = 0;
            };

            if (await addsource.ShowAsync() == ContentDialogResult.Primary)
            {
                string name = addsource.CategoriesText.Text;
                if (string.IsNullOrWhiteSpace(name) && isEmpty) return;

                if (!string.IsNullOrWhiteSpace(name))
                {
                    foreach (Category c in cats.categories)
                    {
                        if (c.title == name)
                        {
                            await (new MessageDialog((new ResourceLoader()).GetString("CategoryExists")).ShowAsync());
                            return;
                        }
                    }

                    Category newcat = new Category()
                    {
                        title = name,
                        websites = new List<Website>()
                        {
                            new Website()
                            {
                                notify = true,
                                url = feed.feedid
                            }
                        }
                    };

                    cats.categories.Add(newcat);
                    SerializerExtensions.SerializeObject(cats,
                        await ApplicationData.Current.LocalFolder.GetFileAsync("sites"));
                    addsource.Hide();
                    await (new MessageDialog(string.Format(rl.GetString("SearchAddMessage"), feed.feedtitle, name),
                        rl.GetString("SearchAddSuccess"))).ShowAsync();
                    return;
                }

                name = (string)box.SelectedItem;
                foreach (Category cat in cats.categories)
                {
                    if (cat.title != name) continue;
                    Website wb = new Website();
                    wb.url = feed.feedid;
                    cat.websites.Add(wb);
                    break;
                }

                SerializerExtensions.SerializeObject(cats,
                    await ApplicationData.Current.LocalFolder.GetFileAsync("sites"));
                addsource.Hide();
                await (new MessageDialog(string.Format(rl.GetString("SearchAddMessage"), feed.feedtitle, name),
                    rl.GetString("SearchAddSuccess"))).ShowAsync();
            }
        }
    }
}
