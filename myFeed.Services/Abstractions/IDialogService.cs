using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    /// <summary>
    /// Asks user for input using message dialogs.
    /// </summary>
    public interface IDialogService
    {
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