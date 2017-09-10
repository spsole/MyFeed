using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;
using Microsoft.Toolkit.Uwp.UI.Extensions;

namespace myFeed.Views.Uwp.Controls
{
    public sealed class SwipeableSplitView : SplitView
    {
        private Grid _paneRoot;
        private Grid _overlayRoot;
        private Rectangle _panArea;
        private Rectangle _dismissLayer;
        private CompositeTransform _paneRootTransform;
        private CompositeTransform _panAreaTransform;
        private Storyboard _openSwipeablePane;
        private Storyboard _closeSwipeablePane;

        private Selector _menuHost;
        private int _toBeSelectedIndex;
        private double _distancePerItem;
        private double _startingDistance;

        private readonly IList<SelectorItem> _menuItems = new List<SelectorItem>();
        private const double TotalPanningDistance = 160d;

        public SwipeableSplitView() => DefaultStyleKey = typeof(SwipeableSplitView);

        private Grid PaneRoot
        {
            get => _paneRoot;
            set
            {
                if (_paneRoot != null)
                {
                    _paneRoot.Loaded -= OnPaneRootLoaded;
                    _paneRoot.ManipulationStarted -= OnManipulationStarted;
                    _paneRoot.ManipulationDelta -= OnManipulationDelta;
                    _paneRoot.ManipulationCompleted -= OnManipulationCompleted;
                }

                _paneRoot = value;
                if (_paneRoot == null) return;

                _paneRoot.Loaded += OnPaneRootLoaded;
                _paneRoot.ManipulationStarted += OnManipulationStarted;
                _paneRoot.ManipulationDelta += OnManipulationDelta;
                _paneRoot.ManipulationCompleted += OnManipulationCompleted;
            }
        }

        private Rectangle PanArea
        {
            get => _panArea;
            set
            {
                if (_panArea != null)
                {
                    _panArea.ManipulationStarted -= OnManipulationStarted;
                    _panArea.ManipulationDelta -= OnManipulationDelta;
                    _panArea.ManipulationCompleted -= OnManipulationCompleted;
                    _panArea.Tapped -= OnDismissLayerTapped;
                }

                _panArea = value;
                if (_panArea == null) return;

                _panArea.ManipulationStarted += OnManipulationStarted;
                _panArea.ManipulationDelta += OnManipulationDelta;
                _panArea.ManipulationCompleted += OnManipulationCompleted;
                _panArea.Tapped += OnDismissLayerTapped;
            }
        }

        private Rectangle DismissLayer
        {
            get => _dismissLayer;
            set
            {
                if (_dismissLayer != null)
                    _dismissLayer.Tapped -= OnDismissLayerTapped;

                _dismissLayer = value;

                if (_dismissLayer != null)
                    _dismissLayer.Tapped += OnDismissLayerTapped;
            }
        }

        private Storyboard OpenSwipeablePaneAnimation
        {
            get => _openSwipeablePane;
            set
            {
                if (_openSwipeablePane != null)
                    _openSwipeablePane.Completed -= OnOpenSwipeablePaneCompleted;

                _openSwipeablePane = value;

                if (_openSwipeablePane != null)
                    _openSwipeablePane.Completed += OnOpenSwipeablePaneCompleted;
            }
        }

        private Storyboard CloseSwipeablePaneAnimation
        {
            get => _closeSwipeablePane;
            set
            {
                if (_closeSwipeablePane != null)
                    _closeSwipeablePane.Completed -= OnCloseSwipeablePaneCompleted;

                _closeSwipeablePane = value;

                if (_closeSwipeablePane != null)
                    _closeSwipeablePane.Completed += OnCloseSwipeablePaneCompleted;
            }
        }

        public static readonly DependencyProperty IsSwipeablePaneOpenProperty =
            DependencyProperty.Register(nameof(IsSwipeablePaneOpen), typeof(bool),
                typeof(SwipeableSplitView), new PropertyMetadata(false, OnIsSwipeablePaneOpenChanged));

