using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.System;
using DryIocAttributes;
using MyFeed.Platform;

namespace MyFeed.Uwp.Services
{
    [Reuse(ReuseType.Singleton)]
    [ExportEx(typeof(IPackagingService))]
    public sealed class UwpPackagingService : IPackagingService
    {
        private const string WindowsMarketUri = "ms-windows-store://review/?ProductId=9nblggh4nw02";
        private const string MailAddress = "worldbeater-dev@yandex.ru";

        public async Task LeaveFeedback()
        {
            var message = new EmailMessage {Subject = "MyFeed Feedback"};
            message.To.Add(new EmailRecipient(MailAddress));
            await EmailManager.ShowComposeNewEmailAsync(message);
        }

        public async Task LeaveReview() => await Launcher.LaunchUriAsync(new Uri(WindowsMarketUri));

        public string Version
        {
            get
            {
                var package = Package.Current;
                var version = package.Id.Version;
                return $"v{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }
    }
}
