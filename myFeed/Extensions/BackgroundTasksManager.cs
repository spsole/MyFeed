using System;
using System.Linq;
using Windows.ApplicationModel.Background;

namespace myFeed.Extensions
{
    /// <summary>
    /// Manages background tasks.
    /// </summary>
    public static class BackgroundTasksManager
    {
        /// <summary>
        /// Registers myFeed notification task with desired checkTime. 0 === NEVER.
        /// </summary>
        /// <param name="checkTime">How often to execute task? 0 === NEVER.</param>
        public static async void RegisterNotifier(uint checkTime)
        {
            // Check for access.
            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus != BackgroundAccessStatus.AlwaysAllowed && 
                backgroundAccessStatus != BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
                return;

            // Unregister old stuff.
            BackgroundTaskRegistration.AllTasks
                .Where(i => i.Value.Name == "myFeedNotify")
                .ToList()
                .ForEach(i => i.Value.Unregister(true));

            // Return if zero.
            if (checkTime == 0) return;
            var builder = new BackgroundTaskBuilder
            {
                Name = "myFeedNotify",
                TaskEntryPoint = "myFeed.FeedUpdater.FeedUpdaterTask"
            };

            // Note: Time measures in minutes here, e.g. 30 = 30 minutes
            builder.SetTrigger(new TimeTrigger(checkTime, false)); 
            builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));
            builder.Register();
        }
    }
}
