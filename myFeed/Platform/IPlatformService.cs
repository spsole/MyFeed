using System;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface IPlatformService
    {
        Task<bool> RegisterBackgroundTask(int freq);

        Task<bool> RegisterTheme(string theme);

        Task CopyTextToClipboard(string text);

        Task Share(string content);

        Task LaunchUri(Uri uri);

        Task ResetApp();
    }
}