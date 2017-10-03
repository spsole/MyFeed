using System;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Provides platform-specific behavior.
    /// </summary>
    public interface IPlatformService
    {
        /// <summary>
        /// Shows share UI to share provided string.
        /// </summary>
        Task Share(string content);

        /// <summary>
        /// Launches Uri async.
        /// </summary>
        Task LaunchUri(Uri uri);

        /// <summary>
        /// Copies text to clipboard.
        /// </summary>
        Task CopyTextToClipboard(string text);

        /// <summary>
        /// Registers background notifications task.
        /// </summary>
        Task RegisterBackgroundTask(int freq);

        /// <summary>
        /// Registers application design theme.
        /// </summary>
        Task RegisterTheme(string theme);

        /// <summary>
        /// Resets application settings, removes stored
        /// data and relaunches the app.
        /// </summary>
        Task ResetApp();
    }
}