        public bool IsSwipeablePaneOpen
        {
            get => (bool) GetValue(IsSwipeablePaneOpenProperty);
            set => SetValue(IsSwipeablePaneOpenProperty, value);
        }

        private static void OnIsSwipeablePaneOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var splitView = (SwipeableSplitView) d;
            switch (splitView.DisplayMode)
            {
                case SplitViewDisplayMode.Inline:
                case SplitViewDisplayMode.CompactOverlay:
                case SplitViewDisplayMode.CompactInline:
                    splitView.IsPaneOpen = (bool) e.NewValue;
                    break;

                case SplitViewDisplayMode.Overlay:
                    if (splitView.OpenSwipeablePaneAnimation == null ||
                        splitView.CloseSwipeablePaneAnimation == null) return;
                    if ((bool) e.NewValue)
                        splitView.OpenSwipeablePane();
                    else
                        splitView.CloseSwipeablePane();
                    break;
            }
        }

        public static readonly DependencyProperty PanAreaInitialTranslateXProperty =
            DependencyProperty.Register(nameof(PanAreaInitialTranslateX), typeof(double),
                typeof(SwipeableSplitView), new PropertyMetadata(0d));

        public double PanAreaInitialTranslateX
        {
            get => (double) GetValue(PanAreaInitialTranslateXProperty);
            set => SetValue(PanAreaInitialTranslateXProperty, value);
        }

        public static readonly DependencyProperty PanAreaThresholdProperty =
            DependencyProperty.Register(nameof(PanAreaThreshold), typeof(double),
                typeof(SwipeableSplitView), new PropertyMetadata(18d));

        public double PanAreaThreshold
        {
            get => (double) GetValue(PanAreaThresholdProperty);
            set => SetValue(PanAreaThresholdProperty, value);
        }

        public static readonly DependencyProperty IsPanSelectorEnabledProperty =
            DependencyProperty.Register(nameof(IsPanSelectorEnabled), typeof(bool),
                typeof(SwipeableSplitView), new PropertyMetadata(false));

        public bool IsPanSelectorEnabled
        {
            get => (bool) GetValue(IsPanSelectorEnabledProperty);
            set => SetValue(IsPanSelectorEnabledProperty, value);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PaneRoot = GetTemplateChild<Grid>("PaneRoot");
            _overlayRoot = GetTemplateChild<Grid>("OverlayRoot");
            PanArea = GetTemplateChild<Rectangle>("PanArea");
            DismissLayer = GetTemplateChild<Rectangle>("DismissLayer");

            var rootGrid = _paneRoot.FindAscendant<Grid>();

            OpenSwipeablePaneAnimation = GetStoryboard(rootGrid, "OpenSwipeablePane");
            CloseSwipeablePaneAnimation = GetStoryboard(rootGrid, "CloseSwipeablePane");

            OnDisplayModeChanged(null, null);
            RegisterPropertyChangedCallback(DisplayModeProperty, OnDisplayModeChanged);
        }

        private void OnDisplayModeChanged(DependencyObject sender, DependencyProperty dp)
        {
            switch (DisplayMode)
            {
                case SplitViewDisplayMode.Inline:
                case SplitViewDisplayMode.CompactOverlay:
                case SplitViewDisplayMode.CompactInline:
                    PanAreaInitialTranslateX = 0d;
                    _overlayRoot.Visibility = Visibility.Collapsed;
                    break;

                case SplitViewDisplayMode.Overlay:
                    PanAreaInitialTranslateX = OpenPaneLength * -1;
                    _overlayRoot.Visibility = Visibility.Visible;
                    break;
            }

            if (_paneRoot.RenderTransform != null)
                ((CompositeTransform) _paneRoot.RenderTransform).TranslateX = PanAreaInitialTranslateX;
        }

        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _panAreaTransform = (CompositeTransform) PanArea.RenderTransform;
            _paneRootTransform = (CompositeTransform) PaneRoot.RenderTransform;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var x = _panAreaTransform.TranslateX + e.Delta.Translation.X;
            if (x < PanAreaInitialTranslateX || x > 0) return;

