using System;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Common;
using myFeed.Interfaces;
using myFeed.Models;
using myFeed.Platform;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [Export(typeof(ChannelViewModel))]
    public sealed class ChannelViewModel
    { 
        public ObservableProperty<string> Url { get; }
        public ObservableProperty<string> Name { get; }
        public ObservableProperty<bool> Notify { get; }

        public ObservableCommand DeleteSource { get; }
        public ObservableCommand OpenInBrowser { get; }
        public ObservableCommand CopyLink { get; }

        public ChannelViewModel(
            ICategoryManager categoryManager,
            IPlatformService platformService,
            IStateContainer stateContainer)
        {
            var channel = stateContainer.Pop<Channel>();
            var category = stateContainer.Pop<Category>();
            var parentViewModel = stateContainer.Pop<
                ChannelCategoryViewModel>();
            Name = new Uri(channel.Uri).Host;
            Notify = channel.Notify;
            Url = channel.Uri;
            
            CopyLink = new ObservableCommand(() => platformService.CopyTextToClipboard(channel.Uri));
            OpenInBrowser = new ObservableCommand(async () =>
            {
                if (!Uri.IsWellFormedUriString(channel.Uri, UriKind.Absolute)) return;
                var uri = new Uri(channel.Uri);
                var plainUri = new Uri(string.Format("{0}://{1}", uri.Scheme, uri.Host));
                await platformService.LaunchUri(plainUri);
            });
            DeleteSource = new ObservableCommand(async () =>
            {
                parentViewModel.Items.Remove(this);
                category.Channels.Remove(channel);
                await categoryManager.UpdateAsync(category);
            });
            Notify.PropertyChanged += async (sender, args) =>
            {
                channel.Notify = Notify.Value;
                await categoryManager.UpdateAsync(category);
            };
        }
    }
}