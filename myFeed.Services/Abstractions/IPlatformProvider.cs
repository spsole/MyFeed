using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Provides platform-specific behavior.
    /// </summary>
    public interface IPlatformProvider
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
        /// Registers banners.
        /// </summary>
        Task RegisterBanners(bool needBanners);

        /// <summary>
        /// Registers application design theme.
        /// </summary>
        Task RegisterTheme(string theme);

        /// <summary>
        /// Picks single file for read.
        /// </summary>
        Task<Stream> PickFileForReadAsync();

        /// <summary>
        /// Picks single file for write.
        /// </summary>
        Task<Stream> PickFileForWriteAsync();

        /// <summary>
        /// Shows message dialog for user.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="title">Message title.</param>
        Task ShowDialog(string message, string title);

        /// <summary>
        /// Shows message dialog for user confirmation.
        /// </summary>
        /// <param name="message">Message text.</param>
        /// <param name="title">Message title.</param>
        /// <returns>True if accepted, false if declined.</returns>
        Task<bool> ShowDialogForConfirmation(string message, string title);

        /// <summary>
        /// Shows dialog to get results from a user.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="title"></param>
        /// <returns>Result string representing user input.</returns>
        Task<string> ShowDialogForResults(string message, string title);

        /// <summary>
        /// Shows search dialog for selection.
        /// </summary>
        /// <param name="options">Options to select from.</param>
        Task<object> ShowDialogForSelection(IEnumerable<object> options);
    }
}