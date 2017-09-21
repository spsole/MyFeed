using System;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace myFeed.Views.Uwp.Notifications
{
    public sealed class Runner : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var defferal = taskInstance.GetDeferral();
            var cost = BackgroundWorkCost.CurrentBackgroundWorkCost;
            if (cost == BackgroundWorkCostValue.High) return;

            defferal.Complete();
        }
    }
}
