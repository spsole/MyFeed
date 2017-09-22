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
        /// <param name="content">Content to share.</param>
        Task Share(string content);

        /// <summary>
        /// Launches Uri async.
        /// </summary>
        /// <param name="uri">Uri to open.</param>
        Task LaunchUri(Uri uri);

        /// <summary>
        /// Copies text to clipboard.
        /// </summary>
        /// <param name="text">Text to copy.</param>
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