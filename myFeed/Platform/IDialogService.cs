using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface IDialogService
    {
        Task ShowDialog(string message, string title);
        
        Task<bool> ShowDialogForConfirmation(string message, string title);
        
        Task<object> ShowDialogForSelection(IEnumerable<object> options);
        
        Task<string> ShowDialogForResults(string message, string title);
    }
}