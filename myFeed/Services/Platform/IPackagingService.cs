using System.Threading.Tasks;

namespace myFeed.Services.Platform
{
    public interface IPackagingService
    {
        Task LeaveFeedback();

        Task LeaveReview();

        string Version { get; }
    }
}
