using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using PokerTracker3000.Common;
using PokerTracker3000.ViewModels;
using PokerTracker3000.WpfComponents;

namespace PokerTracker3000
{
    public partial class MainWindow : Window
    {
        public MainWindowViewModel ViewModel
        {
            get => (MainWindowViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            nameof(ViewModel),
            typeof(MainWindowViewModel),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(null));

        public bool SideMenuOpen
        {
            get => (bool)GetValue(SideMenuOpenProperty);
            set => SetValue(SideMenuOpenProperty, value);
        }
        public static readonly DependencyProperty SideMenuOpenProperty = DependencyProperty.Register(
            nameof(SideMenuOpen),
            typeof(bool),
            typeof(MainWindow),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));


        private enum CurrentFocusArea
        {
            None,
            SideMenu,
            Players,
            BottomPanel
        }

        private const string SettingsFileName = "Settings.json";
        private CurrentFocusArea _lastFocusArea = CurrentFocusArea.None;
        private CurrentFocusArea _currentFocusArea = CurrentFocusArea.None;

        public MainWindow()
        {
            InitializeComponent();
            Settings.Initalize(SettingsFileName);

            ViewModel = new(Settings.App);

            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            MaxWidth = SystemParameters.MaximizedPrimaryScreenWidth;

            var verticalMargin = MaxHeight - Height;
            Top = verticalMargin / 4;
        }

        private void TitleBarButtonPressed(object sender, RoutedEventArgs e)
        {
            if (e is TitleBarButtonClickedEventArgs eventArgs)
            {
                switch (eventArgs.ButtonClicked)
                {
                    case TitleBarButton.Minimize:
                        WindowState = WindowState.Minimized;
                        break;
                    case TitleBarButton.Maximize:
                        WindowState = WindowState.Maximized;
                        break;
                    case TitleBarButton.Restore:
                        WindowState = WindowState.Normal;
                        break;
                    case TitleBarButton.Close:
                        ViewModel.NotifyWindowClosed();
                        Close();
                        break;
                };
            }
        }

        private void KeyDownInWindow(object sender, KeyEventArgs e)
        {
            var inputEvent = Input.InputManager.GetUserInputEventFromKeyboard(e);

            if (inputEvent.IsButtonPressed)
                HandleButtonPressedEvent(inputEvent.Button);
            else if (inputEvent.IsNavigation)
                HandleNavigationEvent(inputEvent.Direction);
        }

        private void HandleButtonPressedEvent(Input.InputManager.UserInputEvent.ButtonPressed button)
        {
            if (button == Input.InputManager.UserInputEvent.ButtonPressed.Start)
            {
                SideMenuOpen = !SideMenuOpen;
                if (SideMenuOpen)
                {
                    _lastFocusArea = _currentFocusArea;
                    _currentFocusArea = CurrentFocusArea.SideMenu;
                }
                else
                {
                    _currentFocusArea = _lastFocusArea;
                    if (_currentFocusArea != CurrentFocusArea.None)
                        ShowFocusHighlightInArea(_currentFocusArea);
                }
            }
            else
            {
                // TODO
            }
        }

        private void HandleNavigationEvent(Input.InputManager.UserInputEvent.NavigationDirection direction)
        {
            var playerViews = GetPlayerViews();
            if (_currentFocusArea == CurrentFocusArea.None)
            {
                _currentFocusArea = CurrentFocusArea.Players;
                playerViews.First().IsHighlighted = true;
                return;
            }

            if (_currentFocusArea == CurrentFocusArea.SideMenu)
            {
                // Handle navigation in side menu
            }
            else
            {
                if (direction == Input.InputManager.UserInputEvent.NavigationDirection.Up ||
                    direction == Input.InputManager.UserInputEvent.NavigationDirection.Down)
                {
                    // TODO
                }
                else
                {
                    if (_currentFocusArea == CurrentFocusArea.Players)
                    {

                    }
                }
            }
        }

        private void ShowFocusHighlightInArea(CurrentFocusArea currentFocusArea) => throw new NotImplementedException();

        private List<Player> GetPlayerViews()
        {
            List<Player> playerViews = new();
            var container = ExtensionMethods.TryFindChild<UniformGrid>(playersOverview);

            if (container == default)
                return playerViews;

            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(container);  i++)
            {
                var p = ExtensionMethods.TryFindChild<Player>(VisualTreeHelper.GetChild(container, i));
                if (p != default)
                    playerViews.Add(p);
            }
            return playerViews;
        }
    }
}
