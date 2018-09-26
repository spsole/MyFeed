using MyFeed.ViewModels;
using System.Threading.Tasks;
using System.Reactive.Disposables;
using Windows.ApplicationModel.Resources;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using ReactiveUI;
using System;
using System.Reactive.Linq;

namespace MyFeed.Uwp.Views
{
    public sealed partial class SettingView : Page, IViewFor<SettingViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(SettingViewModel),
                typeof(SettingView), new PropertyMetadata(null));

        public SettingView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as SettingViewModel;
            this.WhenActivated(disposables => 
            {
                this.Bind(ViewModel,
                    viewModel => viewModel.Font,
                    view => view.FontSlider.Value)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.Read,
                    view => view.ReadSwitch.IsOn)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.Images,
                    view => view.ImagesSwitch.IsOn)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.Period,
                    view => view.PeriodSlider.Value)
                    .DisposeWith(disposables);
                
                this.Bind(ViewModel,
                    viewModel => viewModel.Banners,
                    view => view.BannersSwitch.IsOn)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.Max,
                    view => view.MaxSlider.Value)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Export,
                    view => view.ExportButton)
                    .DisposeWith(disposables);
                ViewModel.ExportSuccess.RegisterHandler(async handle =>
                    handle.SetOutput(await ShowDialog("ExportOpmlSuccess")))
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Import,
                    view => view.ImportButton)
                    .DisposeWith(disposables);
                ViewModel.ImportSuccess.RegisterHandler(async handle =>
                    handle.SetOutput(await ShowDialog("ImportOpmlSuccess")))
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Reset,
                    view => view.ResetButton)
                    .DisposeWith(disposables);
                ViewModel.ResetConfirm.RegisterHandler(async handle =>
                    handle.SetOutput(await ShowDialog("ResetAppNoRestore")))
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Version,
                    view => view.VersionTextBlock.Text)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Feedback,
                    view => view.FeedbackButton)
                    .DisposeWith(disposables);

                this.BindCommand(ViewModel,
                    viewModel => viewModel.Review,
                    view => view.ReviewButton)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Theme)
                    .Take(1).Where(theme => theme == "default")
                    .Subscribe(ignore => DefaultRadioButton.IsChecked = true);
                Observable
                    .FromEventPattern(DefaultRadioButton, "Checked")
                    .Subscribe(ignored => ViewModel.Theme = "default");

                this.WhenAnyValue(x => x.ViewModel.Theme)
                    .Take(1).Where(theme => theme == "light")
                    .Subscribe(ignore => LightRadioButton.IsChecked = true);
                Observable
                    .FromEventPattern(LightRadioButton, "Checked")
                    .Subscribe(ignored => ViewModel.Theme = "light");

                this.WhenAnyValue(x => x.ViewModel.Theme)
                    .Take(1).Where(theme => theme == "dark")
                    .Subscribe(ignore => DarkRadioButton.IsChecked = true);
                Observable
                    .FromEventPattern(DarkRadioButton, "Checked")
                    .Subscribe(ignored => ViewModel.Theme = "dark");
            });
        }

        private async Task<bool> ShowDialog(string messageKey)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            var messageDialog = new MessageDialog(
                resourceLoader.GetString(messageKey),
                resourceLoader.GetString("SettingsNotification"));

            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Ok"), x => { }, true));
            messageDialog.Commands.Add(new UICommand(resourceLoader.GetString("Cancel"), x => { }, false));

            var result = await messageDialog.ShowAsync();
            return (bool)result.Id;
        }

        public SettingViewModel ViewModel
        {
            get => (SettingViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (SettingViewModel)value;
        }
    }
}