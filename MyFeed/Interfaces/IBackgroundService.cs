using System;
using System.Threading.Tasks;

namespace MyFeed.Interfaces
{
    public interface IBackgroundService
    {
        Task CheckForUpdates(DateTime dateTime);
    }
}