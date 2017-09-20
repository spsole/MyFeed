using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Provides platform-specific navigation actions.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Brings user into view with view key.
        /// </summary>
        /// <param name="viewKey">View to show.</param>
        Task Navigate(ViewKey viewKey);

        /// <summary>
        /// Brings user into view with parameter.
        /// </summary>
        /// <param name="viewKey">View to show.</param>
        /// <param name="parameter">Parameter to pass.</param>
        Task Navigate(ViewKey viewKey, object parameter);
    }

    /// <summary>
    /// All views enum.
    /// </summary>
    public enum ViewKey
    {
        FeedView,
        FaveView,
        SearchView,
        SourcesView,
        ArticleView,
        SettingsView
    }
}
