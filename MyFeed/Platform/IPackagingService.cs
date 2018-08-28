using System.Threading.Tasks;

namespace MyFeed.Platform
{
    public interface IPackagingService
    {
        Task LeaveFeedback();

        Task LeaveReview();

        string Version { get; }
    }
}
