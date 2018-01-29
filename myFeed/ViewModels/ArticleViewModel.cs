using System.Reactive;
using DryIocAttributes;
using myFeed.Interfaces;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(ArticleViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class ArticleViewModel
    {
        public ReactiveCommand<Unit, Unit> Load { get; }
        public FeedItemViewModel Article { get; }

        public bool IsLoading { get; private set; }
        public bool Images { get; private set; }
        public double Font { get; private set; }

        public ArticleViewModel(
            FeedItemViewModel feedItemViewModel,
            ISettingManager settingManager)
        {
            IsLoading = true;
            Article = feedItemViewModel;
            Load = ReactiveCommand.CreateFromTask(async () =>
            {
                var settings = await settingManager.Read();
                Images = settings.Images;
                Font = settings.Font;
                IsLoading = false;
            });
        }
    }
}