            _paneRootTransform.TranslateX = _panAreaTransform.TranslateX = x;

            if (sender == _paneRoot && IsPanSelectorEnabled)
            {
                foreach (var item in _menuItems)
                    VisualStateManager.GoToState(item, "Normal", true);

                _toBeSelectedIndex = (int) Math.Round(
                    (e.Cumulative.Translation.Y + _startingDistance) / _distancePerItem,
                    MidpointRounding.AwayFromZero);

                if (_toBeSelectedIndex < 0)
                    _toBeSelectedIndex = 0;
                else if (_toBeSelectedIndex >= _menuItems.Count)
                    _toBeSelectedIndex = _menuItems.Count - 1;

                var itemContainer = _menuItems[_toBeSelectedIndex];
                VisualStateManager.GoToState(itemContainer, "PointerOver", true);
            }
        }

        private async void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var x = e.Velocities.Linear.X;
            if (x <= -0.1)
            {
                CloseSwipeablePane();
            }
            else if (x > -0.1 && x < 0.1)
            {
                if (Math.Abs(_panAreaTransform.TranslateX) > Math.Abs(PanAreaInitialTranslateX) / 2)
                {
                    CloseSwipeablePane();
                }
                else
                {
                    OpenSwipeablePane();
                }
            }
            else
            {
                OpenSwipeablePane();
            }

            if (IsPanSelectorEnabled)
            {
                if (sender == _paneRoot)
                {
                    if (Math.Abs(e.Velocities.Linear.Y) >= 2 ||
                        Math.Abs(e.Cumulative.Translation.X) > Math.Abs(e.Cumulative.Translation.Y))
                    {
                        foreach (var item in _menuItems)
                            VisualStateManager.GoToState(item, "Normal", true);

                        return;
                    }

                    foreach (var item in _menuItems)
                        VisualStateManager.GoToState(item, "Unselected", true);

                    var itemContainer = _menuItems[_toBeSelectedIndex];
                    VisualStateManager.GoToState(itemContainer, "Selected", true);

                    await Task.Delay(250);
                    _menuHost.SelectedIndex = _toBeSelectedIndex;
                }
                else
                {
                    _startingDistance = _distancePerItem * _menuHost.SelectedIndex;
                }
            }
        }

        private void OnPaneRootLoaded(object sender, RoutedEventArgs e)
        {
            if (!IsPanSelectorEnabled) return;

            var border = (Border) PaneRoot.Children[0];
            _menuHost = border.FindDescendant<Selector>();

            if (_menuHost.Items != null)
            {
                foreach (var item in _menuHost.Items)
                {
                    var container = (SelectorItem) _menuHost.ContainerFromItem(item);
                    _menuItems.Add(container);
                }
            }

            _distancePerItem = TotalPanningDistance / _menuItems.Count;
            _startingDistance = _distancePerItem * _menuHost.SelectedIndex;
        }

        private void OpenSwipeablePane()
        {
            if (IsSwipeablePaneOpen) OpenSwipeablePaneAnimation.Begin();
            else IsSwipeablePaneOpen = true;
        }

        private void CloseSwipeablePane()
        {
            if (!IsSwipeablePaneOpen) CloseSwipeablePaneAnimation.Begin();
            else IsSwipeablePaneOpen = false;
        }

        private void OnDismissLayerTapped(object sender, TappedRoutedEventArgs e) => CloseSwipeablePane();

        private void OnOpenSwipeablePaneCompleted(object sender, object e) => DismissLayer.IsHitTestVisible = true;

        private void OnCloseSwipeablePaneCompleted(object sender, object e) => DismissLayer.IsHitTestVisible = false;

        private T GetTemplateChild<T>(string name) where T : DependencyObject => (T) GetTemplateChild(name);

        private static Storyboard GetStoryboard(FrameworkElement e, string name) => (Storyboard) e.Resources[name];
    }
}