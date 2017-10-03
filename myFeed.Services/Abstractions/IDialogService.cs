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
        Task ShowDialog(string message, string title);

        /// <summary>
        /// Shows message dialog for user confirmation.
        /// </summary>
        Task<bool> ShowDialogForConfirmation(string message, string title);

        /// <summary>
        /// Shows dialog to get results from a user.
        /// </summary>
        Task<string> ShowDialogForResults(string message, string title);

        /// <summary>
        /// Shows search dialog for selection.
        /// </summary>
        Task<object> ShowDialogForSelection(IEnumerable<object> options);
    }
}