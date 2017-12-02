﻿using System;
using System.ComponentModel.Composition;
using DryIocAttributes;
using myFeed.Services.Abstractions;
using myFeed.Services.Models;
using myFeed.Services.Platform;
using myFeed.ViewModels.Bindables;

namespace myFeed.ViewModels.Implementations
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
            ICategoryStoreService categoriesRepository,
            IMediationService mediationService,
            IPlatformService platformService)
        {
            var channel = mediationService.Get<Channel>();
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
                var parentViewModel = mediationService.Get<ChannelCategoryViewModel>();
                parentViewModel.Items.Remove(this);
                await categoriesRepository.RemoveChannelAsync(
                    parentViewModel.Category.Value, channel);
            });
            Notify.PropertyChanged += async (sender, args) =>
            {
                channel.Notify = Notify.Value;
                var parentViewModel = mediationService.Get<ChannelCategoryViewModel>();
                await categoriesRepository.UpdateAsync(parentViewModel.Category.Value);
            };
        }
    }
}