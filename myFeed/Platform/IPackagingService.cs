using System.Threading.Tasks;

namespace myFeed.Platform
{
    public interface IPackagingService
    {
        Task LeaveFeedback();

        Task LeaveReview();

        string Version { get; }
    }
}
