using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface IPlatformService
    {
        IReadOnlyDictionary<Type, (string, object)> Icons { get; }

        Task CopyTextToClipboard(string text);
        
        Task RegisterBackgroundTask(int freq);

        Task RegisterTheme(string theme);

        Task Share(string content);

        Task LaunchUri(Uri uri);

        Task ResetApp();
    }
}