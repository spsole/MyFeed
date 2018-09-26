using MyFeed.ViewModels;
using ReactiveUI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System;

namespace MyFeed.Uwp.Views
{
    public sealed partial class MenuView : Page, IViewFor<MenuViewModel>
    {
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty
            .Register(nameof(ViewModel), typeof(MenuViewModel),
                typeof(MenuView), new PropertyMetadata(null));

        public MenuView()
        {
            InitializeComponent();
            DataContextChanged += (a, b) => ViewModel = DataContext as MenuViewModel;
            this.WhenActivated(disposables =>
            {
                TapLayout.Events().Tapped
                    .Select(args => !Menu.IsSwipeablePaneOpen)
                    .Subscribe(value => Menu.IsSwipeablePaneOpen = value)
                    .DisposeWith(disposables);

                this.OneWayBind(ViewModel,
                    viewModel => viewModel.Items,
                    view => view.MenuListBox.ItemsSource)
                    .DisposeWith(disposables);

                this.Bind(ViewModel,
                    viewModel => viewModel.Selection,
                    view => view.MenuListBox.SelectedItem)
                    .DisposeWith(disposables);

                this.WhenAnyValue(x => x.ViewModel.Selection)
                    .Subscribe(ignore => Menu.IsSwipeablePaneOpen = false)
                    .DisposeWith(disposables);

                Observable.FromEventPattern(MenuBtn, "Click")
                    .Subscribe(ignore => Menu.IsSwipeablePaneOpen = true)
                    .DisposeWith(disposables);
            });
        }

        public MenuViewModel ViewModel
        {
            get => (MenuViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        object IViewFor.ViewModel
        {
            get => ViewModel;
            set => ViewModel = (MenuViewModel)value;
        }
    }
}