using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface IDialogService
    {
        Task ShowDialog(string message, string title);
        
        Task<int> ShowDialogForSelection(string title, IEnumerable<string> options);
        
        Task<bool> ShowDialogForConfirmation(string message, string title);
        
        Task<string> ShowDialogForResults(string message, string title);
    }
}