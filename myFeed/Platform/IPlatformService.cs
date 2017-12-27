using System;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface IPlatformService
    {
        Task LaunchUri(Uri uri);
        
        Task Share(string content);

        Task CopyTextToClipboard(string text);

        Task RegisterBackgroundTask(int freq);
        
        Task RegisterTheme(string theme);

        Task ResetApp();
    }
}