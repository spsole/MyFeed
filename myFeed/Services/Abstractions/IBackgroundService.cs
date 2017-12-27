using System;
using System.Threading.Tasks;

namespace myFeed.Services.Abstractions
{
    public interface IBackgroundService
    {
        Task CheckForUpdates(DateTime dateTime);
    }
}