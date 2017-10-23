using System.Collections.Generic;
using System.Threading.Tasks;
using myFeed.Repositories.Models;

namespace myFeed.Services.Platform
{
    public interface INotificationService
    {
        Task SendNotifications(IEnumerable<Article> articles);
    }
}