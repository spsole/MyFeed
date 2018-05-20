using System.Reactive;
using System.Threading.Tasks;
using DryIocAttributes;
using myFeed.Interfaces;
using PropertyChanged;
using ReactiveUI;

namespace myFeed.ViewModels
{
    [Reuse(ReuseType.Transient)]
    [ExportEx(typeof(FeedItemFullViewModel))]
    [AddINotifyPropertyChangedInterface]
    public sealed class FeedItemFullViewModel
    {
        private readonly ISettingManager _settingManager;

        public ReactiveCommand<Unit, Unit> Load { get; }
        public FeedItemViewModel Article { get; }
        public double Font { get; private set; }
        public bool Images { get; private set; }

        public FeedItemFullViewModel(
            FeedItemViewModel feedItemViewModel,
            ISettingManager settingManager)
        {
            Article = feedItemViewModel;
            _settingManager = settingManager;
            Load = ReactiveCommand.CreateFromTask(DoLoad);
        }

        private async Task DoLoad()
        {
            var settings = await _settingManager.Read();
            Images = settings.Images;
            Font = settings.Font;
        }
    }
}
