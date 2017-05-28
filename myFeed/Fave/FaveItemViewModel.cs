using System;
using System.Linq;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using myFeed.Extensions.ViewModels;
using myFeed.Feed;
using myFeed.FeedModels.Models;

namespace myFeed.Fave
{
    /// <summary>
    /// Fave extended class of FeedItemViewModel.
    /// </summary>
    public class FaveItemViewModel : FeedItemViewModel
    {
        /// <summary>
        /// Instantiates new FeedItemViewModel.
        /// </summary>
        public FaveItemViewModel(FeedItemModel model, bool read, bool favorite) : base(model, read, favorite) =>
            IsPinnedProperty.Value = SecondaryTile.Exists(model.GetTileId());

        #region Properties

        /// <summary>
        /// Indicates if the article is already pinned to start screen.
        /// </summary>
        public ObservableProperty<bool> IsPinnedProperty { get; } = new ObservableProperty<bool>();

        #endregion

        #region Methods

        /// <summary>
        /// Pins article to start screen if it's favorite.
        /// </summary>
        public async void PinToStart()
        {
            if (!IsFavorite.Value)
                throw new InvalidOperationException(
                    "Can not pin non-favorite article to start screen.");

            // Pin a tile.
            var model = GetModel();
            var tileId = model.GetTileId();
            var secondaryTile = new SecondaryTile(tileId, "myFeed Article", tileId,
                new Uri("ms-appx:///Assets/Square150x150Logo.scale-200.png"),
                TileSize.Square150x150
            );
            var success = await secondaryTile.RequestCreateAsync();
            if (!success) return;
            IsPinnedProperty.Value = true;

            // Draw UI for tile notifications.
            var contents = string.Format(@"
                <tile>
                    <visual>
                        <binding template='TileMedium'>
                            <text hint-wrap='true' hint-maxLines='2' hint-style='caption'>{0}</text>
                            <text hint-style='captionSubtle'>{1}</text>
                            <text hint-style='captionSubtle'>{2}</text>
                        </binding>
                        <binding template='TileWide'>
                            <text hint-wrap='true' hint-maxLines='3' hint-style='caption'>{0}</text>
                            <text hint-style='captionSubtle'>{1}</text>
                            <text hint-style='captionSubtle'>{3}</text>
                        </binding>
                    </visual>
                </tile> ",
                model.Title, model.FeedTitle,
                // ReSharper disable once UseFormatSpecifierInFormatString
                DateTime.Parse(model.PublishedDate).ToString("dd.MM.yyyy"),
                // ReSharper disable once UseFormatSpecifierInFormatString
                DateTime.Parse(model.PublishedDate).ToString("dd.MM.yyyy HH:mm"));

            // Register notifications for a tile.
            var xmlDoc = new Windows.Data.Xml.Dom.XmlDocument();
            xmlDoc.LoadXml(contents);
            TileUpdateManager
                .CreateTileUpdaterForSecondaryTile(tileId)
                .Update(new TileNotification(xmlDoc));
        }

        /// <summary>
        /// Unpins tile from start screen.
        /// </summary>
        public async void UnpinFromStart()
        {
            if (!IsFavorite.Value)
                throw new InvalidOperationException(
                    "Can not pin non-favorite article to start screen.");

            var model = GetModel();
            var secondaryTiles = await SecondaryTile.FindAllAsync();
            var neededTile = secondaryTiles.FirstOrDefault(i => i.TileId == model.GetTileId());
            if (neededTile == null || await neededTile.RequestDeleteAsync())
                IsPinnedProperty.Value = false;
        }

        /// <summary>
        /// Extends feed item view model to fave item view model.
        /// </summary>
        /// <param name="model">Model</param>
        /// <returns>Fave item view model.</returns>
        public static FaveItemViewModel Extend(FeedItemViewModel model) =>
            new FaveItemViewModel(model.GetModel(), model.IsRead.Value, model.IsFavorite.Value);

        #endregion
    }
}
