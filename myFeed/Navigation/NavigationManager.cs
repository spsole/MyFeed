using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using myFeed.Extensions;

namespace myFeed.Navigation
{
    /// <summary>
    /// Manages navigation scenario for the whole application.
    /// </summary>
    public class NavigationManager
    {
        private static NavigationManager _instance;
        private readonly List<(Action<BackRequestedEventArgs>, EventPriority)> _invokationList;
        private NavigationManager()
        {
            // Settings for navigation manager for current window.
            var systemManager = SystemNavigationManager.GetForCurrentView();

            // Apply titlebar settings.
            systemManager.AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;

            // Apply statusBar settings.
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundOpacity = 1;
                switch (((Frame)Window.Current.Content).RequestedTheme)
                {
                    case ElementTheme.Default:
                        statusBar.BackgroundColor = (Color)Application.Current.Resources["SystemChromeLowColor"];
                        statusBar.ForegroundColor = (Color)Application.Current.Resources["SystemBaseHighColor"];
                        break;
                    case ElementTheme.Light:
                        statusBar.BackgroundColor = Tools.GetColorFromHex("#FFF1F1F1");
                        statusBar.ForegroundColor = Tools.GetColorFromHex("#FF000000");
                        break;
                    case ElementTheme.Dark:
                        statusBar.BackgroundColor = Tools.GetColorFromHex("#FF171717");
                        statusBar.ForegroundColor = Tools.GetColorFromHex("#FFFFFFFF");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            // Definition for invokation list.
            _invokationList = new List<(Action<BackRequestedEventArgs>, EventPriority)>();
            systemManager.BackRequested += OnBackRequested;
        }

        /// <summary>
        /// Returns an instance of navigation manager.
        /// </summary>
        public static NavigationManager GetInstance() => 
            _instance ?? (_instance = new NavigationManager());

        /// <summary>
        /// Handles system back requested event.
        /// </summary>
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            // Invoke delegates in reversed order.
            var list = _invokationList
                .Reverse<(Action<BackRequestedEventArgs>, EventPriority)>()
                .OrderByDescending(i => i.Item2)
                .ToList();

            // Iterate manually.
            foreach (var pair in list)
            {
                if (e.Handled) break;
                pair.Item1.Invoke(e);
            }
        }

        /// <summary>
        /// Adds event to the collection.
        /// </summary>
        /// <param name="action">Action.</param>
        /// <param name="priority">Priority.</param>
        public void AddBackHandler(Action<BackRequestedEventArgs> action, 
            EventPriority priority = EventPriority.Low) =>
                _invokationList.Add((action, priority));
    }

    /// <summary>
    /// Event priority enum.
    /// </summary>
    public enum EventPriority
    {
        /// <summary>
        /// Low priority.
        /// </summary>
        Low,

        /// <summary>
        /// Middle priority.
        /// </summary>
        Normal,

        /// <summary>
        /// High priority.
        /// </summary>
        High,

        /// <summary>
        /// Highest priority.
        /// </summary>
        Highest
    }
}
