using System;
using System.Threading.Tasks;

namespace myFeed.Interfaces
{
    public interface IBackgroundService
    {
        Task CheckForUpdates(DateTime dateTime);
    }
}