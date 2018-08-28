using System.Collections.Generic;
using System.Threading.Tasks;
using MyFeed.Models;

namespace MyFeed.Platform
{
    public interface INotificationService
    {
        Task SendNotifications(IEnumerable<Article> articles);
    }
}